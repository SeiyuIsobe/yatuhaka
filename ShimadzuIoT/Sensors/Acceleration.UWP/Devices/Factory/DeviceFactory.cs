using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using ShimadzuIoTGatewayCore.Devices.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using ShimadzuIoTGatewayCore.Devices;
using ShimadzuIoTGatewayCore.Logging;
using ShimadzuIoTGatewayCore.Telemetry.Factory;
using ShimadzuIoTGatewayCore.Transport.Factory;
using System.Text.RegularExpressions;

namespace ShimadzuIoT.Sensors.Acceleration.Devices.Factory
{
    public class DeviceFactory : IDeviceFactory, IDeviceFactoryHelper
    {
        private readonly string _kindRegx = @"_DK\w*_";
        private readonly string _kind = "Accel";

        public object CallDeviceFactory(string deviceId)
        {
            Regex rgx = new Regex(_kindRegx);

            MatchCollection matches = rgx.Matches(deviceId);
            if (matches.Count > 0)
            {
                // 前後の区切り文字_を消して、DK以降の文字列を取得する
                string kind = matches[0].Value.Replace("_", "").Substring(2);

                // 自分を指しているのか判定する
                if (_kind == kind) return this;
            }

            return null;
        }

        public IDevice CreateDevice(ILogger logger, ITransportFactory transportFactory, ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config)
        {
            var device = new Device(logger, transportFactory, telemetryFactory, configurationProvider);
            device.Init(config);
            return device;
        }
    }
}
