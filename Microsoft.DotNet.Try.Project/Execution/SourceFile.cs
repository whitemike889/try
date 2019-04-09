using System;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.DotNet.Try.Project.Execution
{
    public class SourceFile
    {
        public SourceText Text { get; }

        public string Name { get; }

        private SourceFile(SourceText text, string name)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Name = name;
        }

        public static SourceFile Create(string text, string name)
            => new SourceFile(SourceText.From(text ?? string.Empty), name: name);
    }
}
