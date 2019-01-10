using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace MLS.PackageTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var rootCommand = new RootCommand
                              {
                                  LocateAssembly(),
                                  ExtractPackage(),
                              };

            var parser = new CommandLineBuilder(rootCommand)
                         .UseDefaults()
                         .Build();

            await parser.InvokeAsync(args);

            Command LocateAssembly()
            {
                return new Command("locate-assembly")
                       {
                           Handler = CommandHandler.Create((IConsole console) => console.Out.WriteLine(AssemblyLocation()))
                       };
            }

            Command ExtractPackage()
            {
                return new Command("extract-package", "Extracts the project package zip thingz0rz.")
                       {
                           Handler = CommandHandler.Create(async (IConsole console) =>
                           {
                           var directory = Path.GetDirectoryName(AssemblyLocation());
                               var zipFilePath = Path.Combine(directory, "project.zip");

                               var resource = typeof(Program).Assembly.GetManifestResourceNames()[0];

                               using (var stream = typeof(Program).Assembly.GetManifestResourceStream(resource))
                               {
                                   using (var zipFileStream = File.OpenWrite(zipFilePath))
                                   {
                                       await stream.CopyToAsync(zipFileStream);
                                       await zipFileStream.FlushAsync();
                                   }
                               }

                               ZipFile.ExtractToDirectory(zipFilePath, Path.Combine(directory, "project"));
                           })
                       };
            }
        }

        private static string AssemblyLocation()
        {
            return typeof(Program).Assembly.Location;
        }
    }
}