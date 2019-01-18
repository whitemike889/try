// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace WorkspaceServer
{
    public static class DotnetMuxer
    {
        private static readonly Lazy<FileInfo> _getPath = new Lazy<FileInfo>(() =>
                                                                                 FindDotnetFromAppContext() ??
                                                                                 FindDotnetFromPath());

        public static FileInfo Path => _getPath.Value;

        private static FileInfo FindDotnetFromPath()
        {
            FileInfo fileInfo = null;

            using (var process = Process.Start("dotnet"))
            {
                if (process != null)
                {
                    fileInfo = new FileInfo(process.MainModule.FileName);
                }
            }

            return fileInfo;
        }

        private static FileInfo FindDotnetFromAppContext()
        {
            var muxerFileName = "dotnet".ExecutableName();

            var fxDepsFile = GetDataFromAppDomain("FX_DEPS_FILE");

            if (!string.IsNullOrEmpty(fxDepsFile))
            {
                var muxerDir = new FileInfo(fxDepsFile).Directory?.Parent?.Parent?.Parent;

                if (muxerDir != null)
                {
                    var muxerCandidate = new FileInfo(System.IO.Path.Combine(muxerDir.FullName, muxerFileName));

                    if (muxerCandidate.Exists)
                    {
                        return muxerCandidate;
                    }
                }
            }

            return null;
        }

        public static string GetDataFromAppDomain(string propertyName)
        {
            var appDomainType = typeof(object).GetTypeInfo().Assembly?.GetType("System.AppDomain");
            var currentDomain = appDomainType?.GetProperty("CurrentDomain")?.GetValue(null);
            var deps = appDomainType?.GetMethod("GetData")?.Invoke(currentDomain, new[] { propertyName });
            return deps as string;
        }
    }
}