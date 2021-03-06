﻿using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Newtonsoft.Json;
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
using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using SIotGatewayCore.Telemetry.Factory;
using Main.DataInitialization;

namespace Main.Models
{
    /// <summary>
    /// センサー基板
    /// </summary>
    public class SensorManageModule
    {
        public string ModuleID { get; private set; }
        private SensorModuleWatcher _sensormoduleWatcher = null;
        private SensorList _sensorlist = null;

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        public SensorManageModule(string moduleID)
        {
            this.ModuleID = moduleID;

            //// センサー基盤から送られてくるデバイス名の一覧を受信する
            //_sensormoduleWatcher = new SensorModuleWatcher(this.ModuleID);
            //_sensormoduleWatcher.ReceivedDeviceNames += (sender, e) =>
            //{
            //    _sensorlist = SensorList.ToObject(sender.ToString());
            //};
        }

        public SensorManageModule(SensorModule module)
        {
            this.ModuleID = module.Name;

            _sensorlist = module.Sensors;
        }

        //private BulkDeviceTester _tester = null;
        //private IContainer _gatewayContainer = null;

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
            ITelemetryFactory telemetryFactory = null;// new AnyTelemetryFactory(logger);
            //telemetryFactory.ReceivedTelemetry += (sender, e) =>
            //{
            //    if (null != ReceivedTelemetry)
            //    {
            //        ReceivedTelemetry(sender, e);
            //    }
            //};

            // Azure IoT Hubに接続するオブジェクトを生成する
            var transportFactory = new IotHubTransportFactory(logger, configProvider);

            IVirtualDeviceStorage deviceStorage = null;
            
            //ダミーのデバイスは使わないから不要
            //var useConfigforDeviceList = Convert.ToBoolean(configProvider.GetConfigurationSettingValueOrDefault("UseConfigForDeviceList", "False"), CultureInfo.InvariantCulture);

            // デバイスを登録しているAzure StrageのTableサービスに接続するオブジェクトを生成する
            deviceStorage = new VirtualDeviceTableStorage(configProvider, tableStorageClientFactory);

            //
            IDeviceFactory deviceFactory = null;// new AnyDeviceFactory();

            //// Start Simulator
            var _tester = new BulkDeviceTester(transportFactory, logger, configProvider, telemetryFactory, deviceFactory, deviceStorage);
            _tester.SetDevice(_sensorlist.Sensors);
            _tester.DataInitializer = this.DataInitializer;
            await Task.Run(() => _tester.ProcessDevicesAsync(token), token);
        }

        public IDataInitializer DataInitializer { get; set; }
    }
}
