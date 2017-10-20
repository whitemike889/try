using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
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
                                     TimeSpan.FromSeconds(10);
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

            var timeout = timeoutMilliseconds ??
                          (int) _defaultCommandTimeout.TotalMilliseconds;

            using (var operation = LogConfirm(dotnetPath, args))
            {
                var stdOut = new StringBuilder();
                var stdErr = new StringBuilder();

                using (var process = new Process())
                {
                    process.StartInfo.Arguments = args;
                    process.StartInfo.FileName = dotnetPath;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.WorkingDirectory = _workingDirectory.FullName;

                    process.OutputDataReceived += (_, e) =>
                    {
                        if (e.Data != null)
                        {
                            stdOut.AppendLine(e.Data);
                        }
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data != null)
                        {
                            stdErr.AppendLine(e.Data);
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    Exception exception = null;

                    if (process.WaitForExit(timeout))
                    {
                        operation.Succeed("dotnet.exe exited with {code}", process.ExitCode);
                    }
                    else
                    {
                        exception = new TimeoutException();
                        process.Kill();
                        operation.Fail(exception);
                    }

                    return new RunResult(
                        succeeded: process.HasExited && process.ExitCode == 0,
                        output: $"{stdOut}\n{stdErr}"
                            .Replace("\r\n", "\n")
                            .Split('\n'),
                        exception: exception?.ToString());
                }
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
