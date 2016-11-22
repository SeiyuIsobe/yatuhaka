using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.DataInitialization
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

        public void CreateInitialDataIfNeeded()
        {
            try
            {
                List<string> bootstrappedDevices = null;

                // 1) create default devices
                Task.Run(async () => bootstrappedDevices = await _deviceLogic.BootstrapDefaultDevices()).Wait();

                // 2) create default rules
                //Task.Run(() => _deviceRulesLogic.BootstrapDefaultRulesAsync(bootstrappedDevices)).Wait();

                // 3) create action mappings (do this last to ensure that we'll try to 
                //    recreate if any of the above throws)
                //_actionMappingLogic.InitializeDataIfNecessaryAsync();
            } 
            catch (Exception ex)
            {
                Trace.TraceError("Failed to create initial default data: {0}", ex.ToString());
            }
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
    }
}
