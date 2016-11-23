using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.DataInitialization
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
