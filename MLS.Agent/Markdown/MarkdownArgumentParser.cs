using System.CommandLine;
using System.IO;
using System.Linq;

namespace MLS.Agent.Markdown
{
    public class MarkdownArgumentParser
    {
        public static Parser CreateOptionsParser(IDirectoryAccessor directoryAccessor)
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
                                            if (directoryAccessor.FileExists(relativeFilePath))
                                            {
                                                return ArgumentResult.Success(relativeFilePath);
                                            }

                                            return ArgumentResult.Failure($"File not found: {relativeFilePath.Value}");
                                        }

                                        return ArgumentResult.Failure($"Error parsing the filename: {filename}");
                                    })
            {
                Name = "SourceFile",
                Arity = ArgumentArity.ZeroOrOne
            };

            var projectArg = new Argument<FileInfo>(result =>
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

            projectArg.SetDefaultValue(() =>
            {
                var projectFiles = directoryAccessor.GetAllFilesRecursively()
                                                    .Where(file => file.Extension == ".csproj")
                                                    .ToArray();

                if (projectFiles.Length == 1)
                {
                    return directoryAccessor.GetFullyQualifiedPath(projectFiles.Single());
                }

                return null;
            });

            var regionArgument = new Argument<string>();
            var packageArgument = new Argument<string>();

            var csharp = new Command("csharp", argument: sourceFileArg)
                         {
                             new Option("--project", argument: projectArg),
                             new Option("--region", argument: regionArgument),
                             new Option("--package", argument: packageArgument),
                             new Option("--session", argument: new Argument<string>(defaultValue: "Run"))
                         };


            csharp.AddAlias("CS");
            csharp.AddAlias("C#");
            csharp.AddAlias("CSHARP");
            csharp.AddAlias("cs");
            csharp.AddAlias("c#");

            return new Parser(new RootCommand { csharp });
        }
    }
}
