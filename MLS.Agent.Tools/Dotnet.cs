using System;
using System.IO;
using System.Threading.Tasks;
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

        public Task<CommandLineResult> New(string templateName, string args = null, Budget budget = null)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(templateName));
            }

            return Execute($"new {templateName} {args}", budget);
        }

        public Task<CommandLineResult> Build(string args = null, Budget budget = null) =>
            Execute("build".AppendArgs(args), budget);

        public Task<CommandLineResult> Execute(string args, Budget budget = null) =>
            CommandLine.Execute(
                DotnetMuxer.Path,
                args,
                _workingDirectory,
                budget);
        public Task<CommandLineResult> Publish(string args, Budget budget = null) =>
            Execute("publish".AppendArgs(args), budget);
    }
}
