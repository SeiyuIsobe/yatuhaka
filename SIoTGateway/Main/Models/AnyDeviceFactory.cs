using SIotGatewayCore.Devices.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using SIotGatewayCore.Devices;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Telemetry.Factory;
using SIotGatewayCore.Transport.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using SIoTGateway.Cooler.Devices;

namespace Main.Models
{
    [Obsolete("使用禁止", true)]
    public class AnyDeviceFactory : IDeviceFactory
    {
        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion


        public IDevice CreateDevice(
            ILogger logger,
            ITransportFactory transportFactory,
            ITelemetryFactory telemetryFactory,
            IConfigurationProvider configurationProvider,
            InitialDeviceConfig config)
        {
            var deviceKind = GetDeviceKindHelper.GetDeviceKind(config.DeviceId);

            IDevice device;

            // デバイス名からデバイスの種類を識別する
            switch (deviceKind)
            {
                case "Cooler":
                    device = new CoolerDevice(logger, transportFactory, telemetryFactory, configurationProvider);
                    device.Init(config);
                    return device;

                default:
                    device = new DeviceBase(null, transportFactory, telemetryFactory, configurationProvider);
                    device.Init(config);
                    return device;
            };
        }
    }
}
