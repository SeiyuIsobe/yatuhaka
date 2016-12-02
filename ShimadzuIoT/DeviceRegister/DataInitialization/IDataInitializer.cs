using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceRegister.DataInitialization
{
    public interface IDataInitializer
    {
        Task<List<DeviceModel>> GetAllDevicesAsync();
        Task<DeviceModel> UpdateDeviceAsync(DeviceModel device);
        Task<string> RegistDeviceId(string deviceId);
    }
}
