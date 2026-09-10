using System;
using System.Diagnostics.CodeAnalysis;
using AwesomeAssertions.Execution;
using Xunit;
using Xunit.Sdk;

namespace AwesomeAssertions.Specs.Primitives;

/// <content>
/// The [Not]StartWith specs.
/// </content>
public partial class StringAssertionSpecs
{
    public class StartWith
    {
        [Fact]
        public void When_asserting_string_starts_with_the_same_value_it_should_not_throw()
        {
            // Arrange
            string value = "ABC";

            // Act / Assert
            value.Should().StartWith("AB");
        }

        [Fact]
        public void When_expected_string_is_the_same_value_it_should_not_throw()
        {
            // Arrange
            string value = "ABC";

            // Act / Assert
            value.Should().StartWith(value);
        }

        [Fact]
        public void When_string_does_not_start_with_expected_phrase_it_should_throw()
        {
            // Act
            Action act = () => "ABC".Should().StartWith("ABB", "we want to test the {0} message", "failure");

            // Assert
            act.Should().Throw<XunitException>().WithMessage(
                """
                Expected string to start with the same string because*failure message, but they differ at index 2:
                     ↓ (actual)
                  "ABC"
                  "ABB"
                     ↑ (expected).
                """);
        }

        [Fact]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void
            When_string_does_not_start_with_expected_phrase_and_one_of_them_is_long_it_should_display_both_strings_on_separate_line()
        {
            // Act
            Action act = () => "ABCDEFGHI".Should().StartWith("ABCDDFGHI", "we want to test the {0} message", "failure");

            // Assert
            act.Should().Throw<XunitException>().WithMessage(
                """
                Expected string to start with the same string because*failure message, but they differ at index 4:
                       ↓ (actual)
                  "ABCDEFGHI"
                  "ABCDDFGHI"
                       ↑ (expected).
                """);
        }

        [Fact]
        public void When_string_start_is_compared_with_null_it_should_throw()
        {
            // Act
            Action act = () => "ABC".Should().StartWith(null);

            // Assert
            act.Should().Throw<ArgumentNullException>().WithMessage(
                "Cannot compare start of string with <null>.*");
        }

        [Fact]
        public void When_string_start_is_compared_with_empty_string_it_should_not_throw()
        {
            // Act / Assert
            "ABC".Should().StartWith("");
        }

        [Fact]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void When_string_start_is_compared_with_string_that_is_longer_it_should_throw()
        {
            // Act
            Action act = () => "ABC".Should().StartWith("ABCDEF");

            // Assert
            act.Should().Throw<XunitException>().WithMessage(
                """
                *index 3*
                      ↓ (actual)
                  "ABC"
                  "ABCDEF"
                      ↑ (expected).
                """);
        }

        [Fact]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void Correctly_stop_further_execution_when_inside_assertion_scope()
        {
            // Act
            Action act = () =>
            {
                using var _ = new AssertionScope();
                "ABC".Should().StartWith("ABCDEF").And.StartWith("FEDCBA");
            };

            // Assert
            act.Should().Throw<XunitException>().WithMessage(
                "*\"ABCDEF\"*");
        }

        [Fact]
        public void When_string_start_is_compared_and_actual_value_is_null_then_it_should_throw()
        {
            // Act
            string someString = null;
            Action act = () => someString.Should().StartWith("ABC");

            // Assert
            act.Should().Throw<XunitException>()
                .WithMessage("Expected someString to start with \"ABC\", but found <null>.");
        }

        [Fact]
        public void When_mismatch_outside_subject_bounds_arrows_are_still_aligned()
        {
            // Act
            Action act = () => "A".Should().StartWith("ABCDEFGH");

            // Assert
            act.Should().Throw<XunitException>()
                .WithMessage(
                    """
                    *at index 1*
                        ↓ (actual)
                      "A"
                      "ABCDEFGH"
                        ↑ (expected).
                    """
                );
        }

        [Fact]
        public void When_mismatch_within_subject_bounds_arrows_are_aligned()
        {
            // Act
            Action act = () => "AFoo".Should().StartWith("ABCDEFGH");

            // Assert
            act.Should().Throw<XunitException>()
                .WithMessage(
                    """
                    *at index 1*
                        ↓ (actual)
                      "AFoo"
                      "ABCDEFGH"
                        ↑ (expected).
                    """
                );
        }
    }

    public class NotStartWith
    {
        [Fact]
        public void When_asserting_string_does_not_start_with_a_value_and_it_does_not_it_should_succeed()
        {
            // Arrange
            string value = "ABC";

            // Act / Assert
            value.Should().NotStartWith("DE");
        }

        [Fact]
        public void When_asserting_string_does_not_start_with_a_value_but_it_does_it_should_fail_with_a_descriptive_message()
        {
            // Arrange
            string value = "ABC";

            // Act
            Action action = () =>
                value.Should().NotStartWith("AB", "we want to test the {0} message", "failure");

            // Assert
            action.Should().Throw<XunitException>().WithMessage(
                "Expected value not to start with \"AB\" because*failure message, but found \"ABC\".");
        }

        [Fact]
        public void When_asserting_string_does_not_start_with_a_value_that_is_null_it_should_throw()
        {
            // Arrange
            string value = "ABC";

            // Act
            Action action = () =>
                value.Should().NotStartWith(null);

            // Assert
            action.Should().Throw<ArgumentNullException>().WithMessage(
                "Cannot compare start of string with <null>.*");
        }

        [Fact]
        public void When_asserting_string_does_not_start_with_a_value_that_is_empty_it_should_throw()
        {
            // Arrange
            string value = "ABC";

            // Act
            Action action = () =>
                value.Should().NotStartWith("");

            // Assert
            action.Should().Throw<XunitException>().WithMessage(
                "Expected value not to start with \"\", but found \"ABC\".");
        }

        [Fact]
        public void When_asserting_string_does_not_start_with_a_value_and_actual_value_is_null_it_should_throw()
        {
            // Act
            string someString = null;
            Action act = () => someString.Should().NotStartWith("ABC");

            // Assert
            act.Should().Throw<XunitException>().WithMessage(
                "Expected someString not to start with \"ABC\", but found <null>.");
        }

        [Fact]
        public void When_asserting_string_does_not_start_with_a_value_and_also_with_a_value_it_actually_starts_it_should_throw()
        {
            // Act
            const string someString = "ABC";
            Action act = () => someString.Should().NotStartWith("XYZ").And.NotStartWith("ABC");

            // Assert
            act.Should().Throw<XunitException>().WithMessage(
                "Expected someString not to start with \"ABC\", but found \"ABC\".");
        }
    }
}
