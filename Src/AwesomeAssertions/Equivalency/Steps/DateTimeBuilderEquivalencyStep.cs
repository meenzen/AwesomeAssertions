using System.Xml.Linq;
using AwesomeAssertions.Builders;
using AwesomeAssertions.Execution;

namespace AwesomeAssertions.Equivalency.Steps;

/// <summary>
/// Handles the equivalency of a <see cref="DateTimeBuilder"/> expectation with a subject value.
/// </summary>
public sealed class DateTimeBuilderEquivalencyStep : IEquivalencyStep
{
    /// <inheritdoc />
    public EquivalencyResult Handle(
        Comparands comparands,
        IEquivalencyValidationContext context,
        IValidateChildNodeEquivalency valueChildNodes)
    {
        if (comparands.Expectation is not DateTimeBuilder builder)
        {
            return EquivalencyResult.ContinueWithNext;
        }

        AssertionChain.GetOrCreate().For(context).ReuseOnce();

        builder.Should().Be(comparands.Subject, context.Reason.FormattedMessage, context.Reason.Arguments);
        return EquivalencyResult.EquivalencyProven;
    }
}
