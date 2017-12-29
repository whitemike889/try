using System.Collections.Generic;

namespace OmniSharp.Client.Commands
{
    public class QuickFix
    {
        public string FileName { get; set; }

        public int Line { get; set; }

        public int Column { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public string Text { get; set; }

        public ICollection<string> Projects { get; set; } = new List<string>();

        public override string ToString()
            => $"{Line}:{Column}-{EndLine}:{EndColumn}: {Text} ({FileName})";
    }
}