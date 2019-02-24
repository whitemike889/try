using System;

namespace MLS.Agent
{
    public interface IBrowserLauncher
    {
        void LaunchBrowser(Uri uri);
    }
}