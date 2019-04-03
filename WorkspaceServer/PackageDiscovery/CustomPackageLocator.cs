using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using WorkspaceServer.Packaging;

namespace WorkspaceServer.PackageDiscovery
{
    internal class ToolPackageLocator
    {
        readonly string _basePath;

        public ToolPackageLocator(string basePath)
        {
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public async Task<Package> LocatePackageAsync(string name, Budget budget)
        {
            var fileName = Path.Combine(_basePath, name);
            CommandLineResult result;
            try
            {
                result = await CommandLine.Execute(fileName, "extract-package", budget: budget);
                result = await CommandLine.Execute(fileName, "locate-projects", budget: budget);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                return null;
            }

            var output = result.Output.FirstOrDefault();
            if (output == null || !Directory.Exists(output))
            {
                return null;
            }

            var directory = Path.GetDirectoryName(output);
            var projectDirectory = Path.Combine(directory, "packTarget");
            Console.WriteLine($"Project: {projectDirectory}");
            var package = new NonrebuildablePackage(name, directory: new DirectoryInfo(projectDirectory));
            return package;
        }
    }
}
