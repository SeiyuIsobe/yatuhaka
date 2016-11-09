using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using SIoTGateway.Cooler.Devices.Factory;
using SIoTGateway.Cooler.Telemetry.Factory;
using SIotGatewayCore.Devices.Factory;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Transport.Factory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Main.Models
{
    /// <summary>
    /// センサー基板
    /// </summary>
    public class SensorManageModule
    {
        public string ModuleID { get; set; }

        /// <summary>
        /// IoTゲートウェイサービスを開始する
        /// </summary>
        public async Task Start(CancellationToken token)
        {
            //
            // BulkDeviceTesterに依存関係を注入する
            // 
            var logger = new TraceLogger();

            // Azureへの接続先を管理するオブジェクトを生成する
            var configProvider = new ConfigurationProvider();

            // BLOBのテーブルに接続するオブジェクトを生成する
            var tableStorageClientFactory = new AzureTableStorageClientFactory();

            // テレメトリー
            var telemetryFactory = new CoolerTelemetryFactory(logger);

            // Azure IoT Hubに接続するオブジェクトを生成する
            var transportFactory = new IotHubTransportFactory(logger, configProvider);

            IVirtualDeviceStorage deviceStorage = null;
            var useConfigforDeviceList = Convert.ToBoolean(configProvider.GetConfigurationSettingValueOrDefault("UseConfigForDeviceList", "False"), CultureInfo.InvariantCulture);

            // デバイスを登録しているAzure StrageのTableサービスに接続するオブジェクトを生成する
            deviceStorage = new VirtualDeviceTableStorage(configProvider, tableStorageClientFactory);

            IDeviceFactory deviceFactory = new CoolerDeviceFactory();

            //// Start Simulator
            var tester = new BulkDeviceTester(transportFactory, logger, configProvider, telemetryFactory, deviceFactory, deviceStorage);
            await Task.Run(() => tester.ProcessDevicesAsync(token), token);
        }
    }
}
