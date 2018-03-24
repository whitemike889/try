using System;
using Microsoft.CodeAnalysis;

namespace OmniSharp.Client
{
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
}
