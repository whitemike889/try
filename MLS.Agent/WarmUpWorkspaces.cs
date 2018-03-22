using System;
using System.Threading.Tasks;
using Clockwise;
using WorkspaceServer;

namespace MLS.Agent
{
    public class WarmUpWorkspaces : HostedService
    {
        private readonly WorkspaceServerRegistry workspaceServerRegistry;

        public WarmUpWorkspaces(WorkspaceServerRegistry workspaceServerRegistry)
        {
            this.workspaceServerRegistry = workspaceServerRegistry ??
                                           throw new ArgumentNullException(nameof(workspaceServerRegistry));
        }

        protected override async Task ExecuteAsync(Budget budget) =>
            await workspaceServerRegistry.StartAllServers(budget);
    }
}
