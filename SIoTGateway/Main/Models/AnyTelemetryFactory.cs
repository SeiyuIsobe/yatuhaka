using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SIotGatewayCore.Telemetry.Factory;
using SIotGatewayCore.Devices;
using SIotGatewayCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using SIoTGateway.Cooler.Telemetry.Factory;

namespace Main.Models
{
    [Obsolete("使用禁止", true)]
    public class AnyTelemetryFactory : ITelemetryFactory
    {
        private readonly ILogger _logger;

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        public AnyTelemetryFactory(ILogger logger)
        {
            _logger = logger;
        }

        public object PopulateDeviceWithTelemetryEvents(IDevice device)
        {
            throw new NotImplementedException();
        }

        public ITelemetryFactory CreateTelemetry(string deviceId)
        {
            var deviceKind = GetDeviceKindHelper.GetDeviceKind(deviceId);

            ITelemetryFactory ifactory = null; 

            // デバイス名からデバイスの種類を識別する
            switch (deviceKind)
            {
                case "Cooler":
                    ifactory = new CoolerTelemetryFactory(_logger);
                    return ifactory;

                case "Accel":
                    ifactory = new ShimadzuIoT.Sensors.Acceleration.Telemetry.Factory.TelemetryFactory(_logger);
                    return ifactory;

                default:
                    return null;
            };
        }

        public object PopulateDeviceWithTelemetryEvents(IDevice device, Func<object, Task> sendMessageAsync)
        {
            throw new NotImplementedException();
        }
    }
}
