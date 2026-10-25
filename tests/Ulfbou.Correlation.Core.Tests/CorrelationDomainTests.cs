using System.Text;
using System.Text.Json;
using FluentAssertions;
using Ulfbou.Correlation.Core;
using Xunit;

namespace Ulfbou.Correlation.Core.Tests;

public sealed class CorrelationDomainTests
{
    [Fact]
    public void Canonical_golden_vector_matches_exact_bytes_and_digest()
    {
        using var fixture = JsonDocument.Parse(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "fixtures", "canonical", "correlation-state-canonical-vectors-v1.json")));
        var vector = fixture.RootElement.GetProperty("vectors")[0];
        var input = Encoding.UTF8.GetBytes(vector.GetProperty("input").GetRawText());
        var bytes = CanonicalJson.Canonicalize(input);
        Encoding.UTF8.GetString(bytes).Should().Be(vector.GetProperty("canonicalUtf8").GetString());
        Convert.ToHexString(bytes).ToLowerInvariant().Should().Be(vector.GetProperty("canonicalHex").GetString());
        CanonicalJson.Identity(bytes).Should().Be(vector.GetProperty("sha256").GetString());
    }

    [Theory]
    [InlineData("""{"a":1,"a":2}""", "CANON-DUPLICATE")]
    [InlineData("""{"n":1.5}""", "CANON-NUMBER")]
    [InlineData("""{"n":-0}""", "CANON-NUMBER")]
    [InlineData("""{"n":null}""", "CANON-NULL")]
    public void Invalid_canonical_domains_fail_closed(string json, string code)
    {
        var action = () => CanonicalJson.Canonicalize(Encoding.UTF8.GetBytes(json));
        action.Should().Throw<CorrelationFailure>().Which.Code.Should().Be(code);
    }

    [Fact]
    public void Success_and_cancellation_paths_enforce_terminality()
    {
        var state = CorrelationStateMachine.Next(CorrelationState.Created, CorrelationSignal.DispatchStarted);
        state = CorrelationStateMachine.Next(state, CorrelationSignal.ProviderAccepted);
        var cancelled = CorrelationStateMachine.Next(state, CorrelationSignal.CancelAccepted);
        CorrelationStateMachine.IsTerminal(cancelled).Should().BeTrue();
        var late = () => CorrelationStateMachine.Next(cancelled, CorrelationSignal.ValidProviderResult);
        late.Should().Throw<CorrelationFailure>().Which.Code.Should().Be("STATE-TERMINAL");

        state = CorrelationStateMachine.Next(CorrelationState.AwaitingProvider, CorrelationSignal.ValidProviderResult);
        CorrelationStateMachine.Next(state, CorrelationSignal.ResultNormalized).Should().Be(CorrelationState.Completed);
    }

    [Fact]
    public void Revision_advances_only_from_exact_observation()
    {
        CorrelationStateMachine.NextRevision(3, 3).Should().Be(4);
        var stale = () => CorrelationStateMachine.NextRevision(4, 3);
        stale.Should().Throw<CorrelationFailure>().Which.Code.Should().Be("STATE-REVISION");
    }

    [Fact]
    public void Deletion_requires_exact_revision_digest_identity_and_version()
    {
        const string sha = "sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        var request = new DeletionRequest("corr-example", 5, sha, "correlation-state-canonical-json-v1");
        DeletionAuthorizer.Authorize(request, new(ActiveStatePresence.Present, "corr-example", 5, sha, "correlation-state-canonical-json-v1", "v5")).Should().Be(DeletionDecision.DeleteConditionally);
        DeletionAuthorizer.Authorize(request, new(ActiveStatePresence.Present, "corr-example", 6, sha, "correlation-state-canonical-json-v1", "v6")).Should().Be(DeletionDecision.RevisionConflict);
        DeletionAuthorizer.Authorize(request, new(ActiveStatePresence.Absent)).Should().Be(DeletionDecision.AlreadyAbsent);
        DeletionAuthorizer.DeriveKey("corr-example").Should().Be("active/v1/co/corr-example.json");
    }
}
