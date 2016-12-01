using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Main.DataInitialization
{
    public interface IDataInitializer
    {
        void BootstrapDevice(string id);
        Task<List<DeviceModel>> GetAllDevicesAsync();
        Task<DeviceModel> UpdateDeviceAsync(DeviceModel device);
        Task<string> RegistDeviceId(string deviceId);
    }
}