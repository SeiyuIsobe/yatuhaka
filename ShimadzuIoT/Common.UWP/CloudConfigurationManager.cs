using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common
{
    public class CloudConfigurationManager
    {
        private static Dictionary<string, string> _db = new Dictionary<string, string>()
        {
            {"ActionMappingStoreBlobName", "mappings.json"},
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
            {"DevicePollIntervalSeconds", "20" },
            {"ActionMappingStoreContainerName", "actionmappings"}
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
    }
}
