using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Clockwise;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using MLS.Agent.Tools;
using WorkspaceServer.Models.Execution;
using MLS.Agent.Workspaces;

namespace WorkspaceServer.Servers.Roslyn
{
    internal static class WorkspaceUtilities
    {
        private static readonly string _baseDir = Path.GetDirectoryName(typeof(WorkspaceUtilities).Assembly.Location);

        public static readonly ImmutableArray<string> DefaultUsings = new[]
        {
            "System",
            "System.Linq",
            "System.Collections.Generic"
        }.ToImmutableArray();

        public static ImmutableArray<MetadataReference> DefaultReferencedAssemblies =
            AssembliesNamesToReference()
                .Select(assemblyName =>
                            new FileInfo(Path.Combine(_baseDir, "completion", "references", $"{assemblyName}.dll")))
                .Where(assembly => assembly.Exists)
                .Select(assembly => MetadataReference.CreateFromFile(
                            assembly.FullName,
                            documentation: XmlDocumentationProvider.CreateFromFile(Path.Combine(_baseDir, "completion", "references", $"{assembly.Name}.xml")))
                )
                .Cast<MetadataReference>()
                .ToImmutableArray();

        public static IEnumerable<MetadataReference> GetMetadadataReferences(this IEnumerable<string> filePaths)
        {
            foreach (var filePath in filePaths)
            {
                var fileInfo = new FileInfo(filePath);

                yield return MetadataReference.CreateFromFile(
                    fileInfo.FullName,
                    documentation: XmlDocumentationProvider.CreateFromFile(
                        Path.Combine(_baseDir,
                                     "completion",
                                     "references",
                                     $"{fileInfo.Name}.xml")));
            }
        }

        public static async Task<(Compilation compilation, IReadOnlyCollection<Document> documents)> GetCompilation(
            this WorkspaceBuild build,
            IReadOnlyCollection<SourceFile> sources,
            Budget budget)
        {
            var projectId = ProjectId.CreateNewId();

            var workspace = await build.GetRoslynWorkspace(projectId);

            var currentSolution = workspace.CurrentSolution;

            foreach (var source in sources)
            {
                if (currentSolution.Projects
                                   .SelectMany(p => p.Documents)
                                   .FirstOrDefault(d => d.Name == source.Name) is Document document)
                {
                    // there's a pre-existing document, so overwrite it's contents
                    document = document.WithText(source.Text);
                    currentSolution = document.Project.Solution;
                }
                else
                {
                    var docId = DocumentId.CreateNewId(projectId, $"{build.Name}.Document");

                    currentSolution = currentSolution.AddDocument(docId, source.Name, source.Text);
                }

            }

            var project = currentSolution.GetProject(projectId);

            var compilation = await project.GetCompilationAsync().CancelIfExceeds(budget);

            return (compilation, project.Documents.ToArray());
        }

        private static string[] AssembliesNamesToReference() => new[]
        {
            "mscorlib",
            "netstandard",
            "System.AppContext",
            "System.Collections.Concurrent",
            "System.Collections",
            "System.Collections.NonGeneric",
            "System.Collections.Specialized",
            "System.ComponentModel.Composition",
            "System.ComponentModel",
            "System.ComponentModel.EventBasedAsync",
            "System.ComponentModel.Primitives",
            "System.ComponentModel.TypeConverter",
            "System.Console",
            "System.Core",
            "System.Data.Common",
            "System.Data",
            "System.Diagnostics.Contracts",
            "System.Diagnostics.Debug",
            "System.Diagnostics.FileVersionInfo",
            "System.Diagnostics.Process",
            "System.Diagnostics.StackTrace",
            "System.Diagnostics.TextWriterTraceListener",
            "System.Diagnostics.Tools",
            "System.Diagnostics.TraceSource",
            "System.Diagnostics.Tracing",
            "System",
            "System.Drawing",
            "System.Drawing.Primitives",
            "System.Dynamic.Runtime",
            "System.Globalization.Calendars",
            "System.Globalization",
            "System.Globalization.Extensions",
            "System.IO.Compression",
            "System.IO.Compression.FileSystem",
            "System.IO.Compression.ZipFile",
            "System.IO",
            "System.IO.FileSystem",
            "System.IO.FileSystem.DriveInfo",
            "System.IO.FileSystem.Primitives",
            "System.IO.FileSystem.Watcher",
            "System.IO.IsolatedStorage",
            "System.IO.MemoryMappedFiles",
            "System.IO.Pipes",
            "System.IO.UnmanagedMemoryStream",
            "System.Linq",
            "System.Linq.Expressions",
            "System.Linq.Parallel",
            "System.Linq.Queryable",
            "System.Net",
            "System.Net.Http",
            "System.Net.NameResolution",
            "System.Net.NetworkInformation",
            "System.Net.Ping",
            "System.Net.Primitives",
            "System.Net.Requests",
            "System.Net.Security",
            "System.Net.Sockets",
            "System.Net.WebHeaderCollection",
            "System.Net.WebSockets.Client",
            "System.Net.WebSockets",
            "System.Numerics",
            "System.ObjectModel",
            "System.Reflection",
            "System.Reflection.Extensions",
            "System.Reflection.Primitives",
            "System.Resources.Reader",
            "System.Resources.ResourceManager",
            "System.Resources.Writer",
            "System.Runtime.CompilerServices.VisualC",
            "System.Runtime",
            "System.Runtime.Extensions",
            "System.Runtime.Handles",
            "System.Runtime.InteropServices",
            "System.Runtime.InteropServices.RuntimeInformation",
            "System.Runtime.Numerics",
            "System.Runtime.Serialization",
            "System.Runtime.Serialization.Formatters",
            "System.Runtime.Serialization.Json",
            "System.Runtime.Serialization.Primitives",
            "System.Runtime.Serialization.Xml",
            "System.Security.Claims",
            "System.Security.Cryptography.Algorithms",
            "System.Security.Cryptography.Csp",
            "System.Security.Cryptography.Encoding",
            "System.Security.Cryptography.Primitives",
            "System.Security.Cryptography.X509Certificates",
            "System.Security.Principal",
            "System.Security.SecureString",
            "System.ServiceModel.Web",
            "System.Text.Encoding",
            "System.Text.Encoding.Extensions",
            "System.Text.RegularExpressions",
            "System.Threading",
            "System.Threading.Overlapped",
            "System.Threading.Tasks",
            "System.Threading.Tasks.Parallel",
            "System.Threading.Thread",
            "System.Threading.ThreadPool",
            "System.Threading.Timer",
            "System.Transactions",
            "System.ValueTuple",
            "System.Web",
            "System.Windows",
            "System.Xml",
            "System.Xml.Linq",
            "System.Xml.ReaderWriter",
            "System.Xml.Serialization",
            "System.Xml.XDocument",
            "System.Xml.XmlDocument",
            "System.Xml.XmlSerializer",
            "System.Xml.XPath",
            "System.Xml.XPath.XDocument",

            "Newtonsoft.Json",
            "NodaTime",
            "NodaTime.Testing",
        };
    }
}
