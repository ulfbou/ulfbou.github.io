using System.Text;
using FluentAssertions;
using Ulfbou.Correlation.Application;
using Ulfbou.Correlation.Aws;
using Ulfbou.Correlation.Core;
using Xunit;

namespace Ulfbou.Correlation.Aws.Tests;

public sealed class ActiveStateCodecTests
{
    private static ActiveCorrelation State => new(
        "corr-01jk6m2w8g6wmx9zq7v4c2ab", "op-01jk6m2w8g6wmx9zq7v4c2ac", "req-01jk6m2w8g6wmx9zq7v4c2ad",
        CorrelationState.AwaitingProvider, 3,
        new(2026,10,24,10,0,0,TimeSpan.Zero), new(2026,10,24,10,1,0,TimeSpan.Zero), new(2026,10,24,10,15,0,TimeSpan.Zero),
        "sha256:aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", "ignored-on-encode");

    [Fact]
    public void Round_trip_is_bounded_canonical_and_recomputes_digest()
    {
        var bytes = ActiveStateCodec.Encode(State);
        bytes.Length.Should().BeLessThanOrEqualTo(16 * 1024);
        bytes[^1].Should().NotBe((byte)'\n');
        var decoded = ActiveStateCodec.Decode(bytes);
        decoded.StateRevision.Should().Be(3);
        decoded.StateSha256.Should().Be(CanonicalJson.Identity(bytes));
    }

    [Fact]
    public void Oversized_state_fails_before_parsing()
    {
        var bytes = Encoding.UTF8.GetBytes("{" + new string('x', 17 * 1024) + "}");
        var action = () => ActiveStateCodec.Decode(bytes);
        action.Should().Throw<CorrelationFailure>().Which.Code.Should().Be("STORE-OVERSIZED");
    }

    [Fact]
    public void Delete_key_is_always_derived_from_correlation_identity()
    {
        DeletionAuthorizer.DeriveKey(State.CorrelationId).Should().Be("active/v1/co/corr-01jk6m2w8g6wmx9zq7v4c2ab.json");
    }
}
