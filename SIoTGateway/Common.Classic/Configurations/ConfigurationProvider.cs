using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

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

#if WINDOWS_UWP
        private CloudConfigurationManager _cloud = null;
        private string _return_string = string.Empty;
        public string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue)
        {
            //string configValue = CloudConfigurationManager.GetSetting(configurationSettingName);

            _return_string = string.Empty;

            if(null == _cloud)
            {
                _cloud = new CloudConfigurationManager();

                _cloud.Completed += (sender, e) =>
                {
                    string configValue = _cloud.GetSetting(configurationSettingName, true);

                    if (configValue.StartsWith(ConfigToken, StringComparison.OrdinalIgnoreCase))
                    {
                        if (environment == null)
                        {
                            LoadEnviromentConfigAsync().Wait();
                        }

                        configValue = environment.GetSetting(
                            configValue.Substring(configValue.IndexOf(ConfigToken, StringComparison.Ordinal) + ConfigToken.Length));
                    }

                    try
                    {
                        if (false == configuration.ContainsKey(configurationSettingName))
                        {
                            this.configuration.Add(configurationSettingName, configValue);
                        }
                        else
                        {
                            // exist
                        }
                    }
                    catch (ArgumentException)
                    {
                        // at this point, this key has already been added on a different
                        // thread, so we're fine to continue
                    }

                    _return_string = this.configuration[configurationSettingName];
                };
                _cloud.InitAsync();

                while (true)
                {
                    if (_return_string != string.Empty) break;
                }
            }
            else
            {
                string configValue = _cloud.GetSetting(configurationSettingName, true);

                if (configValue.StartsWith(ConfigToken, StringComparison.OrdinalIgnoreCase))
                {
                    if (environment == null)
                    {
                        LoadEnviromentConfigAsync().Wait();
                    }

                    configValue = environment.GetSetting(
                        configValue.Substring(configValue.IndexOf(ConfigToken, StringComparison.Ordinal) + ConfigToken.Length));
                }

                try
                {
                    if (false == configuration.ContainsKey(configurationSettingName))
                    {
                        this.configuration.Add(configurationSettingName, configValue);
                    }
                    else
                    {
                        // exist
                    }
                }
                catch (ArgumentException)
                {
                    // at this point, this key has already been added on a different
                    // thread, so we're fine to continue
                }

                _return_string = this.configuration[configurationSettingName];
            }

            return _return_string;

        }
#endif
#if !WINDOWS_UWP
        public string GetConfigurationSettingValueOrDefault(string configurationSettingName, string defaultValue)
        {

                if (!this.configuration.ContainsKey(configurationSettingName))
                {
                    string configValue = CloudConfigurationManager.GetSetting(configurationSettingName);

                    if ((configValue != null && configValue.StartsWith(ConfigToken, StringComparison.OrdinalIgnoreCase)))
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
#endif
#if WINDOWS_UWP
        //void LoadEnvironmentConfig()
        //{
        //    if (true == File.Exists("Common\\cloud.settings.xml"))
        //    {
        //        this.environment = new EnvironmentDescription("Common\\cloud.settings.xml");
        //        this.environment.InitAsync();
        //    }
        //}

        private EnvironmentDescription _env = null;

        private async Task LoadEnviromentConfigAsync()
        {
            if (null == _env)
            {
                if (true == File.Exists("Common\\cloud.settings.xml"))
                {
                    this.environment = new EnvironmentDescription("Common\\cloud.settings.xml");
                    await this.environment.InitAsync();
                }
            }
            else
            {
                // no action
            }
        }
#endif
#if !WINDOWS_UWP
        void LoadEnvironmentConfig()
        {
            if (true == File.Exists("cloud.settings.xml"))
            {
                this.environment = new EnvironmentDescription("cloud.settings.xml");
            }
        }
#endif

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
#if !WINDOWS_UWP
                if (environment != null)
                {
                    environment.Dispose();
                }
#endif
            }

            _disposed = true;
        }

        ~ConfigurationProvider()
        {
            Dispose(false);
        }
    }
}
