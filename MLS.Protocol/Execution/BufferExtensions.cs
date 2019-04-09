using System;
using System.Linq;

namespace Microsoft.DotNet.Try.Protocol.Execution
{
    public static class BufferExtensions
    {
        public static Workspace.Buffer GetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne(
            this Workspace workspace,
            BufferId bufferId = null)
        {
            // TODO: (GetBufferWithSpecifiedIdOrSingleBufferIfThereIsOnlyOne) this concept should go away

            var buffer = workspace.Buffers.SingleOrDefault(b => b.Id == bufferId);

            if (buffer == null)
            {
                if (workspace.Buffers.Length == 1)
                {
                    buffer = workspace.Buffers.Single();
                }
                else
                {
                    throw new ArgumentException("Ambiguous buffer");
                }
            }

            return buffer;
        }
    }
}
