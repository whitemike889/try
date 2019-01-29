using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockOptions
    {
        public CodeLinkBlockOptions(
            RelativeFilePath sourceFile = null, 
            FileInfo project = null, 
            string package = null, 
            string region = null, 
            string session = null,
            bool isProjectFileImplicit = false,
            IEnumerable<string> errors = null)
        {
            SourceFile = sourceFile;
            Project = project;
            Package = package;
            Region = region;
            Session = session;
            IsProjectImplicit = isProjectFileImplicit;
            Errors = errors ?? Enumerable.Empty<string>();
        }

        public CodeLinkBlockOptions WithIsProjectImplicit(bool isProjectFileImplicit)
        {
            return new CodeLinkBlockOptions(
                        SourceFile,
                        Project,
                        Package,
                        Region,
                        Session,
                        isProjectFileImplicit: isProjectFileImplicit,
                        errors: Errors);
        }

        public CodeLinkBlockOptions ReplaceErrors(IEnumerable<string> errors)
        {
            return new CodeLinkBlockOptions(
                        SourceFile,
                        Project,
                        Package,
                        Region,
                        Session,
                        IsProjectImplicit,
                        errors: errors);
        }

        public FileInfo Project { get; }
        public string Package { get; }
        public RelativeFilePath SourceFile { get; }
        public string Region { get; }
        public string Session { get; }
        public bool IsProjectImplicit { get; internal set; }
        public IEnumerable<string> Errors { get; }
    }
}