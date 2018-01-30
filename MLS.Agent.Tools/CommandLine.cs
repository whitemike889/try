using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
            CancellationToken? cancellationToken = null) =>
            Execute(exePath.FullName,
                    args,
                    workingDir,
                    cancellationToken);

        public static CommandLineResult Execute(
            string command,
            string args,
            DirectoryInfo workingDir = null,
            CancellationToken? cancellationToken = null)
        {
            args = args ?? "";
            cancellationToken = cancellationToken ?? CancellationToken.None;

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
                (int exitCode, Exception exception) =
                    Task.Run(() =>
                        {
                            process.WaitForExit();

                            operation.Info("PROCESS EXITED: {command} {args}", command, args);

                            operation.Succeed(
                                "{command} {args} exited with {code}",
                                command,
                                args,
                                process.ExitCode);

                            return (process.ExitCode, (Exception) null);
                        })
                        .CancelAfter(
                            cancellationToken.Value,
                            ifCancelled: () =>
                            {
                                operation.Info("TIMEOUT CALLED: {command} {args}", command, args);

                                var ex = new TimeoutException();

                                Task.Run(() =>
                                {
                                    if (!process.HasExited)
                                    {
                                        process.Kill();
                                    }
                                }).DontAwait();

                                operation.Fail(ex);

                                return (124, ex); // like the Linux timeout command 
                            }).Result;

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
            string args,
            [CallerMemberName] string operationName = null) => new ConfirmationLogger(
            operationName: operationName,
            category: Logger.Log.Category,
            message: "Invoking {command} {args}",
            args: new[] { command, args },
            logOnStart: true);
    }
}
