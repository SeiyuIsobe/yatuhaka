using System;
using System.Threading.Tasks;
using SIotGatewayCore.Devices;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Telemetry;

namespace SIoTGateway.Cooler.Telemetry
{
    public class StartupTelemetry : ITelemetry
    {
        private readonly ILogger _logger;
        private readonly IDevice _device;
        
        public StartupTelemetry(ILogger logger, IDevice device)
        {
            _logger = logger;
            _device = device;
        }

        public async Task SendEventsAsync(System.Threading.CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            if (!token.IsCancellationRequested)
            {
                _logger.LogInfo("Sending initial data for device {0}", _device.DeviceID);
                await sendMessageAsync(_device.GetDeviceInfo());
            }
        }
    }
}