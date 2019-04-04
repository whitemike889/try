using System;
using System.CommandLine;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.DotNet.Try.Markdown
{
    public class CodeLinkBlockOptions
    {
        protected static int _sessionIndex;

        public CodeLinkBlockOptions(
            RelativeFilePath destinationFile = null,
            string package = null,
            string region = null,
            string session = null,
            bool editable = false,
            bool hidden = false,
            string runArgs = null,
            ParseResult parseResult = null,
            string packageVersion = null)
        {
            DestinationFile = destinationFile;
            Package = package;
            Region = region;
            Session = session;
            RunArgs = runArgs;
            ParseResult = parseResult;
            PackageVersion = packageVersion;
            Editable = editable;
            Hidden = hidden;

            if (string.IsNullOrWhiteSpace(Session) && Editable)
            {
                Session = $"Run{++_sessionIndex}";
            }
        }

        public string Package { get; }
        public RelativeFilePath DestinationFile { get; }
        public string Region { get; }
        public string RunArgs { get; set; }
        public ParseResult ParseResult { get; }
        public string PackageVersion { get; }
        public string Session { get; }
        public bool Editable { get; }
        public bool Hidden { get; }
        public string Language { get; set; }

        public virtual Task<CodeLinkBlockResult> TryGetExternalContent()
        {
            return Task.FromResult<CodeLinkBlockResult>(null);
        }

        public virtual Task AddAttributes(CodeLinkBlock block)
        {
            if (Package != null)
            {
                block.AddAttribute("data-trydotnet-package", Package);
            }
            
            if (PackageVersion != null)
            {
                block.AddAttribute("data-trydotnet-package-version", PackageVersion);
            }

            return Task.CompletedTask;
        }
    }
}