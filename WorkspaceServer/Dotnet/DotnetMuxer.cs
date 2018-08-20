// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Reflection;
using MLS.Agent.Tools;

namespace MLS.Agent.CLI
{
    public static class DotnetMuxer
    {
        public static FileInfo Path { get; }

        static DotnetMuxer()
        {
            var muxerFileName = "dotnet".ExecutableName();

            var fxDepsFile = GetDataFromAppDomain("FX_DEPS_FILE");

            if (string.IsNullOrEmpty(fxDepsFile))
            {
                return;
            }

            var muxerDir = new FileInfo(fxDepsFile).Directory?.Parent?.Parent?.Parent;

            if (muxerDir == null)
            {
                return;
            }

            var muxerCandidate = new FileInfo(System.IO.Path.Combine(muxerDir.FullName, muxerFileName));

            if (muxerCandidate.Exists)
            {
                Path = muxerCandidate;
            }
            else
            {
                throw new InvalidOperationException("Ain't no muxer!");
            }
        }

        internal static string SharedFxVersion =>
            new FileInfo(GetDataFromAppDomain("FX_DEPS_FILE")).Directory.Name;

        public static string GetDataFromAppDomain(string propertyName)
        {
            var appDomainType = typeof(object).GetTypeInfo().Assembly?.GetType("System.AppDomain");
            var currentDomain = appDomainType?.GetProperty("CurrentDomain")?.GetValue(null);
            var deps = appDomainType?.GetMethod("GetData")?.Invoke(currentDomain, new[] {propertyName});
            return deps as string;
        }
    }
}