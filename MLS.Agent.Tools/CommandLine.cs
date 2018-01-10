using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pocket;
using Recipes;
using static Pocket.Logger<MLS.Agent.Tools.CommandLine>;

namespace MLS.Agent.Tools
{
    public static class CommandLine
    {
        public static CommandLineResult Execute(
            FileInfo exePath,
            string args,
            DirectoryInfo workingDir = null,
            TimeSpan? timeout = null) =>
            Execute(exePath.FullName,
                    args,
                    workingDir,
                    timeout);

        public static CommandLineResult Execute(
            string command,
            string args,
            DirectoryInfo workingDir = null,
            TimeSpan? timeout = null)
        {
            args = args ?? "";

            var stdOut = new StringBuilder();
            var stdErr = new StringBuilder();

            using (var operation = LogConfirm(command, args))
            using (var process = StartProcess(
                command,
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

                int exitCode;

                if (process.WaitForExit(timeoutMs))
                {
                    exitCode = process.ExitCode;

                    operation.Succeed(
                        "{command} exited with {code}",
                        command,
                        exitCode);
                }
                else
                {
                    exitCode = 124; // like the Linux timeout command 

                    exception = new TimeoutException();
                    Task.Run(() => process.Kill()).Timeout(TimeSpan.FromSeconds(1)).DontAwait();
                    operation.Fail(exception);
                }

                return new CommandLineResult(
                    exitCode: exitCode,
                    output: stdOut.Replace("\r\n", "\n").ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries),
                    error: stdErr.Replace("\r\n", "\n").ToString().Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries),
                    exception: exception);
            }
        }

        public static Process StartProcess(
            string command,
            string args,
            DirectoryInfo workingDir,
            Action<string> output = null,
            Action<string> error = null)
        {
            args = args ?? "";

            Log.Trace("{workingDir}> {command} {args}", workingDir == null
                                                            ? ""
                                                            : workingDir.FullName, command, args);

            var process = new Process
            {
                StartInfo =
                {
                    Arguments = args,
                    FileName = command,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    WorkingDirectory = workingDir?.FullName
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

        private static ConfirmationLogger LogConfirm(
            object command,
            string args) => new ConfirmationLogger(
            category: Logger.Log.Category,
            message: "Invoking {command} {args}",
            args: new[] { command, args },
            logOnStart: true);
    }
}
