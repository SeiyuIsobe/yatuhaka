using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using Newtonsoft.Json;
using ShimadzuIoT.Sensors.Acceleration.Telemetry;
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
        //private OperationValue _operationValue = new OperationValue();

        public Device(ILogger logger, ITransportFactory transportFactory,
               ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider)
            : base(logger, transportFactory, telemetryFactory, configurationProvider)
        {
            //_operationValue = JsonConvert.DeserializeObject<OperationValue>(base._operationValueStream);
        }

        protected override void InitCommandProcessors()
        {

        }

        public void OnStartTelemetryCommand()
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            remoteMonitorTelemetry.TelemetryActive = true;
        }

        public void OnStopTelemetryCommnad()
        {
            var remoteMonitorTelemetry = (RemoteMonitorTelemetry)_telemetryController;
            remoteMonitorTelemetry.TelemetryActive = false;
        }

        public void OnChangeElapseTime(int time)
        {

        }

        // センサー制御値
        public override object OperationValue
        {
            get
            {
                return JsonConvert.DeserializeObject<OperationValue>(base._operationValueStream);
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
