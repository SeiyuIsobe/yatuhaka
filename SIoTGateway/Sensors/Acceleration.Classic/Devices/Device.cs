using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json;
using ShimadzuIoT.Sensors.Acceleration.CommandProcessors;
using ShimadzuIoT.Sensors.Acceleration.Telemetry;
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

namespace ShimadzuIoT.Sensors.Acceleration.Devices
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

        public override void OnStartTelemetryCommand()
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
        }

        public override void OnStopTelemetryCommnad()
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

        //public override void SetOperationValue(string valuestream)
        //{
        //    var deviceModel = JsonConvert.DeserializeObject<DeviceModel>(valuestream);

        //    if (null != deviceModel && null != ((DeviceModel)deviceModel).OperationValue)
        //    {
        //        _operationValue = JsonConvert.DeserializeObject<OperationValue>(((DeviceModel)deviceModel).OperationValue.ToString());
        //    }
        //}
    }
}
