using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json;
using ShimadzuIoT.Sensors.Atomopshere.CommandProcessors;
using ShimadzuIoT.Sensors.Atomopshere.Telemetry;
using ShimadzuIoT.Sensors.Common.CommandParameters;
using ShimadzuIoT.Sensors.Common.CommandProcessors;
using SIotGatewayCore.Devices;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Telemetry.Factory;
using SIotGatewayCore.Transport.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimadzuIoT.Sensors.Atomopshere.Devices
{
    /// <summary>
    /// 加速度センサー
    /// </summary>
    public class Device : DeviceBase
    {
        // センサー制御値
        private OperationValue _operationValueDefault = new OperationValue();

        public Device(ILogger logger, ITransportFactory transportFactory,
               ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider)
            : base(logger, transportFactory, telemetryFactory, configurationProvider)
        {
            //_operationValue = JsonConvert.DeserializeObject<OperationValue>(base._operationValueStream);
        }

        protected override void InitCommandProcessors()
        {
            var startCommandProcessor = new StartCommandProcessor(this);
            var stopCommandProcessor = new StopCommandProcessor(this);
            var changeElapseTimeCommandProcessor = new ChangeElapseTimeCommandProcessor(this);

            startCommandProcessor.NextCommandProcessor = stopCommandProcessor;
            stopCommandProcessor.NextCommandProcessor = changeElapseTimeCommandProcessor;

            RootCommandProcessor = startCommandProcessor;

        }

        public override async void OnStartTelemetryCommand()
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            remoteMonitorTelemetry.TelemetryActive = true;

            // DeviceModelを更新
            var operationValue = JsonConvert.DeserializeObject<OperationValue>(base.InitialDevice.OperationValue);
            var param = operationValue.IsAvailableCommandParameter;

            // フラグを更新
            param.IsAvailable = remoteMonitorTelemetry.TelemetryActive;

            // 更新
            base.InitialDevice.OperationValue = JsonConvert.SerializeObject(operationValue);

            // クラウド
            base.InitialDevice.ObjectType = "DeviceInfo";
            await((RemoteMonitorTelemetry)base.TelemetryController).SendDeviceModelAsync(base.InitialDevice);
        }

        public override async void OnStopTelemetryCommnad()
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            remoteMonitorTelemetry.TelemetryActive = false;

            // DeviceModelを更新
            var operationValue = JsonConvert.DeserializeObject<OperationValue>(base.InitialDevice.OperationValue);
            var param = operationValue.IsAvailableCommandParameter;

            // フラグを更新
            param.IsAvailable = remoteMonitorTelemetry.TelemetryActive;

            // 更新
            base.InitialDevice.OperationValue = JsonConvert.SerializeObject(operationValue);

            // クラウド
            base.InitialDevice.ObjectType = "DeviceInfo";
            await ((RemoteMonitorTelemetry)base.TelemetryController).SendDeviceModelAsync(base.InitialDevice);
        }

        public void OnChangeElapseTime(int time)
        {

        }

        // センサー制御値
        public override object OperationValue
        {
            get
            {
                try
                {
                    return JsonConvert.DeserializeObject<OperationValue>(base._operationValueStream);
                }
                catch
                {
                    return new OperationValue();
                }
                
            }
        }

        public override string OperationValueDefault
        {
            get
            {
                return JsonConvert.SerializeObject(new OperationValue());
            }
        }

        /// <summary>
        /// センサー値から得られるデータを定義する
        /// </summary>
        /// <param name="dm"></param>
        override public void AssignTelemetry(DeviceModel dm)
        {
            dm.Telemetry.Add(new Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Telemetry("Atomos", "Atom (hPa)", "double"));
        }

        /// <summary>
        /// コマンドを定義する
        /// </summary>
        /// <param name="dm"></param>
        override public void AssignCommands(DeviceModel dm)
        {
            dm.Commands.Add(
               new Command(
                   StartCommandProcessor.CommandName
                   ));

            dm.Commands.Add(
                new Command(
                    StopCommandProcessor.CommandName
                    ));
        }

        /// <summary>
        /// 初期値を定義する
        /// </summary>
        /// <param name="dm"></param>
        override public void AssignOperationValue(DeviceModel dm)
        {
            dm.OperationValue = this.OperationValueDefault;
        }
    }
}
