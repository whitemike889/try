using System.Collections.Generic;
using System.IO;

namespace OmniSharp.Client.Commands
{
    public class AutoCompleteRequest : AbstractOmniSharpFileCommandArguments
    {
        public override string Command => CommandNames.AutoComplete;

        private string _wordToComplete;

        public AutoCompleteRequest(FileInfo fileName = null, string buffer = null, int? line = null, int? column = null, IEnumerable<LinePositionSpanTextChange> changes = null) : base(fileName, buffer, line, column, changes)
        {
        }

        public string WordToComplete
        {
            get => _wordToComplete ?? "";
            set => _wordToComplete = value;
        }

        /// <summary>
        ///   Specifies whether to return the code documentation for
        ///   each and every returned autocomplete result.
        /// </summary>
        public bool WantDocumentationForEveryCompletionResult { get; set; }

        /// <summary>
        ///   Specifies whether to return importable types. Defaults to
        ///   false. Can be turned off to get a small speed boost.
        /// </summary>
        public bool WantImportableTypes { get; set; }

        /// <summary>
        /// Returns a 'method header' for working with parameter templating.
        /// </summary>
        public bool WantMethodHeader { get; set; }

        /// <summary>
        /// Returns a snippet that can be used by common snippet libraries
        /// to provide parameter and type parameter placeholders
        /// </summary>
        public bool WantSnippet { get; set; }

        /// <summary>
        /// Returns the return type
        /// </summary>
        public bool WantReturnType { get; set; }

        /// <summary>
        /// Returns the kind (i.e Method, Property, Field)
        /// </summary>
        public bool WantKind { get; set; }

        public string TriggerCharacter { get; set; }
    }
}