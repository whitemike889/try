using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using Pocket;

namespace WorkspaceServer.Packaging
{
    internal class ToolPackageLocator
    {
        private readonly string _basePath;

        public ToolPackageLocator(string basePath = "")
        {
            _basePath = basePath ?? "";
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

            var directory = output;
            var projectDirectory = Path.Combine(directory, "packTarget");
            Logger<ToolPackageLocator>.Log.Info($"Project: {projectDirectory}");
            var package = new NonrebuildablePackage(name, directory: new DirectoryInfo(projectDirectory));
            return package;
        }
    }
}
