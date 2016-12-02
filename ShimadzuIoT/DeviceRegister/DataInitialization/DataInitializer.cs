using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;

namespace DeviceRegister.DataInitialization
{
    public class DataInitializer : IDataInitializer
    {
        private readonly IActionMappingLogic _actionMappingLogic;
        private readonly IDeviceLogic _deviceLogic;
        private readonly IDeviceRulesLogic _deviceRulesLogic;

        public DataInitializer(
            IActionMappingLogic actionMappingLogic,
            IDeviceLogic deviceLogic,
            IDeviceRulesLogic deviceRulesLogic)
        {
            if (actionMappingLogic == null)
            {
                throw new ArgumentNullException("actionMappingLogic");
            }

            if (deviceLogic == null)
            {
                throw new ArgumentNullException("deviceLogic");
            }

            if (deviceRulesLogic == null)
            {
                throw new ArgumentNullException("deviceRulesLogic");
            }

            _actionMappingLogic = actionMappingLogic;
            _deviceLogic = deviceLogic;
            _deviceRulesLogic = deviceRulesLogic;
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
