using System;
using System.IO;

namespace MLS.Agent
{
    public class StartupOptions
    {
        public StartupOptions(
            bool production = false,
            bool languageService = false,
            string key = null,
            string applicationInsightsKey = null,
            bool logToFile = false,
            string id = null,
            string regionId = null,
            DirectoryInfo rootDirectory = null,
            DirectoryInfo addSource = null,
            Uri uri = null)
        {
            LogToFile = logToFile;
            Id = id;
            Production = production;
            IsLanguageService = languageService;
            Key = key;
            ApplicationInsightsKey = applicationInsightsKey;
            RegionId = regionId;
            RootDirectory = rootDirectory;
            AddSource = addSource;
            Uri = uri;
        }

        public string Id { get; set; }
        public string RegionId { get; set; }
        public DirectoryInfo RootDirectory { get; set; }
        public DirectoryInfo AddSource { get; }
        public Uri Uri { get; }
        public bool Production { get; set; }
        public bool IsLanguageService { get; set; }
        public string Key { get; set; }
        public string ApplicationInsightsKey { get; set; }
        public bool LogToFile { get; set; }
        public bool IsInHostedMode => RootDirectory == null;

        public string EnvironmentName =>
            Production || !IsInHostedMode
                ? Microsoft.AspNetCore.Hosting.EnvironmentName.Production
                : Microsoft.AspNetCore.Hosting.EnvironmentName.Development;
    }
}