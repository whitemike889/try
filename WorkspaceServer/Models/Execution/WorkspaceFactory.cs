using System;
using System.IO;
using System.Linq;
using MLS.Agent.Tools;
using MLS.Protocol.Execution;

namespace WorkspaceServer.Models.Execution
{
    public class WorkspaceFactory
    {
        public static Workspace FromDirectory(DirectoryInfo directory, string workspaceType)
        {
            var filesOnDisk = directory.GetFiles("*.cs", SearchOption.AllDirectories)
                                       .Where(f => !f.IsBuildOutput())
                                       .ToArray();

            if (!filesOnDisk.Any())
            {
                throw new ArgumentException("Directory does not contain any .cs files.");
            }

            var files = filesOnDisk.Select(file => new MLS.Protocol.Execution.Workspace.File(file.Name, file.Read())).ToList();

            return new Workspace(
                files: files.ToArray(),
                buffers: new[]
                {
                    new MLS.Protocol.Execution.Workspace.Buffer(
                        BufferId.Parse(files.First().Name),
                        filesOnDisk.First().Read(),
                        0)
                },
                workspaceType: workspaceType);
        }
    }
}
