using System;
using System.Threading;
using System.Threading.Tasks;
using WorkspaceServer;

namespace MLS.Agent
{
    public class WarmUpWorkspaces : HostedService, IDisposable
    {
        private readonly WorkspaceServerRegistry workspaceServerRegistry;
        private Thread thread;

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

            thread = new Thread(() =>
            {
                workspaceServerRegistry.StartAllServers().Wait(cancellationToken);
            });

            thread.Start();
        }

        public void Dispose()
        {
        }
    }
}
