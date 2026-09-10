using AwesomeAssertions.Execution;
using Xunit;

namespace AwesomeAssertions.Specs.Execution;

public partial class AssertionChainSpecs
{
    public class ForFailingAssertion
    {
        [Fact]
        public void A_nested_assertion_cannot_leave_a_chain_behind_to_be_reused()
        {
            AssertionChain chainToBeReused = AssertionChain.GetOrCreate();

            AssertionChain.GetOrCreate().ForFailingAssertion(() =>
            {
                var constraint = 5.Should().Be(6);

                // an assertion returning an AndWhichConstraint does this to amend the caller identifier
                chainToBeReused.ReuseOnce();

                return constraint;
            });

            AssertionChain.GetOrCreate().Should().NotBeSameAs(chainToBeReused,
                "the reuse requested inside the nested assertion must not affect the assertions that follow");
        }

        [Fact]
        public void A_nested_assertion_does_not_pick_up_a_chain_to_be_reused()
        {
            AssertionChain assertionChain = AssertionChain.GetOrCreate();
            AssertionChain chainToBeReused = AssertionChain.GetOrCreate();
            chainToBeReused.ReuseOnce();

            AssertionChain nestedChain = null;

            assertionChain.ForFailingAssertion(() =>
            {
                nestedChain = AssertionChain.GetOrCreate();

                return 5.Should().Be(6);
            });

            nestedChain.Should().NotBeSameAs(chainToBeReused,
                "the nested assertion must not run on the chain that the assertion being built reserved");
        }

        [Fact]
        public void A_chain_to_be_reused_survives_a_nested_assertion()
        {
            AssertionChain assertionChain = AssertionChain.GetOrCreate();
            AssertionChain chainToBeReused = AssertionChain.GetOrCreate();
            chainToBeReused.ReuseOnce();

            assertionChain.ForFailingAssertion(() => 5.Should().Be(6));

            AssertionChain.GetOrCreate().Should().BeSameAs(chainToBeReused,
                "the nested assertion must not consume the reuse requested by the assertion that is being built");
        }
    }
}
