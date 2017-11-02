using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using External;
using Pocket;
using Recipes;
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

        public void New(string templateName, TimeSpan? timeout = null)
        {
            if (string.IsNullOrWhiteSpace(templateName))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(templateName));
            }

            ExecuteDotnet($"new {templateName}", timeout);
        }

        public void Restore(TimeSpan? timeout = null) =>
            ExecuteDotnet("restore", timeout);

        public RunResult Run(TimeSpan? timeout = null) =>
            ExecuteDotnet("run", timeout);

        public RunResult ExecuteDotnet(string args, TimeSpan? timeout = null)
        {
            timeout = timeout ?? _defaultCommandTimeout;

            var dotnetPath = DotnetMuxer.Path.FullName;

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

                    if (process.WaitForExit((int) timeout.Value.TotalMilliseconds))
                    {
                        operation.Succeed("dotnet.exe exited with {code}", process.ExitCode);
                    }
                    else
                    {
                        exception = new TimeoutException();
                        Task.Run(() => process.KillTree(1000)).DontAwait();
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
    }
}
