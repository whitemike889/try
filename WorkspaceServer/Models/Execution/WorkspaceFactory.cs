using System;
using System.IO;
using System.Linq;
using Microsoft.DotNet.Try.Protocol;
using MLS.Agent.Tools;

namespace WorkspaceServer.Models.Execution
{
    public class WorkspaceFactory
    {
        public static Workspace CreateWorkspaceFromDirectory(
            DirectoryInfo directory,
            string workspaceType,
            bool includeInstrumentation = false)
        {
            var filesOnDisk = directory.GetFiles("*.cs", SearchOption.AllDirectories)
                                       .Where(f => !f.IsBuildOutput())
                                       .ToArray();

            var files = filesOnDisk.Select(file => new Workspace.File(file.Name, file.Read())).ToList();

            return new Workspace(
                files: files.ToArray(),
                buffers: files.Select(f => new Workspace.Buffer(
                                          f.Name,
                                          filesOnDisk.Single(fod => fod.Name == f.Name)
                                                     .Read()))
                              .ToArray(),
                workspaceType: workspaceType,
                includeInstrumentation: includeInstrumentation);
        }
    }
}
