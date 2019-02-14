using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MLS.Agent.Markdown
{
    public class CodeFenceOptionsParser
    {
        private readonly Parser _parser;
        private CodeLinkBlockOptions _options;

        public CodeFenceOptionsParser(IDirectoryAccessor directoryAccessor)
        {
            _parser = CreateOptionsParser(directoryAccessor, o => _options = o);
        }

        public CodeLinkBlockOptions Parse(string line)
        {
            _options = null;
            var result = _parser.Parse(line);

            if (result.CommandResult.Name != "csharp")
            {
                return null;
            }

            if (result.Errors.Any())
            {
                return new CodeLinkBlockOptions().ReplaceErrors(result.Errors.Select(e => e.Message));
            }

            _parser.InvokeAsync(line).Wait();
            return _options;
        }

        private static Parser CreateOptionsParser(IDirectoryAccessor directoryAccessor, Action<CodeLinkBlockOptions> extractOptionsDelegate)
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
                             new Option("--session", argument: new Argument<string>(defaultValue: "Run"))
                         };

            csharp.AddAlias("CS");
            csharp.AddAlias("C#");
            csharp.AddAlias("CSHARP");
            csharp.AddAlias("cs");
            csharp.AddAlias("c#");

            var binder = new TypeBinder(typeof(CodeLinkBlockOptions));
            var command = new RootCommand { csharp };

            csharp.Handler = CommandHandler.Create<InvocationContext>(context =>
            {
                var options = (CodeLinkBlockOptions) binder.CreateInstance(context);

                var projectResult = context.ParseResult.CommandResult["project"];
                if (projectResult?.IsImplicit ?? false)
                {
                    options = options.WithIsProjectImplicit(
                        isProjectFileImplicit: true);
                }

                options = options.ReplaceErrors(context.ParseResult.Errors.Select(e => e.Message));

                options.RunArgs = string.Join(" ", context.ParseResult.UnparsedTokens
                                                          .Select(t => Regex.IsMatch(t, @".*\s.*")
                                                                           ? $"\"{t}\""
                                                                           : t));

                extractOptionsDelegate(options);
                return 0;
            });

            return new Parser(command);
        }
    }
}
