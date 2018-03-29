using System.Threading.Tasks;
using Clockwise;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;
using static WorkspaceServer.Servers.WorkspaceServer;

namespace MLS.Agent.Tests
{
    public class ExceptionExtensionsTests
    {
        [Fact]
        public async Task When_time_budget_expires_in_user_code_then_a_417_is_returned()
        {
            using (VirtualClock.Start())
            {
                var budget = new TimeBudget(10.Seconds());

                await Clock.Current.Wait(11.Seconds());

                budget.RecordEntry(UserCodeCompletedBudgetEntryName);

                var exception = new BudgetExceededException(budget);

                exception.ToHttpStatusCode().Should().Be(417);
            }
        }

        [Fact]
        public async Task When_time_budget_expires_prior_to_user_code_then_a_504_is_returned()
        {
            using (VirtualClock.Start())
            {
                var budget = new TimeBudget(10.Seconds());

                await Clock.Current.Wait(11.Seconds());

                var exception = new BudgetExceededException(budget);

                exception.ToHttpStatusCode().Should().Be(504);
            }
        }
    }
}
