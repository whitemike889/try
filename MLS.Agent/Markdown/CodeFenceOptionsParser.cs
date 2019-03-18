using System;
using System.CommandLine;
using System.CommandLine.Binding;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MLS.Agent.Markdown
{
    public class CodeFenceOptionsParser
    {
        private readonly Parser _parser;
        private readonly ModelBinder<CodeLinkBlockOptions> _modelBinder= new ModelBinder<CodeLinkBlockOptions>();
        private static int sessionIndex;

        public CodeFenceOptionsParser(IDirectoryAccessor directoryAccessor)
        {
            if (directoryAccessor == null)
            {
                throw new ArgumentNullException(nameof(directoryAccessor));
            }

            _parser = CreateOptionsParser(directoryAccessor);
        }

        public CodeLinkBlockOptions Parse(string line)
        {
            var result = _parser.Parse(line);

            if (result.CommandResult.Name != "csharp")
            {
                return null;
            }

            if (result.Errors.Any())
            {
                return new CodeLinkBlockOptions().ReplaceErrors(result.Errors.Select(e => e.Message));
            }

            if (result.Tokens.Count == 1)
            {
                return null;
            }

            var options = (CodeLinkBlockOptions)_modelBinder.CreateInstance(new BindingContext(result));

            var projectResult = result.CommandResult["project"];
            if (projectResult?.IsImplicit ?? false)
            {
                options = options.WithIsProjectImplicit(isProjectFileImplicit: true);
            }

            options = options.ReplaceErrors(result.Errors.Select(e => e.Message));

            options.RunArgs = Untokenize(result);

            return options;
        }

        private static string Untokenize(ParseResult result) =>
            string.Join(" ", result.Tokens
                                   .Select(t => t.Value)
                                   .Skip(1)
                                   .Select(t => Regex.IsMatch(t, @".*\s.*")
                                                    ? $"\"{t}\""
                                                    : t));

        private static Parser CreateOptionsParser(IDirectoryAccessor directoryAccessor)
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
                             new Option("--session", argument: new Argument<string>(defaultValue: () => $"Run{++sessionIndex}"))
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
