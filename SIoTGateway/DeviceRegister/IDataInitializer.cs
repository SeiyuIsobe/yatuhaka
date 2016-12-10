using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeviceRegister
{
    /// <summary>
     /// Represents component to create initial data for the system
     /// </summary>
    public interface IDataInitializer
    {
        void CreateInitialDataIfNeeded();
        Task<List<DeviceModel>> GetAllDevicesAsync();
        Task<DeviceModel> UpdateDeviceAsync(DeviceModel device);
        Task<string> RegistDeviceId(string deviceId);
    }
}