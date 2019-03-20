using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockOptions
    {
        public CodeLinkBlockOptions(
            RelativeFilePath sourceFile = null,
            RelativeFilePath destinationFile = null,
            FileInfo project = null,
            string package = null,
            string region = null,
            string session = null,
            bool isProjectFileImplicit = false,
            IEnumerable<string> errors = null,
            string runArgs = null)
        {
            SourceFile = sourceFile;
            Project = project;
            Package = package;
            Region = region;
            Session = session;
            IsProjectImplicit = isProjectFileImplicit;
            RunArgs = runArgs;
            DestinationFile = destinationFile;
            Errors = errors ?? Enumerable.Empty<string>();
        }

      

        public CodeLinkBlockOptions WithIsProjectImplicit(bool isProjectFileImplicit)
        {
            return new CodeLinkBlockOptions(
                SourceFile,
                DestinationFile,
                Project,
                Package,
                Region,
                Session,
                isProjectFileImplicit,
                Errors,
                RunArgs);
        }

        public CodeLinkBlockOptions ReplaceErrors(IEnumerable<string> errors)
        {
            return new CodeLinkBlockOptions(
                SourceFile,
                DestinationFile,
                Project,
                Package,
                Region,
                Session,
                IsProjectImplicit,
                errors,
                RunArgs);
        }

        public FileInfo Project { get; }
        public string Package { get; }
        public RelativeFilePath SourceFile { get; }
        public RelativeFilePath DestinationFile { get; }
        public string Region { get; }
        public string RunArgs { get; set; }
        public string Session { get; }
        public bool IsProjectImplicit { get; }
        public IEnumerable<string> Errors { get; }
    }
}