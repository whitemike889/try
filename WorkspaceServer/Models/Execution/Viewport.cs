using System;
using Microsoft.CodeAnalysis.Text;

namespace WorkspaceServer.Models.Execution
{
    public class Viewport
    {
        public Viewport(SourceFile destination, TextSpan region, string name)
        {
            Region = region;
            Name = name;
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        }

        public string Name { get; }

        public TextSpan Region { get; }

        public SourceFile Destination { get; }
    }
}
