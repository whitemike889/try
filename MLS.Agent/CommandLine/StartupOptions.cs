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
            DirectoryInfo dir = null,
            DirectoryInfo addPackageSource = null,
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
            Dir = dir ?? new DirectoryInfo(Directory.GetCurrentDirectory());
            AddPackageSource = addPackageSource;
            Uri = uri;
            EnablePreviewFeatures = enablePreviewFeatures;
            Package = package;
            PackageVersion = packageVersion;

            Mode = dir == null
                       ? StartupMode.Hosted
                       : StartupMode.Try;
        }

        public bool EnablePreviewFeatures { get; }
        public string Id { get; }
        public string RegionId { get; }
        public DirectoryInfo Dir { get; }
        public DirectoryInfo AddPackageSource { get; }
        public Uri Uri { get; set; }
        public bool Production { get; }
        public bool IsLanguageService { get; set; }
        public string Key { get; }
        public string ApplicationInsightsKey { get; }

        public StartupMode Mode { get; } 

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