using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using ShimadzuIoTGatewayCore.Devices;
using ShimadzuIoTGatewayCore.Devices.Factory;
using ShimadzuIoTGatewayCore.Logging;
using ShimadzuIoTGatewayCore.Telemetry.Factory;
using ShimadzuIoTGatewayCore.Transport.Factory;
using System;
using System.Text.RegularExpressions;

namespace ShimadzuIoTGatewayCore.Cooler.Devices.Factory
{
    public class CoolerDeviceFactory : IDeviceFactory, IDeviceFactoryHelper
    {
        private readonly string _kindRegx = @"_DK\w*_";
        private readonly string _kind = "Cooler";

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

        //#region 受信イベント
        //public event EventHandler ReceivedTelemetry;
        //#endregion


        public IDevice CreateDevice(ILogger logger, ITransportFactory transportFactory,
            ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config)
        {
            var device = new CoolerDevice(logger, transportFactory, telemetryFactory, configurationProvider);
            device.Init(config);
            return device;
        }
    }
}
