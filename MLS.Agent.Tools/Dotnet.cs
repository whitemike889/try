using System;
using System.IO;
using Clockwise;

namespace MLS.Agent.Tools
{
    public class Dotnet
    {
        private readonly DirectoryInfo _workingDirectory;

        public Dotnet(DirectoryInfo workingDirectory = null)
        {
            _workingDirectory = workingDirectory ??
                                new DirectoryInfo(Directory.GetCurrentDirectory());
        }

        public CommandLineResult New(string templateName, string args = null, TimeBudget budget = null)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(templateName));
            }

            return Execute($"new {templateName} {args}", budget);
        }

        public CommandLineResult Build(string args = null, TimeBudget budget = null) =>
            Execute("build".AppendArgs(args), budget);

        public CommandLineResult Execute(string args, TimeBudget budget = null) =>
            CommandLine.Execute(
                DotnetMuxer.Path,
                args,
                _workingDirectory,
                budget);
    }
}
