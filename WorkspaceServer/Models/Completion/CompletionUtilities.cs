using System;
using System.Collections.Generic;
using System.Text;
using OmniSharp.Client.Commands;

namespace WorkspaceServer.Models.Completion
{
    internal static class CompletionUtilities
    {
        public static string GetWordAt(this string source, int position)
        {
            var index = position;
            while (index >= 1)
            {
                var ch = source[index - 1];
                if (ch != '_' && !char.IsLetterOrDigit(ch))
                {
                    break;
                }

                index--;
            }

            return source.Substring(index, position - index);
        }
    }
}
