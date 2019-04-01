using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Markdown
{
    public class CodeLinkBlockOptions
    {
        private static int sessionIndex;

        public CodeLinkBlockOptions(
            RelativeFilePath sourceFile = null,
            RelativeFilePath destinationFile = null,
            FileInfo project = null,
            string package = null,
            string region = null,
            string session = null,
            bool isProjectFileImplicit = false,
            bool editable = false,
            bool hidden = false,
            IEnumerable<string> errors = null,
            string runArgs = null)
        {
            SourceFile = sourceFile;
            DestinationFile = destinationFile;
            Project = project;
            Package = package;
            Region = region;
            Session = session;
            IsProjectImplicit = isProjectFileImplicit;
            RunArgs = runArgs;
            Editable = editable;
            Hidden = hidden;
            Errors = errors ?? Enumerable.Empty<string>();

            if (string.IsNullOrWhiteSpace(Session) && !Editable)
            {
                Session = $"Run{++sessionIndex}";
            }
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
                Editable,
                Hidden,
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
                Editable,
                Hidden,
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
        public bool Editable { get; }
        public bool Hidden { get; }
        public IEnumerable<string> Errors { get; }
        public string Language { get; set; }
    }
}