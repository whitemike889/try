using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace OmniSharp.Emit
{
    public class Diagnostic
    {
        private readonly Microsoft.CodeAnalysis.Diagnostic diagnostic;

        public Diagnostic(Microsoft.CodeAnalysis.Diagnostic diagnostic)
        {
            this.diagnostic = diagnostic ?? throw new ArgumentNullException(nameof(diagnostic));
        }

        public string Message => diagnostic.GetMessage();

        public DiagnosticDescriptor Descriptor => diagnostic.Descriptor;

        public string Id => diagnostic.Id;

        public DiagnosticSeverity Severity => diagnostic.Severity;

        public int WarningLevel => diagnostic.WarningLevel;

        public bool IsSuppressed => diagnostic.IsSuppressed;

        public Location Location => new Location(diagnostic.Location);

        public IReadOnlyList<Location> AdditionalLocations => diagnostic.AdditionalLocations.Select(l => new Location(l)).ToArray();
    }
}
