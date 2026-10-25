using FluentAssertions;using Ulfbou.Correlation.Core;using Ulfbou.Correlation.Runtime;using Xunit;
namespace Ulfbou.Correlation.Runtime.Tests;
public sealed class RuntimeBoundaryTests
{
 [Fact] public void Unknown_property_fails_closed(){var a=()=>StrictJson.Read<CreateRequest>("""{"schemaVersion":"correlation-create-request-v1","unexpected":1}""",8192,new HashSet<string>{"schemaVersion"});a.Should().Throw<CorrelationFailure>().Which.Code.Should().Be("HTTP-PROPERTY");}
 [Fact] public void Oversized_body_fails_before_parsing(){var a=()=>StrictJson.Read<CreateRequest>(new string('x',8193),8192,new HashSet<string>());a.Should().Throw<CorrelationFailure>().Which.Code.Should().Be("HTTP-OVERSIZED");}
 [Fact] public void Limits_are_contractual(){RuntimeLimits.CleanupEntries.Should().Be(25);RuntimeLimits.CleanupRequestBytes.Should().Be(128*1024);RuntimeLimits.ProviderResponseBytes.Should().Be(16*1024);}
}
