using SIotGatewayCore.Devices;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Telemetry.Factory;
using System;

namespace SIoTGateway.Cooler.Telemetry.Factory
{
    public class CoolerTelemetryFactory : ITelemetryFactory
    {
        private readonly ILogger _logger;

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        public CoolerTelemetryFactory(ILogger logger)
        {
            _logger = logger;
        }

        public object PopulateDeviceWithTelemetryEvents(IDevice device)
        {
            var startupTelemetry = new StartupTelemetry(_logger, device);
            device.TelemetryEvents.Add(startupTelemetry);

            var monitorTelemetry = new RemoteMonitorTelemetry(_logger, device.DeviceID);
            monitorTelemetry.ReceivedTelemetry += (sender, e) =>
            {
                if (null != ReceivedTelemetry)
                {
                    ReceivedTelemetry(sender, null);
                }
            };
            device.TelemetryEvents.Add(monitorTelemetry);

            return monitorTelemetry;
        }
    }
}
