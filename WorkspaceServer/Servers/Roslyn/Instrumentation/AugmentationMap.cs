using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation
{
    public class AugmentationMap : ISerializableEveryLine
    {
        public Dictionary<SyntaxNode, Augmentation> Data { get; }
        public AugmentationMap(Dictionary<SyntaxNode, Augmentation> data = null)
        {
            Data = data ?? new Dictionary<SyntaxNode, Augmentation>();
        }

        public AugmentationMap(params Augmentation[] augmentations)
        {
            Data = new Dictionary<SyntaxNode, Augmentation>();
            foreach (var augmentation in augmentations)
            {
                Data[augmentation.AssociatedStatement] = augmentation;
            }
        }
        public string SerializeForLine(SyntaxNode line)
        {
            return Data[line].Serialize();
        }
    }
}
