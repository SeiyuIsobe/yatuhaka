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
            //string configValue = CloudConfigurationManager.GetSetting(configurationSettingName);

            var cloud = new CloudConfigurationManager();
            string configValue = cloud.GetSetting(configurationSettingName, true);

            if (configValue.StartsWith(ConfigToken, StringComparison.OrdinalIgnoreCase))
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
                if(false == configuration.ContainsKey(configurationSettingName))
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

            return this.configuration[configurationSettingName];
        }

        void LoadEnvironmentConfig()
        {
            //var executingPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

            // Check for build_output
            //int buildLocation = executingPath.IndexOf("Build_Output", StringComparison.OrdinalIgnoreCase);
            //if (buildLocation >= 0)
            //{
            //    //string fileName = executingPath.Substring(0, buildLocation) + "local.config.user";
            //    //if (File.Exists(fileName))
            //    //{

            //cloud.setting.xmlはlocal.config.userと同じ

#if ISEIYU
            if (true == File.Exists("Common\\cloud.settings.iseiyu.xml"))
            {
                this.environment = new EnvironmentDescription("Common\\cloud.settings.iseiyu.xml");
            }
#else
            if(true == File.Exists("Common\\cloud.settings.xml"))
            {
                this.environment = new EnvironmentDescription("Common\\cloud.settings.xml");
            }

#endif

            //    //    return;
            //    //}
            //}

            //// Web roles run in there app dir so look relative
            //int location = executingPath.IndexOf("Web\\bin", StringComparison.OrdinalIgnoreCase);

            //if (location == -1)
            //{
            //    location = executingPath.IndexOf("WebJob\\bin", StringComparison.OrdinalIgnoreCase);
            //}
            //if (location >=0)
            //{
            //    //string fileName = executingPath.Substring(0, location) + "..\\local.config.user";
            //    //if (File.Exists(fileName))
            //    //{
            //    //    this.environment = new EnvironmentDescription(fileName);
            //    //    return;
            //    //}
            //}

            //throw new ArgumentException("Unable to locate local.config.user file.  Make sure you have run 'build.cmd local'.");
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
                //if (environment != null)
                //{
                //    environment.Dispose();
                //}
            }

            _disposed = true;
        }

        ~ConfigurationProvider()
        {
            Dispose(false);
        }
    }
}
