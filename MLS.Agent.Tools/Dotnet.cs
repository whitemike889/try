using System;
using System.IO;

namespace MLS.Agent.Tools
{
    public class Dotnet
    {
        private readonly TimeSpan _defaultCommandTimeout;
        private readonly DirectoryInfo _workingDirectory;

        public Dotnet(
            DirectoryInfo workingDirectory = null,
            TimeSpan? defaultCommandTimeout = null)
        {
            _defaultCommandTimeout = defaultCommandTimeout ??
                                     TimeSpan.FromSeconds(10);
            _workingDirectory = workingDirectory ??
                                new DirectoryInfo(Directory.GetCurrentDirectory());
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

        public CommandLineResult Run(TimeSpan? timeout = null) =>
            Execute("run", timeout);

        public CommandLineResult Execute(string args, TimeSpan? timeout = null)
        {
            timeout = timeout ?? _defaultCommandTimeout;

            return CommandLine.Execute(
                DotnetMuxer.Path,
                args,
                _workingDirectory,
                timeout);
        }
    }
}
