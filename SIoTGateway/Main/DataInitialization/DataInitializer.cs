using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.DataInitialization
{
    public class DataInitializer : IDataInitializer
    {
        private readonly IDeviceLogic _deviceLogic;

        public DataInitializer(IDeviceLogic deviceLogic)
        {
            if (deviceLogic == null)
            {
                throw new ArgumentNullException("deviceLogic");
            }

            _deviceLogic = deviceLogic;
        }

        public void BootstrapDevice(string id)
        {
            string ret_string = string.Empty;

            Task.Run(async () => ret_string = await _deviceLogic.BootstrapDevice(id)).Wait();
        }

        public async Task<List<DeviceModel>> GetAllDevicesAsync()
        {
            return await _deviceLogic.GetAllDeviceAsync();
        }

        public async Task<DeviceModel> UpdateDeviceAsync(DeviceModel device)
        {
            // IoT Hub
            await _deviceLogic.UpdateDeviceEnabledStatusAsync(device.DeviceProperties.DeviceID, (bool)device.DeviceProperties.HubEnabledState);

            // DocumentDB
            return await _deviceLogic.UpdateDeviceAsync(device);
        }

        public async Task<string> RegistDeviceId(string deviceId)
        {
            // 既に登録されていると例外が飛ぶ
            try
            {
                return await _deviceLogic.BootstrapDefaultDevices(deviceId);
            }
            catch
            {
                return deviceId;
            }
        }
    }
}
