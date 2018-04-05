using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Pocket;
using Recipes;

using static Pocket.Logger<MLS.Agent.AgentConfiguration>;
namespace MLS.Agent
{
    public class AgentConfiguration : IServiceProvider
    {
        private readonly PocketContainer _container;

        public AgentConfiguration(IConfigurationRoot configurationRoot, IHostingEnvironment environment)
        {
            if (configurationRoot == null)
            {
                throw new ArgumentNullException(nameof(configurationRoot));
            }


            var environment1 = environment ?? throw new ArgumentNullException(nameof(environment));

            _container = new PocketContainer();
            _container.AddStrategy(type =>
            {
                if (type.Name.EndsWith("Settings"))
                {
                var settings = configurationRoot.For(type);

                object envVarSettings = null;
                try
                {
                    envVarSettings = EnvironmentVariableDeserializer.DeserializeFromEnvVars(type);
                    if (settings != null && envVarSettings == null)
                    {
                        Log.Warning("environment variable strategy: failed to deserialize {FullName}", null, type.FullName);
                    }
                    if (settings is null && envVarSettings != null)
                    {
                        Log.Info("environment variable strategy: no original settings class for {}", type.FullName);
                    }
                    else if (settings != null && envVarSettings != null)
                    {
                        if (!settings.Equals(envVarSettings))
                        {
                            Log.Warning(
                                "environment variable strategy: not equal deserializations for {FullName}, {Other} VS {Env}",
                                null,
                                type.FullName,
                                settings.ToString(),
                                envVarSettings.ToString());
                        }
                        else
                        {
                            Log.Info("environment variable strategy: successfully deserialized {}", type.FullName);
                        }
                    }

                }
                catch (Exception e)
                {
                    Log.Error("environment variable strategy: exception during deserialization and comparison for {FullName}", e, type.FullName);
                }

                if (envVarSettings != null)
                {
                    settings = envVarSettings;
                }

                _container.RegisterSingle(type, c => settings);

                return c => c.Resolve(type);
                }

                return null;
            });
                ConfigureForAllEnvironments();

            if (environment1.IsProduction())
            {
                ConfigureForProduction();
            }
            else
            {
                ConfigureForLocal();

                if (environment1.IsTest())
                {
                    ConfigureForTest();
                }

                if (environment1.IsDevelopment())
                {
                    ConfigureForDevelopment();
                }
            }

        }

        private void ConfigureForDevelopment()
        {
            _container.RegisterSingle(c => new WorkspaceSettings { CanRun = true });
        }

        private void ConfigureForTest()
        {
            _container.RegisterSingle(c => new WorkspaceSettings { CanRun = true });
        }

        private void ConfigureForLocal()
        {
            _container.RegisterSingle(c => new WorkspaceSettings { CanRun = true });
        }

        private void ConfigureForProduction()
        {
            _container.RegisterSingle(c => EnvironmentVariableDeserializer.DeserializeFromEnvVars<WorkspaceSettings>());
        }

        private void ConfigureForAllEnvironments()
        {
        }

        public object GetService(Type serviceType)
        {
            return _container.Resolve(serviceType);
        }
    }
}