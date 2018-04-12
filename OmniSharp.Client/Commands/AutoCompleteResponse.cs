using System.Collections.Generic;

namespace OmniSharp.Client.Commands
{
    public class AutoCompleteResponse
    {
        /// <summary>
        /// The text to be "completed", that is, the text that will be inserted in the editor.
        /// </summary>
        public string CompletionText { get; set; }
        public string Description { get; set; }
        /// <summary>
        /// The text that should be displayed in the auto-complete UI.
        /// </summary>
        public string DisplayText { get; set; }
        public string RequiredNamespaceImport { get; set; }
        public string MethodHeader { get; set; }
        public string ReturnType { get; set; }
        public string Snippet { get; set; }
        public string Kind { get; set; }
        public bool IsSuggestionMode { get; set; }
    }
}