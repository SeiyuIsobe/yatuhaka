using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations
{
    public class ConfigurationProvider : IConfigurationProvider, IDisposable
    {
        readonly Dictionary<string, string> configuration = new Dictionary<string, string>();
        EnvironmentDescription environment = null;
        const string ConfigToken = "config:";
        bool _disposed = false;

        public string GetConfigurationSettingValue(string configurationSettingName)
        {
            return this.GetConfigurationSettingValueOrDefault(configurationSettingName, string.Empty);
        }

        public string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue)
        {

            if (!this.configuration.ContainsKey(configurationSettingName))
            {
                string configValue = CloudConfigurationManager.GetSetting(configurationSettingName);
                System.Diagnostics.Debug.WriteLine($"->\t{configurationSettingName}\t{configValue}");

                //    bool isEmulated = Environment.CommandLine.Contains("iisexpress.exe") || 
                //        Environment.CommandLine.Contains("w3wp.exe") ||
                //        Environment.CommandLine.Contains("WebJob.vshost.exe");

                if (configValue != null && configValue.StartsWith(ConfigToken, StringComparison.OrdinalIgnoreCase))
                {
                    if (environment == null)
                    {
                        LoadEnvironmentConfig();
                    }

                    configValue = environment.GetSetting(
                            configValue.Substring(configValue.IndexOf(ConfigToken, StringComparison.Ordinal) + ConfigToken.Length));
                }
                try
                {
                    this.configuration.Add(configurationSettingName, configValue);
                }
                catch (ArgumentException)
                {
                    // at this point, this key has already been added on a different
                    // thread, so we're fine to continue
                }
            }

            return this.configuration[configurationSettingName];
        }

        void LoadEnvironmentConfig()
        {
            if (true == File.Exists("cloud.settings.xml"))
            {
                this.environment = new EnvironmentDescription("cloud.settings.xml");
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (environment != null)
                {
                    environment.Dispose();
                }
            }

            _disposed = true;
        }

        ~ConfigurationProvider()
        {
            Dispose(false);
        }
    }
}
