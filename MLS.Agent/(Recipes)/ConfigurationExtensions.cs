using System;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
namespace Recipes
{
    internal static class ConfigurationExtensions
    {
        public static object For(this IConfiguration configuration, Type settingsType)
        {
            var configurationSection = configuration.GetSection(settingsType.Name);

            object settings;

            if (configurationSection.Value != null &&
                settingsType != typeof(string))
            {
                settings = JsonConvert.DeserializeObject(configurationSection.Value, settingsType);
            }
            else
            {
                settings = configurationSection.Get(settingsType);
            }

            return settings ?? Activator.CreateInstance(settingsType);
        }
    }
}
