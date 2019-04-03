using System;

namespace WorkspaceServer
{
    public class PackageNotFoundException : Exception
    {
        public PackageNotFoundException(string message):base(message)
        {
        }
    }
}