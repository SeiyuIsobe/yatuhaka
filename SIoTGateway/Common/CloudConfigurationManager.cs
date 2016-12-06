using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common
{
    public class CloudConfigurationManager
    {
        private static Dictionary<string, string> _db = new Dictionary<string, string>()
        {
            {"device.StorageConnectionString","config:ServiceStoreAccountConnectionString"},
            {"device.TableName","config:DeviceTableName"},
            {"iotHub.ConnectionString","config:IotHubConnectionString"},
            {"docdb.EndpointUrl","config:DocDbEndPoint"},
            {"docdb.PrimaryAuthorizationKey","config:DocDBKey"},
            {"eventHub.HubName","config:ServiceEHName"},
            {"eventHub.ConnectionString","config:RulesEventHubName"},
            {"eventHub.StorageConnectionString","config:RulesEventHubConnectionString"},
            {"RulesEventHub.Name","config:AADClientId"},
            {"RulesEventHub.ConnectionString","config:AADInstance"},
            {"ObjectTypePrefix","config:AADTenant"},
            {"ida.AADClientId","actionmappings"},
            {"ida.AADInstance","actionmappings"},
            {"ida.AADTenant","actionmappings"},
            {"MapApiQueryKey","actionmappings"},
            {"iotHub.HostName","config:IotHubName"},
            {"DevicePollIntervalSeconds", "20" }
        };

        public static string GetSetting(string key)
        {
            try
            {
                return _db[key];
            }
            catch { }

            return "FALSE";
        }

        private XmlDocument _xdoc = null;
        private const string ValueAttributeName = "value";
        private const string SettingXpath = "//add[@name='{0}']";

        public CloudConfigurationManager()
        {
            _xdoc = new XmlDocument();
            using (XmlReader reader = XmlReader.Create("Common\\appconfig_clone.xml"))
            {
                _xdoc.Load(reader);
            }
        }

        public string GetSetting(string settingName, bool errorOnNull = true)
        {
            if (string.IsNullOrEmpty(settingName))
            {
                throw new ArgumentNullException("settingName");
            }

            string result = string.Empty;
            XmlNode node = this.GetSettingNode(settingName.Trim());
            if (node != null)
            {
                result = node.Attributes[ValueAttributeName].Value;
            }
            else
            {
                if (errorOnNull)
                {
                    var message = string.Format(CultureInfo.InvariantCulture, "{0} was not found", settingName);
                    throw new ArgumentException(message);
                }
            }
            return result;
        }

        private XmlNode GetSettingNode(string settingName)
        {
            string xpath = string.Format(CultureInfo.InvariantCulture, SettingXpath, settingName);
            var list = _xdoc.GetElementsByTagName("add");
            foreach (XmlNode xn in list)
            {
                var ss = xn.Attributes["key"];
                if (null != ss)
                {
                    if (ss.Value == settingName)
                    {
                        return xn;
                    }
                }
            }

            return null;
        }
    }
}
