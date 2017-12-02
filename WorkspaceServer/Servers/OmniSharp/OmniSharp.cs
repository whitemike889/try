using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Pocket;
using static Pocket.Logger<WorkspaceServer.Servers.OmniSharp.OmniSharp>;

namespace WorkspaceServer.Servers.OmniSharp
{
    internal static class OmniSharp
    {
        private static readonly DirectoryInfo OmniSharpInstallFolder =
            new DirectoryInfo(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".trydotnet",
                    "omnisharp"));

        private static readonly FileInfo OmniSharpExe =
            new FileInfo(
                Path.Combine(
                    OmniSharpInstallFolder.FullName,
                    "OmniSharp.exe"));

        private static readonly FileInfo OmniSharpRunScript =
            new FileInfo(
                Path.Combine(
                    OmniSharpInstallFolder.FullName,
                    "run"));

        public static FileInfo GetPath()
        {
            var fileInfo = GetInstalledLocation() ??
                           GetFromWellKnownLocationOrAcquire() ??
                           throw new OmniSharpNotFoundException("Failed to locate or acquire OmniSharp.");

            Log.Info("Using OmniSharp at {path}", fileInfo);

            return fileInfo;
        }

        private static FileInfo GetFromWellKnownLocationOrAcquire()
        {
            using (Log.OnEnterAndExit())
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return AcquireForWindows();
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return AcquireForLinux();
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return AcquireForOSX();
                }

                throw new InvalidOperationException("BeOS? You go dawg.");
            }
        }

        private static FileInfo AcquireForLinux()
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                if (!OmniSharpRunScript.Exists)
                {
                    var targzFile = Download(new Uri(@"https://github.com/OmniSharp/omnisharp-roslyn/releases/download/v1.27.2/omnisharp-linux-x64.tar.gz"));

                    CommandLine.Execute(
                        "tar",
                        $"xvz {targzFile.FullName} -C {OmniSharpInstallFolder}");

                    OmniSharpRunScript.Refresh();

                    if (!OmniSharpRunScript.Exists)
                    {
                        return null;
                    }
                }

                operation.Succeed();

                return OmniSharpExe;
            }
        }

        private static FileInfo AcquireForOSX()
        {
            return null;
        }

        private static FileInfo AcquireForWindows()
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                if (!OmniSharpExe.Exists)
                {
                    var zipFile = Download(new Uri(@"https://github.com/OmniSharp/omnisharp-roslyn/releases/download/v1.27.2/omnisharp-win-x64.zip"));

                    using (var stream = zipFile.OpenRead())
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        archive.ExtractToDirectory(OmniSharpInstallFolder.FullName);
                    }

                    OmniSharpExe.Refresh();

                    if (!OmniSharpExe.Exists)
                    {
                        return null;
                    }
                }

                operation.Succeed();

                return OmniSharpExe;
            }
        }

        private static FileInfo Download(Uri uri)
        {
            return Task.Run(async () =>
            {
                var fileToWriteTo = Path.GetTempFileName();

                using (var httpClient = new HttpClient())
                {
                    var response = await httpClient.GetStreamAsync(uri);

                    using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
                    {
                        await response.CopyToAsync(streamToWriteTo);
                    }
                }

                return new FileInfo(fileToWriteTo);
            }).Result;
        }

        private static FileInfo GetInstalledLocation()
        {
            var omnisharpPathEnvironmentVariableName = "TRYDOTNET_OMNISHARP_PATH";

            var environmentVariable = Environment.GetEnvironmentVariable(omnisharpPathEnvironmentVariableName);

            if (environmentVariable == null)
            {
                return null;
            }

            var fileInfo = new FileInfo(environmentVariable);

            return fileInfo.Exists
                       ? fileInfo
                       : null;
        }
    }
}
