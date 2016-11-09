using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using SIotGatewayCore.Devices;
using SIotGatewayCore.Devices.Factory;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Telemetry.Factory;
using SIotGatewayCore.Transport.Factory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Main
{
    public class BulkDeviceTester
    {
        // change this to inject a different logger
        private readonly ILogger _logger;
        private readonly ITransportFactory _transportFactory;
        private readonly IConfigurationProvider _configProvider;
        private readonly ITelemetryFactory _telemetryFactory;
        private readonly IDeviceFactory _deviceFactory;
        private readonly IVirtualDeviceStorage _deviceStorage;

        private List<InitialDeviceConfig> _deviceList;
        private readonly int _devicePollIntervalSeconds;

        private const int DEFAULT_DEVICE_POLL_INTERVAL_SECONDS = 120;

        public BulkDeviceTester(ITransportFactory transportFactory, ILogger logger, IConfigurationProvider configProvider,
            ITelemetryFactory telemetryFactory, IDeviceFactory deviceFactory, IVirtualDeviceStorage virtualDeviceStorage)
        {
            _transportFactory = transportFactory;
            _logger = logger;
            _configProvider = configProvider;
            _telemetryFactory = telemetryFactory;
            _deviceFactory = deviceFactory;
            _deviceStorage = virtualDeviceStorage;
            _deviceList = new List<InitialDeviceConfig>();

            string pollingIntervalString = _configProvider.GetConfigurationSettingValueOrDefault(
                                        "DevicePollIntervalSeconds",
                                        DEFAULT_DEVICE_POLL_INTERVAL_SECONDS.ToString(CultureInfo.InvariantCulture));

            _devicePollIntervalSeconds = Convert.ToInt32(pollingIntervalString, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Retrieves a set of device configs from the repository and creates devices with this information
        /// Once the devices are built, they are started
        /// </summary>
        /// <param name="token"></param>
        public async Task ProcessDevicesAsync(CancellationToken token)
        {
            var dm = new DeviceManager(_logger, token);

            try
            {
                // IoT開始
                //
                // なぜここはwhileで無限ループしているか？
                // それは定期的にAzureでデバイスの有効・無効を確認するため
                //
                while (!token.IsCancellationRequested)
                {
                    var newDevices = new List<InitialDeviceConfig>();
                    var removedDevices = new List<string>();

                    // Azureからデバイスのリストを取得する
                    // 「無効」になっているデバイスは取得しない
                    var devices = await _deviceStorage.GetDeviceListAsync();

                    if (devices != null && devices.Any())
                    {
                        newDevices = devices.Where(d => !_deviceList.Any(x => x.DeviceId == d.DeviceId)).ToList();
                        removedDevices =
                            _deviceList.Where(d => !devices.Any(x => x.DeviceId == d.DeviceId))
                                .Select(x => x.DeviceId)
                                .ToList();
                    }
                    else if (_deviceList != null && _deviceList.Any())
                    {
                        removedDevices = _deviceList.Select(x => x.DeviceId).ToList();
                    }

                    if (newDevices.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("-> New Device found.");
                        //_logger.LogInfo("********** {0} NEW DEVICES FOUND ********** ", newDevices.Count);
                    }
                    if (removedDevices.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine("-> Device Removed.");
                        //_logger.LogInfo("********** {0} DEVICES REMOVED ********** ", removedDevices.Count);
                    }


                    //reset the base list of devices for comparison the next
                    //time we retrieve the device list
                    _deviceList = devices;

                    if (removedDevices.Any())
                    {
                        //stop processing any devices that have been removed
                        dm.StopDevices(removedDevices);
                    }

                    //begin processing any new devices that were retrieved
                    if (newDevices.Any())
                    {
                        var devicesToProcess = new List<IDevice>();

                        foreach (var deviceConfig in newDevices)
                        {
                            //_logger.LogInfo("********** SETTING UP NEW DEVICE : {0} ********** ", deviceConfig.DeviceId);
                            System.Diagnostics.Debug.WriteLine($"-> SETTING UP NEW DEVICE : {deviceConfig.DeviceId}");
                            devicesToProcess.Add(_deviceFactory.CreateDevice(_logger, _transportFactory, _telemetryFactory, _configProvider, deviceConfig));
                        }

#pragma warning disable 4014
                        //don't wait for this to finish
                        dm.StartDevicesAsync(devicesToProcess);
#pragma warning restore 4014
                    }
                    await Task.Delay(TimeSpan.FromSeconds(_devicePollIntervalSeconds), token);
                }
            }
            catch (TaskCanceledException)
            {
                //do nothing if task was cancelled
                _logger.LogInfo("********** Primary worker role cancellation token source has been cancelled. **********");
            }
            finally
            {
                //ensure that all devices have been stopped
                dm.StopAllDevices();
            }
        }
    }
}
