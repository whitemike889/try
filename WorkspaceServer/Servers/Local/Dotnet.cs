using System;
using System.IO;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Servers.Local
{
    public class Dotnet
    {
        private readonly TimeSpan _defaultCommandTimeout;
        private readonly DirectoryInfo _workingDirectory;

        public Dotnet(
            DirectoryInfo workingDirectory,
            TimeSpan? defaultCommandTimeout = null)
        {
            _defaultCommandTimeout = defaultCommandTimeout ??
                                     TimeSpan.FromSeconds(10);
            _workingDirectory = workingDirectory ??
                                throw new ArgumentNullException(nameof(workingDirectory));
        }

        public void New(string templateName, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(templateName));
            }

            Execute($"new {templateName}", timeout);
        }

        public void Restore(TimeSpan? timeout = null) =>
            Execute("restore", timeout);

        public RunResult Run(TimeSpan? timeout = null) =>
            Execute("run", timeout);

        public RunResult Execute(string args, TimeSpan? timeout = null)
        {
            timeout = timeout ?? _defaultCommandTimeout;

            var exePath = DotnetMuxer.Path.FullName;

            return CommandLine.Execute(exePath,
                                       args,
                                       _workingDirectory.FullName,
                                       timeout);
        }
    }
}
