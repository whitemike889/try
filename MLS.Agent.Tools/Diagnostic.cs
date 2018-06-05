using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MLS.Agent.Tools
{
    public class Diagnostic
    {
        private readonly Microsoft.CodeAnalysis.Diagnostic _diagnostic;

        public Diagnostic(Microsoft.CodeAnalysis.Diagnostic diagnostic)
        {
            this._diagnostic = diagnostic ?? throw new ArgumentNullException(nameof(diagnostic));
        }

        public string Message => _diagnostic.GetMessage();

        public DiagnosticDescriptor Descriptor => _diagnostic.Descriptor;

        public string Id => _diagnostic.Id;

        public DiagnosticSeverity Severity => _diagnostic.Severity;

        public int WarningLevel => _diagnostic.WarningLevel;

        public bool IsSuppressed => _diagnostic.IsSuppressed;

        public Location Location => new Location(_diagnostic.Location);

        public IReadOnlyList<Location> AdditionalLocations => _diagnostic.AdditionalLocations.Select(l => new Location(l)).ToArray();
    }
}
