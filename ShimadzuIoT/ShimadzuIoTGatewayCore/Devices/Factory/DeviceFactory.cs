using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using ShimadzuIoTGatewayCore.Logging;
using ShimadzuIoTGatewayCore.Telemetry.Factory;
using ShimadzuIoTGatewayCore.Transport.Factory;

namespace ShimadzuIoTGatewayCore.Devices.Factory
{
    public class DeviceFactory : IDeviceFactory
    {
        public IDevice CreateDevice(ILogger logger, ITransportFactory transportFactory, 
            ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config)
        {
            var device = new DeviceBase(logger, transportFactory, telemetryFactory, configurationProvider);
            device.Init(config);
            return device;
        }
    }
}
