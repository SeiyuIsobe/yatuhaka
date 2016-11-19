using Autofac;
using Main.DataInitialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using SIoTBroker;
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
    class SIoTGateway
    {
        private readonly Dictionary<string, CancellationTokenSource> _cancellationTokens;
        private SensorModuleWatcher _sensormoduleWatcher = null;

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        public SIoTGateway()
        {
            _cancellationTokens = new Dictionary<string, CancellationTokenSource>();

            // センサー基盤から送られてくるデバイス名の一覧を受信する
            _sensormoduleWatcher = new SensorModuleWatcher();
            _sensormoduleWatcher.ReceivedDeviceNames += (sender, e) =>
            {

            };


            // センサー基盤オブジェクトの生成
            // センサー基盤のIDを設定する
            SensorManageModule sensormodule = new SensorManageModule("SM19710613");
            sensormodule.ReceivedTelemetry += (sender, e) =>
            {
                if (null != ReceivedTelemetry)
                {
                    ReceivedTelemetry(sender, null);
                }
            };
            _sensorModuleList.Add(sensormodule);

            BuildContainer();
        }

        internal void ActivatedSensor(string sensorModuleID)
        {
            

            Stop();

            

            //Start();
        }

        private List<SensorManageModule> _sensorModuleList = new List<SensorManageModule>();
        private SIoTBroker.SIoTBroker _broker = new SIoTBroker.SIoTBroker();

        /// <summary>
        /// IoTゲートウェイサービスを開始する
        /// </summary>
        public async void Start()
        {
            // 断念
            // サンプルを参考にしても同じように動かない
            //var creator = _gatewayContainer.Resolve<IDataInitializer>();
            //creator.BootstrapDevice("aaaa23456789");

            await Task.Run(async () =>
            {
                var startDeviceTasks = new List<Task>();

                foreach (SensorManageModule smm in _sensorModuleList)
                {
                    var deviceCancellationToken = new CancellationTokenSource();

                    _cancellationTokens.Add("aaa", deviceCancellationToken);
                    startDeviceTasks.Add(smm.Start(deviceCancellationToken.Token));
                }

                await Task.WhenAll(startDeviceTasks);
            });
        }

        public void Stop()
        {
            foreach(KeyValuePair<string, CancellationTokenSource> kvp in _cancellationTokens)
            {
                kvp.Value.Cancel();
            }

            _cancellationTokens.Clear();
        }

        private IContainer _gatewayContainer;
        public void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new GatewayModule());
            _gatewayContainer = builder.Build();
        }
    }
}
