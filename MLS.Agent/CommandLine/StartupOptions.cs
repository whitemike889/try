using System;
using System.IO;
using Microsoft.DotNet.Try.Markdown;

namespace MLS.Agent.CommandLine
{
    public class StartupOptions : IDefaultCodeBlockAnnotations
    {
        public StartupOptions(
            bool production = false,
            bool languageService = false,
            string key = null,
            string applicationInsightsKey = null,
            string id = null,
            string regionId = null,
            DirectoryInfo rootDirectory = null,
            DirectoryInfo addSource = null,
            Uri uri = null,
            DirectoryInfo logPath = null,
            bool verbose = false,
            bool enablePreviewFeatures = false,
            string package = null,
            string packageVersion = null)
        {
            LogPath = logPath;
            Verbose = verbose;
            Id = id;
            Production = production;
            IsLanguageService = languageService;
            Key = key;
            ApplicationInsightsKey = applicationInsightsKey;
            RegionId = regionId;
            RootDirectory = rootDirectory;
            AddSource = addSource;
            Uri = uri;
            EnablePreviewFeatures = enablePreviewFeatures;
            Package = package;
            PackageVersion = packageVersion;
        }

        public bool EnablePreviewFeatures { get; }
        public string Id { get; }
        public string RegionId { get; }
        public DirectoryInfo RootDirectory { get; }
        public DirectoryInfo AddSource { get; }
        public Uri Uri { get; }
        public bool Production { get; }
        public bool IsLanguageService { get; set; }
        public string Key { get; }
        public string ApplicationInsightsKey { get; }

        public StartupMode Mode => RootDirectory == null
                                       ? StartupMode.Hosted
                                       : StartupMode.Try; 

        public string EnvironmentName =>
            Production || Mode != StartupMode.Hosted
                ? Microsoft.AspNetCore.Hosting.EnvironmentName.Production
                : Microsoft.AspNetCore.Hosting.EnvironmentName.Development;

        public DirectoryInfo LogPath { get;  }

        public bool Verbose { get; }

        public string Package { get; }

        public string PackageVersion { get; }
    }
}