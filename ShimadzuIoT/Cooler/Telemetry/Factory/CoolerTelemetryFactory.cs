using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using ShimadzuIoTGatewayCore.Devices;
using ShimadzuIoTGatewayCore.Logging;
using ShimadzuIoTGatewayCore.Telemetry.Factory;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShimadzuIoTGatewayCore.Cooler.Telemetry.Factory
{
    public class CoolerTelemetryFactory : ITelemetryFactory, ITelemetryFactoryHelper
    {
        private readonly ILogger _logger;
        private readonly string _kindRegx = @"_DK\w*_";
        private readonly string _kind = "Cooler";

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        public CoolerTelemetryFactory(ILogger logger)
        {
            _logger = logger;
        }

        public object PopulateDeviceWithTelemetryEvents(IDevice device)
        {
            //var startupTelemetry = new StartupTelemetry(_logger, device);
            //device.TelemetryEvents.Add(startupTelemetry);

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

        public ITelemetryFactory CreateTelemetry(string deviceId)
        {
            throw new NotImplementedException();
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

        public object PopulateDeviceWithTelemetryEvents(IDevice device, Func<object, Task> sendMessageAsync)
        {
            throw new NotImplementedException();
        }
    }
}
