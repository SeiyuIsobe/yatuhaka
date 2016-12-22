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

namespace ShimadzuIoT.Sensors.Atomopshere.Telemetry.Factory
{
    /// <summary>
    /// 加速度センサー
    /// </summary>
    public class TelemetryFactory : ITelemetryFactory, ITelemetryFactoryHelper
    {
        private readonly ILogger _logger;

        // 注意：DeviceFactoryも同じようにすること
        private readonly string _kindRegx = @"_ATOM_";
        private readonly string _kind = "ATOM";

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

        /// <summary>
        /// センサーにテレメトリーを送信するオブジェクトを登録
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public object PopulateDeviceWithTelemetryEvents(IDevice device)
        {
            return PopulateDeviceWithTelemetryEvents(device, null);
        }

        public object PopulateDeviceWithTelemetryEvents(IDevice device, Func<object, Task> sendMessageAsync)
        {
            // センサーデータを送信するテレメトリー
            var monitorTelemetry = new RemoteMonitorTelemetry(_logger, device);
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

        public object CallTelemetryFactory(string deviceIdOrg)
        {
            string deviceId = string.Empty;

            Regex rgx = new Regex(_kindRegx);

            // 末尾チェック
            // 末尾に"_"が付いているか
            if("_" == deviceIdOrg.Substring(deviceIdOrg.Length - 1))
            {
                deviceId = deviceIdOrg;
            }
            else
            {
                deviceId = deviceIdOrg + "_";
            }

            MatchCollection matches = rgx.Matches(deviceId);
            if (matches.Count > 0)
            {
                // 前後の区切り文字_を消す
                string kind = matches[0].Value.Replace("_", "");

                // 自分を指しているのか判定する
                if (_kind == kind) return this;
            }

            return null;
        }
    }
}
