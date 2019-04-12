using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Markdig;
using Microsoft.DotNet.Try.Markdown;
using MLS.Agent.Tools;
using WorkspaceServer;

namespace MLS.Agent.Markdown
{
    public class LocalCodeFenceAnnotationsParser : CodeFenceAnnotationsParser
    {
        private readonly IDirectoryAccessor _directoryAccessor;
        private readonly PackageRegistry _packageRegistry;

        public LocalCodeFenceAnnotationsParser(
            IDirectoryAccessor directoryAccessor,
            PackageRegistry packageRegistry,
            IDefaultCodeBlockAnnotations defaultAnnotations = null) : base(defaultAnnotations, csharp =>
        {
            AddProjectOption(csharp, directoryAccessor);
            AddSourceFileOption(csharp);
        })
        {
            _directoryAccessor = directoryAccessor;
            _packageRegistry = packageRegistry ?? throw new ArgumentNullException(nameof(packageRegistry));
        }

        public override CodeFenceOptionsParseResult TryParseCodeFenceOptions(
            string line, 
            MarkdownParserContext context = null)
        {
            var result = base.TryParseCodeFenceOptions(line, context);

            if (result is SuccessfulCodeFenceOptionParseResult succeeded &&
                succeeded.Annotations is LocalCodeBlockAnnotations local)
            {
                local.DirectoryAccessor =
                    new AsyncLazy<IDirectoryAccessor>(async () =>
                                                          await GetDirectoryAccessorForPackage(
                                                              local,
                                                              _packageRegistry) ?? _directoryAccessor);

                var projectResult = local.ParseResult.CommandResult["project"];
                if (projectResult?.IsImplicit ?? false)
                {
                    local.IsProjectImplicit = true;
                }
            }

            return result;
        }

        private static async Task<IDirectoryAccessor> GetDirectoryAccessorForPackage(
            CodeBlockAnnotations annotations,
            PackageRegistry packageRegistry)
        {
            if (annotations.Package != null)
            {
                var installedPackage = await packageRegistry.Get(annotations.Package);

                if (installedPackage?.Directory != null)
                {
                    return new FileSystemDirectoryAccessor(installedPackage.Directory);
                }
            }

            return null;
        }

        protected override ModelBinder CreateModelBinder()
        {
            return new ModelBinder(typeof(LocalCodeBlockAnnotations));
        }

        private static void AddSourceFileOption(Command csharp)
        {
            var sourceFileArg = new Argument<RelativeFilePath>(
                                    result =>
                                    {
                                        var filename = result.Arguments.SingleOrDefault();

                                        if (filename == null)
                                        {
                                            return ArgumentResult.Success<string>(null);
                                        }

                                        if (RelativeFilePath.TryParse(filename, out var relativeFilePath))
                                        {
                                            return ArgumentResult.Success(relativeFilePath);
                                        }

                                        return ArgumentResult.Failure($"Error parsing the filename: {filename}");
                                    })
                                {
                                    Name = "SourceFile",
                                    Arity = ArgumentArity.ZeroOrOne
                                };

            var sourceFileOption = new Option("--source-file",
                                              argument: sourceFileArg);

            csharp.AddOption(sourceFileOption);
        }

        private static void AddProjectOption(
            Command csharp,
            IDirectoryAccessor directoryAccessor)
        {
            var projectOptionArgument = new Argument<FileInfo>(result =>
                                        {
                                            var projectPath = new RelativeFilePath(result.Arguments.Single());

                                            if (directoryAccessor.FileExists(projectPath))
                                            {
                                                return ArgumentResult.Success(directoryAccessor.GetFullyQualifiedPath(projectPath));
                                            }

                                            return ArgumentResult.Failure($"Project not found: {projectPath.Value}");
                                        })
                                        {
                                            Name = "project",
                                            Arity = ArgumentArity.ExactlyOne
                                        };

            projectOptionArgument.SetDefaultValue(() =>
            {
                var rootDirectory = directoryAccessor.GetFullyQualifiedPath(new RelativeDirectoryPath("."));
                var projectFiles = directoryAccessor.GetAllFilesRecursively()
                                                    .Where(file => directoryAccessor.GetFullyQualifiedPath(file.Directory) == rootDirectory && file.Extension == ".csproj")
                                                    .ToArray();

                if (projectFiles.Length == 1)
                {
                    return directoryAccessor.GetFullyQualifiedPath(projectFiles.Single());
                }

                return null;
            });

            var projectOption = new Option("--project",
                                           argument: projectOptionArgument);

            csharp.Add(projectOption);
        }
    }
}