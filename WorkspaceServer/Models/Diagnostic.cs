using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using OmniSharp.Client;
using Location = OmniSharp.Client.Location;

namespace WorkspaceServer.Models
{
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
