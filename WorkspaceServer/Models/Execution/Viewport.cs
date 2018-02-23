using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkspaceServer.Models.Execution
{
    public class Viewport
    {
        public Viewport(SourceFile destination, TextSpan region)
        {
            this.Region = region;
            this.Destination = destination;
        }

        public TextSpan Region { get; }

        public SourceFile Destination { get; }
    }
}
