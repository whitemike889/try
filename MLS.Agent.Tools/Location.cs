using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace MLS.Agent.Tools
{
    public class Location
    {
        private readonly Microsoft.CodeAnalysis.Location _location;

        public Location(Microsoft.CodeAnalysis.Location location)
        {
            this._location = location ?? throw new ArgumentNullException(nameof(location));
        }

        public LocationKind Kind => _location.Kind;

        public bool IsInSource => _location.IsInSource;

        public bool IsInMetadata => _location.IsInMetadata;

        public TextSpan SourceSpan => _location.SourceSpan;

        public FileLinePositionSpan MappedLineSpan => _location.GetMappedLineSpan();
    }
}
