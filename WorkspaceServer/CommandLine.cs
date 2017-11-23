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
            using (var operation = LogConfirm(exePath, args))
            using (var process = new Process())
            {
                var stdOut = new StringBuilder();
                var stdErr = new StringBuilder();

                process.StartInfo.Arguments = args;
                process.StartInfo.FileName = exePath;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WorkingDirectory = workingDir;

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

                if (process.WaitForExit((int) (timeout ?? TimeSpan.MaxValue).TotalMilliseconds))
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

        private static ConfirmationLogger LogConfirm(string exePath, string args) => new ConfirmationLogger(
            category: Logger<Dotnet>.Log.Category,
            message: "Invoking {dotnet} {args}",
            args: new object[] { exePath, args },
            logOnStart: true);
    }
}