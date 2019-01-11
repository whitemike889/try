using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace MLS.PackageTool
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var parser = CommandLineParser.Create(
                (console) => LocateAssemblyHandler(console),
                (console) => ExtractPackageHandler(console));

            await parser.InvokeAsync(args);
        }

        public static async Task ExtractPackageHandler(IConsole console)
        { 
            var directory = AssemblyDirectory();
            var zipFilePath = Path.Combine(directory, "project.zip");

            File.Delete(zipFilePath);

            string targetDirectory = Path.Combine(directory, "project");
            try
            {
                Directory.Delete(targetDirectory, recursive: true);
            }
            catch (DirectoryNotFoundException)
            {
            }

            var resource = typeof(Program).Assembly.GetManifestResourceNames()[0];

            using (var stream = typeof(Program).Assembly.GetManifestResourceStream(resource))
            {
                using (var zipFileStream = File.OpenWrite(zipFilePath))
                {
                    await stream.CopyToAsync(zipFileStream);
                    await zipFileStream.FlushAsync();
                }
            }

            ZipFile.ExtractToDirectory(zipFilePath, targetDirectory);
            File.Delete(zipFilePath);
        }

        public static void LocateAssemblyHandler(IConsole console)
        {
            console.Out.WriteLine(AssemblyLocation());
        }

        public static string AssemblyLocation()
        {
            return typeof(Program).Assembly.Location;
        }

        public static string AssemblyDirectory()
        {
            return Path.GetDirectoryName(AssemblyLocation());
        }
    }


    public class CommandLineParser
    {
        public static Parser Create(Action<IConsole> getAssembly, Func<IConsole, Task> extract)
        {
            var rootCommand = new RootCommand
                              {
                                  LocateAssembly(),
                                  ExtractPackage(),
                              };

            var parser = new CommandLineBuilder(rootCommand)
                         .UseDefaults()
                         .Build();

            Command LocateAssembly()
            {
                return new Command("locate-assembly")
                {
                    Handler = CommandHandler.Create(getAssembly)
                };
            }

            Command ExtractPackage()
            {
                return new Command("extract-package", "Extracts the project package zip thingz0rz.")
                {
                    Handler = CommandHandler.Create(extract)
                };
            }

            return parser;
        }
    }
}