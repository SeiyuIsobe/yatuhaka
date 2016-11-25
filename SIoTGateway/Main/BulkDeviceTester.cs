using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Newtonsoft.Json;
using SIoTGateway.Cooler.Devices.Factory;
using SIoTGateway.Cooler.Telemetry.Factory;
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
using Windows.Web.Http;

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

        private TelemetryFactoryResolver _telemetryFactoryResolver = new TelemetryFactoryResolver();
        private DeviceFactoryResolver _deviceFactoryResolver = new DeviceFactoryResolver();

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

            #region ここに使うセンサーのファクトリーを登録する
            // デバイスファクトリーの解決装置
            _deviceFactoryResolver.Add(new CoolerDeviceFactory());
            _deviceFactoryResolver.Add(new ShimadzuIoT.Sensors.Acceleration.Devices.Factory.DeviceFactory());

            // テレメトリーファクトリーの解決装置
            _telemetryFactoryResolver.Add(new CoolerTelemetryFactory(_logger));
            _telemetryFactoryResolver.Add(new ShimadzuIoT.Sensors.Acceleration.Telemetry.Factory.TelemetryFactory(_logger));
            #endregion
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
                    // センサー基盤と繋がっていない間は何もしない
                    // 繋がっている場合は_deviceListに要素がある
                    if (null != _deviceList && _deviceList.Count > 0)
                    {
                        var newDevicesOnCloud = new List<InitialDeviceConfig>();
                        var newDevices = new List<InitialDeviceConfig>();
                        var removedDevices = new List<string>();

                        // Azureからデバイスのリストを取得する
                        // 「無効」になっているデバイスは取得しない
                        var devices = await _deviceStorage.GetDeviceListAsync();

                        if (devices != null && devices.Any())
                        {
                            // センサー基盤に繋がっているデバイスリストとWebで「有効」になっているデバイスリストを
                            // 突き合わせて、一致するものを取得する
                            // クラウド側の一致したものをnewDevicesOnCloudに格納
                            // センサー基盤の一致したものをnewDevicesに格納
                            newDevicesOnCloud = devices.Where(d => _deviceList.Any(x => x.DeviceId == d.DeviceId)).ToList();
                            newDevices = _deviceList.Where(d => devices.Any(x => x.DeviceId == d.DeviceId)).ToList();

                            // 
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
                        }
                        if (removedDevices.Count > 0)
                        {
                        }

                        if (removedDevices.Any())
                        {
                            //stop processing any devices that have been removed
                            dm.StopDevices(removedDevices);
                        }

                        //begin processing any new devices that were retrieved
                        //
                        // デバイス
                        // クラウドにあってこちらに無いデバイス
                        if (newDevices.Any())
                        {
                            var devicesToProcess = new List<IDevice>();

                            // 
                            foreach (var deviceConfig in newDevices)
                            {
                                // 起動直後でインスタンスが生成されていないデバイス
                                if(null == deviceConfig.Key)
                                {
                                    // キーはクラウド側が持っているので
                                    // クラウド側を絞り出す
                                    var config = newDevicesOnCloud.Where(d => d.DeviceId == deviceConfig.DeviceId).ToList();

                                    // テレメトリー
                                    var telemetryFactory = _telemetryFactoryResolver.Resolve(deviceConfig.DeviceId);

                                    // デバイス
                                    var df = _deviceFactoryResolver.Resolve(deviceConfig.DeviceId);
                                    var device = ((IDeviceFactory)df).CreateDevice(_logger, _transportFactory, (ITelemetryFactory)telemetryFactory, _configProvider, config[0]);

                                    // リストに追加
                                    devicesToProcess.Add(device);
                                }
                            }

                            // 一致するデバイスを保存する
                            _deviceList = newDevicesOnCloud;

#pragma warning disable 4014
                            //don't wait for this to finish
                            dm.StartDevicesAsync(devicesToProcess);
#pragma warning restore 4014
                        }
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

        public void SetDevice(List<string> list)
        {
            if(null == _deviceList)
            {
                _deviceList = new List<InitialDeviceConfig>();
            }
            else
            {
                _deviceList.Clear();
            }

            foreach(string name in list)
            {
                _deviceList.Add(new InitialDeviceConfig() { DeviceId = name });
            }
        }
    }
}
