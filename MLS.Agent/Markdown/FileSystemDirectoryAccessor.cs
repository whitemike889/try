﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MLS.Agent.Markdown
{
    public class FileSystemDirectoryAccessor : IDirectoryAccessor
    {
        private readonly DirectoryInfo _rootDirectory;
        public static readonly HashSet<char> DisallowedPathChars = new HashSet<char>(new char[]{
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

        public static readonly HashSet<char> DisallowedFileNameChars = new HashSet<char>(new char[]{
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

        public FileSystemDirectoryAccessor(DirectoryInfo rootDir)
        {
            _rootDirectory = rootDir ?? throw new System.ArgumentNullException(nameof(rootDir));
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(GetFullyQualifiedPath(filePath));
        }
        
        

        public string ReadAllText(string filePath)
        {
            return File.ReadAllText(GetFullyQualifiedPath(filePath));
        }

        public string GetFullyQualifiedPath(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException();
            }

            var path = Path.IsPathRooted(filePath)
                ? filePath
                : Path.Combine(_rootDirectory.FullName, filePath);

            var normalizedPath = path.NormalizePath();
            ThrowIfContainsDisallowedCharacters(normalizedPath);
            return normalizedPath;
        }

        public static void ThrowIfContainsDisallowedCharacters(string filePath)
        {
            var filename = Path.GetFileName(filePath);
            foreach(var ch in filename)
            {
                if(DisallowedFileNameChars.Contains(ch))
                {
                    throw new ArgumentException($"The character {ch} is not allowed in the filename");
                }
            }

            foreach (var ch in filePath)
            {
                if (DisallowedPathChars.Contains(ch))
                {
                    throw new ArgumentException($"The character {ch} is not allowed in the path");
                }
            }
        }

        public IDirectoryAccessor GetDirectoryAccessorForRelativePath(string relativePath)
        {   
            return new FileSystemDirectoryAccessor(new DirectoryInfo(Path.Combine(_rootDirectory.FullName, relativePath)));
        }

        public IEnumerable<FileInfo> GetAllFilesRecursively()
        {
            return _rootDirectory.GetFiles("*", SearchOption.AllDirectories);
        }
    }
}
