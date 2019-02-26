using System;

namespace MLS.Agent.Tests
{
    public class FakeBrowerLauncher : IBrowserLauncher
    {
        public void LaunchBrowser(Uri uri)
        {
            LaunchedUri = uri;
        }

        public Uri LaunchedUri { get; private set; }
    }
}