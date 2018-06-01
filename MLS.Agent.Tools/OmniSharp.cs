using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Pocket;

namespace MLS.Agent.Tools
{
    public class OmniSharp
    {
        protected AsyncLazy<FileInfo> _omniSharp { get; }

        private readonly DirectoryInfo _omniSharpInstallFolder;
        private readonly string _version;

        private static readonly ConcurrentDictionary<string, OmniSharp> _cache = new ConcurrentDictionary<string, OmniSharp>();
        private readonly Logger _log;
        private const string DefaultVersion = "v1.29.0-beta1";

        public OmniSharp(string version)
        {
            _version = version ?? throw new ArgumentNullException(nameof(version));

            _log = new Logger($"{nameof(OmniSharp)}:{_version}");

            var environmentVariable = Environment.GetEnvironmentVariable("TRYDOTNET_OMNISHARP_PATH");

            var omnisharpDirectory =
                environmentVariable ??
                Path.Combine(Paths.UserProfile,
                             ".trydotnet",
                             "omnisharp");

            _omniSharpInstallFolder = new DirectoryInfo(Path.Combine(omnisharpDirectory, version));

            _omniSharp = new AsyncLazy<FileInfo>(CheckInstallationAndAcquireIfNeeded);

            async Task<FileInfo> CheckInstallationAndAcquireIfNeeded()
            {
                using (_log.OnEnterAndExit())
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

                    _log.Info("Using OmniSharp at {path}", fileInfo);

                    return fileInfo;
                }
            }
        }

        public static async Task<FileInfo> EnsureInstalledOrAcquire(FileInfo dotTryDotNetPath)
        {
            var version = dotTryDotNetPath.Exists
                ? await dotTryDotNetPath.ReadAsync()
                : DefaultVersion;

            var omnisharp = _cache.GetOrAdd(version, v => new OmniSharp(v));
            return await omnisharp._omniSharp.ValueAsync();
        }
        
        private async Task<FileInfo> AcquireAndExtractWithTar(string file)
        {
            var omniSharpRunScript = new FileInfo(
                Path.Combine(
                    _omniSharpInstallFolder.FullName,
                    "run"));

            if (!omniSharpRunScript.Exists)
            {
#if DEBUG
                using (var operation = _log.OnEnterAndConfirmOnExit())
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
#else
                _log.Error($"Omnisharp not found at {omniSharpRunScript.FullName}");
                throw new InvalidOperationException($"Omnisharp not found at {omniSharpRunScript.FullName}");
#endif
            }

            return omniSharpRunScript;
        }

        private async Task<FileInfo> AcquireAndExtractWithZip(string file)
        {
            var omniSharpExe = new FileInfo(
                Path.Combine(
                    _omniSharpInstallFolder.FullName,
                    "OmniSharp.exe"));

            if (!omniSharpExe.Exists)
            {
#if DEBUG
                using (var operation = _log.OnEnterAndConfirmOnExit())
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
#else
                _log.Error($"Omnisharp not found at {omniSharpExe.FullName}");
                throw new InvalidOperationException($"Omnisharp not found at {omniSharpExe.FullName}");
#endif
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
