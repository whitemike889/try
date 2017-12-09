using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using WorkspaceServer.Servers.Scripting;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class ConsoleRedirectionTests
    {
        [Fact]
        public async Task StandardOutput_is_captured()
        {
            using (var console = await RedirectConsoleOutput.Acquire())
            {
                Console.WriteLine("hello");

                console.StandardOutput.Should().Be("hello");
            }
        }

        [Fact]
        public async Task StandardError_is_captured()
        {
            using (var console = await RedirectConsoleOutput.Acquire())
            {
                Console.Error.WriteLine("oops!");

                console.StandardError.Should().Be("oops!");
            }
        }

        [Fact]
        public async void Multiple_threads_each_capturing_console_dont_conflict()
        {
            const int PRINT_COUNT = 10;
            const int THREAD_COUNT = 10;
            var barrier = new Barrier(THREAD_COUNT);

            async Task ThreadWork(string toPrint)
            {
                barrier.SignalAndWait(1000 /*ms*/);
                using (var console = await RedirectConsoleOutput.Acquire())
                {
                    var builder = new StringBuilder();
                    for (var i = 0; i < PRINT_COUNT; i++)
                    {
                        Console.Write(toPrint);
                        builder.Append(toPrint);
                        await Task.Yield();
                    }

                    console.StandardOutput.Should().Be(builder.ToString());
                }
            }

            var threads = new List<Task>();
            for (var i = 0; i < THREAD_COUNT; i++)
            {
                threads.Add(Task.Run(async () => await ThreadWork($"hello from thread {i}!")));
            }

            foreach (var thread in threads)
            {
                await thread;
            }
        }
    }
}
