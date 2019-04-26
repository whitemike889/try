using System;
using WorkspaceServer;

namespace MLS.Agent.Tests
{
    public class FakeBrowserLauncher : IBrowserLauncher
    {
        public IDirectoryAccessor DirectoryAccessor { get; }

        public FakeBrowserLauncher(IDirectoryAccessor directoryAccessor)
        {
            DirectoryAccessor = directoryAccessor;
        }

        public void LaunchBrowser(Uri uri)
        {
            LaunchedUri = uri;
        }

        public Uri LaunchedUri { get; private set; }
    }
}