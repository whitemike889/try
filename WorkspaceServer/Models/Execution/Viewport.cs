using System;
using Microsoft.CodeAnalysis.Text;

namespace WorkspaceServer.Models.Execution
{
    public class Viewport
    {
        public Viewport(SourceFile destination, TextSpan region, BufferId bufferId)
        {
            Region = region;
            BufferId = bufferId;
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        }

        public BufferId BufferId { get; }

        public TextSpan Region { get; }

        public SourceFile Destination { get; }
    }
}
