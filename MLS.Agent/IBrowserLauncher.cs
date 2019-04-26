using System;
using WorkspaceServer;

namespace MLS.Agent
{
    public interface IBrowserLauncher
    {
        void LaunchBrowser(Uri uri);
        IDirectoryAccessor DirectoryAccessor { get; }
    }
}