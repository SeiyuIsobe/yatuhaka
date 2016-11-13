﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public class ApiRegistrationRepository : IApiRegistrationRepository
    {
        private const string API_TABLE_NAME = "ApiRegistration";
        private readonly IAzureTableStorageClient _azureTableStorageClient;

        public ApiRegistrationRepository(IConfigurationProvider configProvider, IAzureTableStorageClientFactory tableStorageClientFactory)
        {
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(configProvider.GetConfigurationSettingValue("device.StorageConnectionString"), API_TABLE_NAME);
        }

        public bool AmendRegistration(ApiRegistrationModel apiRegistrationModel)
        {
            try
            {
                var incomingEntity = new ApiRegistrationTableEntity()
                {
                    Password = apiRegistrationModel.Password,
                    BaseUrl = apiRegistrationModel.BaseUrl,
                    Username = apiRegistrationModel.Username,
                    LicenceKey = apiRegistrationModel.LicenceKey
                };

                _azureTableStorageClient.ExecuteAsync(TableOperation.InsertOrMerge(incomingEntity));
            }
            catch (StorageException)
            {
                return false;
            }
            return true;
        }

        public ApiRegistrationModel RecieveDetails()
        {
            var query = new TableQuery<ApiRegistrationTableEntity>().
                Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                    ApiRegistrationTableEntity.GetPartitionKey(ApiRegistrationProviderType.Jasper)));

            var response = _azureTableStorageClient.ExecuteQueryAsync(query);
            if (response == null) return new ApiRegistrationModel();

            var apiRegistrationTableEntities = response as IList<ApiRegistrationTableEntity> /*?? response.ToList()*/;
            var apiRegistrationTableEntity = apiRegistrationTableEntities.FirstOrDefault();

            if (apiRegistrationTableEntity == null) return new ApiRegistrationModel();

            return new ApiRegistrationModel()
            {
                Username = apiRegistrationTableEntity.Username,
                BaseUrl = apiRegistrationTableEntity.BaseUrl,
                LicenceKey = apiRegistrationTableEntity.LicenceKey,
                Password = apiRegistrationTableEntity.Password
            };

        }

        public bool IsApiRegisteredInAzure()
        {
            var retrieveOperation = TableOperation.Retrieve<ApiRegistrationTableEntity>(ApiRegistrationTableEntity.GetPartitionKey(ApiRegistrationProviderType.Jasper),
                                        ApiRegistrationTableEntity.GetRowKey(ApiRegistrationProviderType.Jasper));
            var retrievedResult = _azureTableStorageClient.ExecuteAsync(retrieveOperation);
            return retrievedResult.Result != null;
        }

        public bool DeleteApiDetails()
        {
            var entity = new DynamicTableEntity(ApiRegistrationTableEntity.GetPartitionKey(ApiRegistrationProviderType.Jasper),
                                ApiRegistrationTableEntity.GetRowKey(ApiRegistrationProviderType.Jasper));
            entity.ETag = "*";
            _azureTableStorageClient.ExecuteAsync(TableOperation.Delete(entity));
            return true;
        }
    }
}