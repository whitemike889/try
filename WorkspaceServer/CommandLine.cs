using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using External;
using Pocket;
using Recipes;
using WorkspaceServer.Models.Execution;
using WorkspaceServer.Servers.Local;

namespace WorkspaceServer
{
    internal static class CommandLine
    {
        public static RunResult Execute(
            string exePath,
            string args,
            string workingDir,
            TimeSpan? timeout = null)
        {
            args = args ?? "";

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            using (var operation = LogConfirm(exePath, args))
            using (var process = StartProcess(
                exePath,
                args,
                workingDir,
                output: data =>
                {
                    stdOut.AppendLine(data);
                    operation.Info("{x}", data);
                },
                error: data =>
                {
                    stdErr.AppendLine(data);
                    operation.Error("{x}", args: data);
                }))
            {
                Exception exception = null;

                int timeoutMs = timeout.HasValue
                                    ? (int) timeout.Value.TotalMilliseconds
                                    : int.MaxValue;

                if (process.WaitForExit(timeoutMs))
                {
                    operation.Succeed(
                        "{exe} exited with {code}",
                        exePath,
                        process.ExitCode);
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

        public static Process StartProcess(
            string exePath,
            string args,
            string workingDir,
            Action<string> output = null,
            Action<string> error = null)
        {
            args = args ?? "";

            var process = new Process
            {
                StartInfo =
                {
                    Arguments = args,
                    FileName = exePath,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    WorkingDirectory = workingDir
                }
            };

            if (output != null)
            {
                process.OutputDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        output(eventArgs.Data);
                    }
                };
            }

            if (error != null)
            {
                process.ErrorDataReceived += (sender, eventArgs) =>
                {
                    if (eventArgs.Data != null)
                    {
                        error(eventArgs.Data);
                    }
                };
            }

            process.Start();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        private static ConfirmationLogger LogConfirm(string exePath, string args) => new ConfirmationLogger(
            category: Logger<Dotnet>.Log.Category,
            message: "Invoking {dotnet} {args}",
            args: new object[] { exePath, args },
            logOnStart: true);
    }
}
