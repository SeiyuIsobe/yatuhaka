using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using SIotGatewayCore.Telemetry.Factory;
using SIotGatewayCore.Transport.Factory;

namespace SIotGatewayCore.Devices.Factory
{
    public interface IDeviceFactory
    {
        IDevice CreateDevice(Logging.ILogger logger, ITransportFactory transportFactory,
            ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config);
    }
}
