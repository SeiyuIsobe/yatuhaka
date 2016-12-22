using SIotGatewayCore.Devices;
using SIotGatewayCore.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShimadzuIoT.Sensors.Atomopshere.Telemetry
{
    public class StartupTelemetry : SensorsBase
    {
        public StartupTelemetry(ILogger logger, IDevice device)
            :base(logger, device)
        {

        }
        
        override public async Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            await _sendMessageAsync(_device.GetDeviceInfo());
        }
    }
}
