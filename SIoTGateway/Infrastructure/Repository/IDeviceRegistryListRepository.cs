using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using System.Collections.Generic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface IDeviceRegistryListRepository
    {
        /// <summary>
        /// Gets a list of type Device depending on search parameters, sort column, sort direction,
        /// starting point, page size, and filters.
        /// </summary>
        /// <param name="query">The device query.</param>
        /// <returns></returns>
        Task<DeviceListQueryResult> GetDeviceList(DeviceListQuery query);

        Task<List<DeviceModel>> GetDevicesAllAsync();
    }
}