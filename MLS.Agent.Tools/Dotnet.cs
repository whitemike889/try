using System;
using System.IO;
using System.Threading;
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

        public CommandLineResult New(string templateName, string args = null, CancellationToken? cancellationToken = null)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(templateName));
            }

            return Execute($"new {templateName} {args}",
                           cancellationToken ??
                           Clock.Current.CreateCancellationToken(TimeSpan.FromSeconds(10)));
        }

        public CommandLineResult Build(CancellationToken? cancellationToken = null) =>
            Execute("build",
                    cancellationToken ??
                    Clock.Current.CreateCancellationToken(TimeSpan.FromSeconds(20)));

        public CommandLineResult Execute(string args, CancellationToken? cancellationToken = null) =>
            CommandLine.Execute(
                DotnetMuxer.Path,
                args,
                _workingDirectory,
                cancellationToken ??
                Clock.Current.CreateCancellationToken(TimeSpan.FromSeconds(10)));
    }
}
