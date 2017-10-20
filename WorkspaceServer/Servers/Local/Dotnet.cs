using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Pocket;
using WorkspaceServer.Models.Execution;
using static Pocket.Logger<WorkspaceServer.Servers.Local.Dotnet>;

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
                                     TimeSpan.FromSeconds(30);
            _workingDirectory = workingDirectory ??
                                throw new ArgumentNullException(nameof(workingDirectory));
        }

        public void New(string templateName, int? timeoutMilliseconds = null)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(templateName));
            }

            ExecuteDotnet($"new {templateName}", timeoutMilliseconds);
        }

        public void Restore(int? timeoutMilliseconds = null) => 
            ExecuteDotnet("restore", timeoutMilliseconds);

        public RunResult Run(int? timeoutMilliseconds = null) => 
            ExecuteDotnet("run", timeoutMilliseconds);

        public RunResult ExecuteDotnet(string args, int? timeoutMilliseconds = null)
        {
            var dotnetPath = DotnetMuxer.Path.FullName;

            using (var operation = LogConfirm(dotnetPath, args))
            using (var process = Process.Start(new ProcessStartInfo
            {
                Arguments = args,
                FileName = dotnetPath,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                WorkingDirectory = _workingDirectory.FullName
            }))
            {
                if (!process.WaitForExit(timeoutMilliseconds ??
                                         (int) _defaultCommandTimeout.TotalMilliseconds))
                {
                    operation.Fail(message: "Timed out");
                    throw new TimeoutException("Timed out waiting for dotnet.exe.");
                }

                operation.Trace("dotnet.exe exited with {code}", process.ExitCode);

                var stdOut = SplitLines(process.StandardOutput);

                var stdErr = SplitLines(process.StandardError);

                operation.Succeed();

                return new RunResult
                (
                    succeeded: process.ExitCode == 0,
                    output: stdOut.Concat(stdErr).ToArray()
                );
            }
        }

        private static ConfirmationLogger LogConfirm(string dotnetPath, string args) => new ConfirmationLogger(
            category: Log.Category,
            message: "Invoking {dotnet} {args}",
            args: new object[] { dotnetPath, args },
            logOnStart: true);

        private static IReadOnlyCollection<string> SplitLines(StreamReader reader) =>
            reader.ReadToEnd()
                  .Replace("\r\n", "\n")
                  .Split('\n');
    }
}
