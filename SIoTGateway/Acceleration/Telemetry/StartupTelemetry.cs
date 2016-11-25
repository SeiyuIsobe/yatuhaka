using SIotGatewayCore.Devices;
using SIotGatewayCore.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimadzuIoT.Sensors.Acceleration.Telemetry
{
    public class StartupTelemetry : SensorsBase
    {
        public StartupTelemetry(ILogger logger, IDevice device)
            :base(logger, device)
        {

        }
    }
}
