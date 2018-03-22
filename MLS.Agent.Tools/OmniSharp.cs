using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Pocket;

namespace MLS.Agent.Tools
{
    public static class OmniSharp
    {
        private static readonly DirectoryInfo _omniSharpInstallFolder;
        private static readonly string _version = @"v1.29.0-beta1";
        private static readonly AsyncLazy<FileInfo> _omniSharp;
        private static readonly Logger Log = new Logger(nameof(OmniSharp));

        static OmniSharp()
        {
            var environmentVariable = Environment.GetEnvironmentVariable("TRYDOTNET_OMNISHARP_PATH");

            var omniSharpInstallPath =
                environmentVariable ??
                Path.Combine(Paths.UserProfile,
                             ".trydotnet",
                             "omnisharp",
                             _version);

            _omniSharpInstallFolder = new DirectoryInfo(omniSharpInstallPath);

            _omniSharp = new AsyncLazy<FileInfo>(CheckInstallationAndAcquireIfNeeded);

            async Task<FileInfo> CheckInstallationAndAcquireIfNeeded()
            {
                using (Log.OnEnterAndExit())
                {
                    FileInfo fileInfo;

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        fileInfo = await AcquireAndExtractWithZip("omnisharp-win-x64.zip");
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        fileInfo = await AcquireAndExtractWithTar("omnisharp-linux-x64.tar.gz");
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        fileInfo = await AcquireAndExtractWithTar("omnisharp-osx.tar.gz");
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unrecognized OS: {RuntimeInformation.OSDescription}");
                    }

                    if (fileInfo == null)
                    {
                        throw new OmniSharpNotFoundException("Failed to locate or acquire OmniSharp.");
                    }

                    Log.Info("Using OmniSharp at {path}", fileInfo);

                    return fileInfo;
                }
            }
        }

        public static async Task<FileInfo> EnsureInstalledOrAcquire() => 
            await _omniSharp.ValueAsync();

        private static async Task<FileInfo> AcquireAndExtractWithTar(string file)
        {
            var omniSharpRunScript = new FileInfo(
                Path.Combine(
                    _omniSharpInstallFolder.FullName,
                    "run"));

            if (!omniSharpRunScript.Exists)
            {
                using (var operation = Log.OnEnterAndConfirmOnExit())
                {
                    var downloadUri = new Uri($@"https://github.com/OmniSharp/omnisharp-roslyn/releases/download/{_version}/{file}");

                    operation.Info("OmniSharp not found at {path}. Downloading from {uri}.", omniSharpRunScript, downloadUri);

                    var targzFile = await Download(downloadUri);

                    _omniSharpInstallFolder.Create();

                    await CommandLine.Execute(
                        "tar",
                        $"xvf {targzFile.FullName} -C {_omniSharpInstallFolder}");

                    omniSharpRunScript.Refresh();

                    if (!omniSharpRunScript.Exists)
                    {
                        return null;
                    }

                    operation.Succeed();
                }
            }

            return omniSharpRunScript;
        }

        private static async Task<FileInfo> AcquireAndExtractWithZip(string file)
        {
            var omniSharpExe = new FileInfo(
                Path.Combine(
                    _omniSharpInstallFolder.FullName,
                    "OmniSharp.exe"));

            if (!omniSharpExe.Exists)
            {
                using (var operation = Log.OnEnterAndConfirmOnExit())
                {
                    var zipFile = await Download(new Uri($@"https://github.com/OmniSharp/omnisharp-roslyn/releases/download/{_version}/{file}"));

                    using (var stream = zipFile.OpenRead())
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        archive.ExtractToDirectory(_omniSharpInstallFolder.FullName);
                    }

                    omniSharpExe.Refresh();

                    if (!omniSharpExe.Exists)
                    {
                        return null;
                    }

                    operation.Succeed();
                }
            }

            return omniSharpExe;
        }

        private static async Task<FileInfo> Download(Uri uri)
        {
            var fileToWriteTo = Path.GetTempFileName();

            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetStreamAsync(uri))
            using (Stream streamToWriteTo = File.Open(fileToWriteTo, FileMode.Create))
            {
                await response.CopyToAsync(streamToWriteTo);
            }

            return new FileInfo(fileToWriteTo);
        }
    }
}
