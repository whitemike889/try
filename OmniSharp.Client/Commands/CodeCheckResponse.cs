using System;
using System.Collections.Generic;

namespace OmniSharp.Client.Commands
{
    public class CodeCheckResponse
    {
        public CodeCheckResponse(IReadOnlyCollection<QuickFix> quickFixes)
        {
            QuickFixes = quickFixes ?? Array.Empty<QuickFix>();
        }

        public IReadOnlyCollection<QuickFix> QuickFixes { get; }
    }
}