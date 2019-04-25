using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;

namespace WorkspaceServer.Packaging
{
    internal class ToolPackageLocator : IToolPackageLocator
    {
        // FIX: (ToolPackageLocator) rename
        private readonly string _basePath;

        public ToolPackageLocator(string basePath = "")
        {
            _basePath = basePath ?? "";
        }

        public async Task<Package> LocatePackageAsync(string name, Budget budget)
        {
            var assetDirectory = await PrepareToolAndLocateAssetDirectory(new FileInfo(Path.Combine(_basePath, name)));

            if (assetDirectory == null)
            {
                return null;
            }

            return new NonrebuildablePackage(name, directory: assetDirectory);
        }

        public async Task<DirectoryInfo> PrepareToolAndLocateAssetDirectory(FileInfo tool, Budget budget = null)
        {
            CommandLineResult result;

            try
            {
                result = await CommandLine.Execute(tool.FullName, "prepare-package", budget: budget);

                result.ThrowOnFailure();

                result = await CommandLine.Execute(tool.FullName, "locate-projects", budget: budget);

                result.ThrowOnFailure();
            }
            catch (Win32Exception)
            {
                return null;
            }

            var directory = result.Output.FirstOrDefault();
            if (directory == null || !Directory.Exists(directory))
            {
                return null;
            }

            return new DirectoryInfo(Path.Combine(directory, "packTarget"));
        }
    }
}