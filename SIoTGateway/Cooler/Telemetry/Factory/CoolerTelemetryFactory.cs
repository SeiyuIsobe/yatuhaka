using SIotGatewayCore.Devices;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Telemetry.Factory;

namespace SIoTGateway.Cooler.Telemetry.Factory
{
    public class CoolerTelemetryFactory : ITelemetryFactory
    {
        private readonly ILogger _logger;


        public CoolerTelemetryFactory(ILogger logger)
        {
            _logger = logger;
        }

        public object PopulateDeviceWithTelemetryEvents(IDevice device)
        {
            var startupTelemetry = new StartupTelemetry(_logger, device);
            device.TelemetryEvents.Add(startupTelemetry);

            var monitorTelemetry = new RemoteMonitorTelemetry(_logger, device.DeviceID);
            device.TelemetryEvents.Add(monitorTelemetry);

            return monitorTelemetry;
        }
    }
}
