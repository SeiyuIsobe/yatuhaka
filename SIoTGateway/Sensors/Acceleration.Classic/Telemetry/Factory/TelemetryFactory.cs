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
            // 今回の実験用では最初に登録した情報のままでいく
            //
            //// 最初の一発だけ送信するテレメトリー
            //var startupTelemetry = new StartupTelemetry(_logger, device);
            //device.TelemetryEvents.Add(startupTelemetry);

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
