using SIotGatewayCore.Devices;
using System;
using System.Threading.Tasks;

namespace SIotGatewayCore.Telemetry.Factory
{
    public interface ITelemetryFactory
    {
        /// <summary>
        /// Populates a device with telemetry events or logic
        /// </summary>
        /// <param name="device">Device interface to populate</param>
        /// <returns>
        /// Returns object as a way to handle returning the instance that is generating telemetry data
        /// so that it can be used by the caller of this method
        /// </returns>
        object PopulateDeviceWithTelemetryEvents(IDevice device);
        ITelemetryFactory CreateTelemetry(string deviceId);
        object PopulateDeviceWithTelemetryEvents(IDevice device, Func<object, Task> sendMessageAsync);
    }
}
