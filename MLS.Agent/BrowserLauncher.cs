using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using WorkspaceServer;

namespace MLS.Agent
{
    internal class BrowserLauncher : IBrowserLauncher
    {
        public IDirectoryAccessor DirectoryAccessor { get; }

        public BrowserLauncher(IDirectoryAccessor directoryAccessor)
        {
            DirectoryAccessor = directoryAccessor;
        }

        public void LaunchBrowser(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}"));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", uri.ToString());
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", uri.ToString());
            }
        }
    }
}