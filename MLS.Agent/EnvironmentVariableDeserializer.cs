using System;
using System.Collections.Generic;
using System.Reflection;
using static Pocket.Logger<MLS.Agent.EnvironmentVariableDeserializer>;
using Newtonsoft.Json;
using Pocket;

namespace MLS.Agent
{
    public class ShortNameAttribute : Attribute
    {
        public string Name;

        public ShortNameAttribute(string v)
        {
            this.Name = v;
        }
    }
    public class RealEnvironmentVariableAccess : IEnvironmentVariableAccess
    {
        public string Get(string name) => System.Environment.GetEnvironmentVariable(name);
    }

    public interface IEnvironmentVariableAccess
    {
        string Get(string name);
    }

    public static class EnvironmentVariableDeserializer
    {
        public static T DeserializeFromEnvVars<T>(IEnvironmentVariableAccess envAccess = null) where T : class
        {
            return DeserializeFromEnvVars(typeof(T), envAccess) as T;
        }

        public static object DeserializeFromEnvVars(Type type, IEnvironmentVariableAccess envAccess = null)
        {
            envAccess = envAccess ?? new RealEnvironmentVariableAccess();

            var typeName = type.Name;
            var shortNames = (ShortNameAttribute[])type.GetCustomAttributes(typeof(ShortNameAttribute), inherit: false);
            if (shortNames.Length > 0)
            {
                typeName = typeof(ShortNameAttribute).GetField("Name").GetValue(shortNames[0]) as string;
            }

            var c = type.GetConstructor(new Type[] { });
            if (c is null)
            {
                Log.Warning("No no-args constructor for {Type}", null, type.FullName);
                return null;
            }
            var instance = c.Invoke(new object[] { });
            var gotAll = true;

            string LookupEnvVarForProperty(string propertyName)
            {
                var envVarKey = $"CUSTOMCONNSTR_{typeName}_{propertyName}";
                var envVarValue = envAccess.Get(envVarKey);
                if (envVarValue is null)
                {
                    Log.Warning("Environment variable lookup failed for {EnvVarKey}", null, envVarKey);
                    gotAll = false;
                }
                return envVarValue;
            }

            foreach (var property in type.GetProperties(~BindingFlags.Static))
            {
                var propertyName = property.Name;
                var propertyShortNames = (ShortNameAttribute[])property.GetCustomAttributes(typeof(ShortNameAttribute), inherit: false);
                if (propertyShortNames.Length > 0)
                {
                    propertyName = typeof(ShortNameAttribute).GetField("Name").GetValue(propertyShortNames[0]) as string;
                }

                if (property.GetSetMethod() is null)
                {
                    Log.Warning("Environment Variable Parsing: No setter for {Name}", null, propertyName);
                    gotAll = false;
                    continue;
                }

                if (property.PropertyType == typeof(string))
                {
                    var envVarValue = LookupEnvVarForProperty(propertyName);
                    if (envVarValue is null)
                    {
                        continue;
                    }

                    property.SetValue(instance, envVarValue);
                }
                else if (property.PropertyType == typeof(bool))
                {
                    var envVarValue = LookupEnvVarForProperty(propertyName);
                    if (envVarValue is null)
                    {
                        continue;
                    }
                    if (!bool.TryParse(envVarValue, out var boolValue))
                    {
                        Log.Warning("Environment variable parsing to bool failed for {Name}, expected boolean, got \"{Value}\"", null, propertyName, envVarValue);
                        continue;
                    }
                    property.SetValue(instance, boolValue);
                }
                else if (property.PropertyType == typeof(int))
                {
                    var envVarValue = LookupEnvVarForProperty(propertyName);
                    if (envVarValue is null)
                    {
                        continue;
                    }
                    if (!int.TryParse(envVarValue, out var intValue))
                    {
                        Log.Warning("Environment variable parsing to int failed for {Name}, expected integer, got \"{Value}\"", null, propertyName, envVarValue);
                        continue;
                    }
                    property.SetValue(instance, intValue);
                }
                else if (
                    property.PropertyType.GenericTypeArguments.Length == 2 &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var envVarValue = LookupEnvVarForProperty(propertyName);
                    if (envVarValue is null)
                    {
                        continue;
                    }
                    property.SetValue(instance, JsonConvert.DeserializeObject(envVarValue, property.PropertyType));
                }
                else if (property.PropertyType.IsClass)
                {
                    var recField = DeserializeFromEnvVars(property.PropertyType, envAccess);
                    if (recField is null)
                    {
                        gotAll = false;
                        continue;
                    }
                    property.SetValue(instance, recField);
                }
                else
                {
                    var envVarValue = LookupEnvVarForProperty(propertyName);
                    Log.Warning("Unexpected type {PropertyType} during environment variable parsing", null, property.PropertyType.FullName);
                }
            }

            return gotAll ? instance : null;
        }
    }
}
