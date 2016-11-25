using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
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
        public Device(ILogger logger, ITransportFactory transportFactory,
               ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider)
            : base(logger, transportFactory, telemetryFactory, configurationProvider)
        {
        }
    }
}
