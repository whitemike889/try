using System;
using System.Threading;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer;

namespace MLS.Agent
{
    public class WarmUpWorkspaces : HostedService, IDisposable
    {
        private readonly WorkspaceServerRegistry workspaceServerRegistry;
        private Budget budget;

        public WarmUpWorkspaces(WorkspaceServerRegistry workspaceServerRegistry)
        {
            this.workspaceServerRegistry = workspaceServerRegistry ??
                                           throw new ArgumentNullException(nameof(workspaceServerRegistry));
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            budget = new Budget(cancellationToken);

            using (SchedulerContext.Establish(budget))
            {
                await workspaceServerRegistry.StartAllServers(budget);
            }
        }

        public void Dispose() => budget?.Cancel();
    }
}
