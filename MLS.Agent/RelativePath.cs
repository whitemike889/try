using System;
using System.Collections.Generic;
using System.IO;

namespace MLS.Agent
{
    public abstract class RelativePath
    {
        protected RelativePath(string value)
        {
            if (value == null)
            {
                throw new ArgumentException("Path cannot be null", nameof(value));
            }

            string normalized = value.Replace('\\', '/');
            ThrowIfContainsDisallowedCharacters(normalized);
            Value = normalized;
        }

        public string Value { get; }
        private static readonly HashSet<char> DisallowedPathChars = new HashSet<char>(new char[]{
            '|',
            '\0',
            '\u0001',
            '\u0002',
            '\u0003',
            '\u0004',
            '\u0005',
            '\u0006',
            '\a',
            '\b',
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            '\u000e',
            '\u000f',
            '\u0010',
            '\u0011',
            '\u0012',
            '\u0013',
            '\u0014',
            '\u0015',
            '\u0016',
            '\u0017',
            '\u0018',
            '\u0019',
            '\u001a',
            '\u001b',
            '\u001c',
            '\u001d',
            '\u001e',
            '\u001f' });

        private static readonly HashSet<char> DisallowedFileNameChars = new HashSet<char>(new char[]{
            '"',
            '<',
            '>',
            '|',
            '\0',
            '\u0001',
            '\u0002',
            '\u0003',
            '\u0004',
            '\u0005',
            '\u0006',
            '\a',
            '\b',
            '\t',
            '\n',
            '\v',
            '\f',
            '\r',
            '\u000e',
            '\u000f',
            '\u0010',
            '\u0011',
            '\u0012',
            '\u0013',
            '\u0014',
            '\u0015',
            '\u0016',
            '\u0016',
            '\u0017',
            '\u0018',
            '\u0019',
            '\u001a',
            '\u001b',
            '\u001c',
            '\u001d',
            '\u001e',
            '\u001f',
            ':',
            '*',
            '?',
            '\\'});

        private static void ThrowIfContainsDisallowedCharacters(string path)
        {
            var filename = Path.GetFileName(path);
            foreach (var ch in filename)
            {
                if (DisallowedFileNameChars.Contains(ch))
                {
                    throw new ArgumentException($"The character {ch} is not allowed in the filename");
                }
            }

            foreach (var ch in path)
            {
                if (DisallowedPathChars.Contains(ch))
                {
                    throw new ArgumentException($"The character {ch} is not allowed in the path");
                }
            }
        }
    }
}