using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace OmniSharp.Client
{
    public class LinePositionSpan
    {
        public LinePositionSpan(LinePosition start, LinePosition end)
        {
            Start = start;
            End = end;
        }

        public LinePosition Start { get; }

        public LinePosition End { get; }

        public override string ToString() => $"({Start})-({End})";
    }

    public class TextSpan
    {
        public TextSpan(
            int start = 0,
            int end = 0,
            int length = 0,
            bool isEmpty = false)
        {
            Start = start;
            End = end;
            Length = length;
            IsEmpty = isEmpty;
        }

        public int Start { get; }
        public int End { get; }
        public int Length { get; }
        public bool IsEmpty { get; }
    }

    public class FileLinePositionSpan
    {
        public FileLinePositionSpan(
            string path = null,
            bool hasMappedPath = false,
            LinePosition startLinePosition = null,
            LinePosition endLinePosition = null,
            LinePositionSpan span = null)
        {
            Path = path;
            HasMappedPath = hasMappedPath;
            StartLinePosition = startLinePosition;
            EndLinePosition = endLinePosition;
            Span = span;
        }

        public string Path { get; }

        public bool HasMappedPath { get; }

        public LinePosition StartLinePosition { get; }

        public LinePosition EndLinePosition { get; }

        public LinePositionSpan Span { get; }

        public override string ToString() => $"{Path}: {Span}";
    }

    public class Location
    {
        public Location(
            FileLinePositionSpan mappedLineSpan = default(FileLinePositionSpan),
            LocationKind kind = default(LocationKind),
            bool isInSource = false,
            bool isInMetadata = false,
            TextSpan sourceSpan = null)
        {
            MappedLineSpan = mappedLineSpan;
            Kind = kind;
            IsInSource = isInSource;
            IsInMetadata = isInMetadata;
            SourceSpan = sourceSpan;
        }

        public LocationKind Kind { get; }

        public bool IsInSource { get; }

        public bool IsInMetadata { get; }

        public TextSpan SourceSpan { get; }

        public FileLinePositionSpan MappedLineSpan { get; }

        public override string ToString() => $"({MappedLineSpan.Span.Start},{MappedLineSpan.Span.End})";
    }

    public class LinePosition
    {
        public LinePosition(
            int line,
            int character,
            bool isOneBased = false)
        {
            var minValue = isOneBased
                               ? 1
                               : 0;

            if (line < minValue)
            {
                throw new ArgumentException($"Value must be at least {minValue} when {nameof(isOneBased)} is set to {isOneBased}", nameof(line));
            }

            if (character < minValue)
            {
                throw new ArgumentException($"Value must be at least {minValue} when {nameof(isOneBased)} is set to {isOneBased}", nameof(character));
            }

            Line = line;
            Character = character;
            IsOneBased = isOneBased;
        }

        public int Line { get; }

        public int Character { get; }

        public bool IsOneBased { get; }

        public override string ToString() => $"{Line},{Character}";
    }

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

    public static class LinePositionExtensions
    {
        public static LinePosition OneBased(this LinePosition position) =>
            position.IsOneBased
                ? position
                : new LinePosition(position.Line + 1, position.Character + 1, true);
    }
}
