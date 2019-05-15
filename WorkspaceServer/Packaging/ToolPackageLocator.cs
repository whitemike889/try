using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Clockwise;
using MLS.Agent.Tools;
using WorkspaceServer.WorkspaceFeatures;

namespace WorkspaceServer.Packaging
{
    internal class ToolPackageLocator : IToolPackageLocator
    {
        // FIX: (ToolPackageLocator) rename
        private readonly DirectoryInfo _baseDirectory;

        public ToolPackageLocator(DirectoryInfo baseDirectory = null)
        {
            _baseDirectory = baseDirectory ?? Package.DefaultPackagesDirectory;
        }

        public async Task<IPackage> LocatePackageAsync(string name, Budget budget)
        {
            var candidateTool = new PackageTool(name, _baseDirectory);
            if (!candidateTool.Exists)
            {
                return null;
            }

            var assetDirectory = await PrepareToolAndLocateAssetDirectory(candidateTool);

            if (assetDirectory == null)
            {
                return null;
            }

            var projectAsset = new ProjectAsset(new FileSystemDirectoryAccessor(assetDirectory));
            var wasmAsset = await candidateTool.LocateWasmAsset();

            var p2 = new Package2(new PackageDescriptor(name), new FileSystemDirectoryAccessor(assetDirectory.Parent));
            p2.Add(projectAsset);
            if(wasmAsset != null)
            {
                p2.Add(wasmAsset);
            }


            return p2;
        }

        public async Task<DirectoryInfo> PrepareToolAndLocateAssetDirectory(PackageTool tool)
        {
            await tool.Prepare();
            var directory = await tool.LocateBuildAsset();
            if (directory == null || !directory.Exists)
            {
                return null;
            }

            return directory;
        }
    }
}