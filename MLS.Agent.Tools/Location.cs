using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MLS.Agent.Tools
{
    public class Location
    {
        private readonly Microsoft.CodeAnalysis.Location location;

        public Location(Microsoft.CodeAnalysis.Location location)
        {
            this.location = location ?? throw new ArgumentNullException(nameof(location));
        }

        public LocationKind Kind => location.Kind;

        public bool IsInSource => location.IsInSource;

        public bool IsInMetadata => location.IsInMetadata;

        public TextSpan SourceSpan => location.SourceSpan;

        public FileLinePositionSpan MappedLineSpan => location.GetMappedLineSpan();
    }
}
