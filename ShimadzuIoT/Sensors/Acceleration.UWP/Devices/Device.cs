using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using ShimadzuIoT.Sensors.Acceleration.Telemetry;
using ShimadzuIoTGatewayCore.Devices;
using ShimadzuIoTGatewayCore.Logging;
using ShimadzuIoTGatewayCore.Telemetry.Factory;
using ShimadzuIoTGatewayCore.Transport.Factory;
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
        public Device(ILogger logger, ITransportFactory transportFactory,
               ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider)
            : base(logger, transportFactory, telemetryFactory, configurationProvider)
        {
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
    }
}
