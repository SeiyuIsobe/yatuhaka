using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
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
using System.Text.RegularExpressions;

namespace ShimadzuIoT.Sensors.Acceleration.Devices.Factory
{
    public class DeviceFactory : IDeviceFactory, IDeviceFactoryHelper
    {
        // 注意：TelemetryFactoryも同じようにすること
        private readonly string _kindRegx = @"_ACCE_";
        private readonly string _kind = "ACCE";

        public object CallDeviceFactory(string deviceIdOrg)
        {
            string deviceId = string.Empty;

            Regex rgx = new Regex(_kindRegx);

            // 末尾チェック
            // 末尾に"_"が付いているか
            if ("_" == deviceIdOrg.Substring(deviceIdOrg.Length - 1))
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

        public IDevice CreateDevice(ILogger logger, ITransportFactory transportFactory, ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider, InitialDeviceConfig config)
        {
            var device = new Device(logger, transportFactory, telemetryFactory, configurationProvider);

            if(null == logger && null == transportFactory && null == telemetryFactory && null == configurationProvider && null == config)
            {
                // 引数が全てNULLはデバイスをクラウドに新規登録する際に使う特別な使い方
                // このデバイスの初期値を取得するためだけに使う
            }
            else
            {
                device.Init(config);
            }
            
            return device;
        }
    }
}
