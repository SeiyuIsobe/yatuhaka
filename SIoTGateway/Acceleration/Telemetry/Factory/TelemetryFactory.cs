using SIotGatewayCore.Telemetry.Factory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SIotGatewayCore.Devices;
using SIotGatewayCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using System.Text.RegularExpressions;

namespace ShimadzuIoT.Sensors.Acceleration.Telemetry.Factory
{
    /// <summary>
    /// 加速度センサー
    /// </summary>
    public class TelemetryFactory : ITelemetryFactory, ITelemetryFactoryHelper
    {
        private readonly ILogger _logger;
        private readonly string _kindRegx = @"_DK\w*_";
        private readonly string _kind = "Accel";

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        public TelemetryFactory(ILogger logger)
        {
            _logger = logger;
        }

        public ITelemetryFactory CreateTelemetry(string deviceId)
        {
            throw new NotImplementedException();
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

        public object CallTelemetryFactory(string deviceId)
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
    }
}
