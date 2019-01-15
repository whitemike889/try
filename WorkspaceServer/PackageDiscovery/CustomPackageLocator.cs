using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    internal class CustomPackageLocator
    {
        readonly DirectoryInfo _workingDirectory;

        public CustomPackageLocator(DirectoryInfo workingDirectory)
        {
            _workingDirectory = workingDirectory;
        }

        public async Task<Package> LocatePackageAsync(string name, Budget budget)
        {
            var fileName = Path.Combine(_workingDirectory.FullName, name);
            CommandLineResult result;
            try
            {
                result = await CommandLine.Execute(fileName, "locate-assembly", budget: budget);

            }
            catch (System.ComponentModel.Win32Exception)
            {
                return null;
            }

            var output = result.Output.FirstOrDefault();
            if (output == null || !File.Exists(output))
            {
                return null;
            }

            var directory = Path.GetDirectoryName(output);
            var projectDirectory = Path.Combine(directory, "project");
            Console.WriteLine($"Project: {projectDirectory}");
            var package = new Package(name, directory: new DirectoryInfo(projectDirectory));
            return package;
        }
    }
}
