using System.Collections.Generic;
using Microsoft.CodeAnalysis;

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
}namespace OmniSharp.Client{
    public class Diagnostic
    {
        public Diagnostic(
            string id = null,
            string message = null,
            DiagnosticSeverity defaultSeverity = DiagnosticSeverity.Hidden,
            DiagnosticSeverity severity = DiagnosticSeverity.Hidden,
            int warningLevel = 0,
            bool isSuppressed = false,
            bool isWarningAsError = false,
            Location location = null,
            IReadOnlyList<Location> additionalLocations = null,
            IDictionary<string, string> properties = null)
        {
            Id = id;
            Message = message;
            DefaultSeverity = defaultSeverity;
            Severity = severity;
            WarningLevel = warningLevel;
            IsSuppressed = isSuppressed;
            IsWarningAsError = isWarningAsError;
            Location = location;
            AdditionalLocations = additionalLocations;
            Properties = properties;
        }

        public string Id { get; }

        public string Message { get; }

        public DiagnosticSeverity DefaultSeverity { get; }

        public DiagnosticSeverity Severity { get; }

        public int WarningLevel { get; }

        public bool IsSuppressed { get; }

        public bool IsWarningAsError { get; }

        public Location Location { get; }

        public IReadOnlyList<Location> AdditionalLocations { get; }

        public IDictionary<string, string> Properties { get; }

        public override string ToString() =>
            $"({Location?.MappedLineSpan?.StartLinePosition?.OneBased()}): error {Id}: {Message}";
    }
}