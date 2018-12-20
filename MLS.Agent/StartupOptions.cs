using System;

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
            string regionId = null)
        {
            LogToFile = logToFile;
            Id = id;
            Production = production;
            IsLanguageService = languageService;
            Key = key;
            ApplicationInsightsKey = applicationInsightsKey;
            RegionId = regionId;
        }

        public string Id { get; set; }
        public string RegionId { get; set; }
        public bool Production { get; set; }
        public bool IsLanguageService { get; set; }
        public string Key { get; set; }
        public string ApplicationInsightsKey { get; set; }
        public bool LogToFile { get; set; }
    }
}