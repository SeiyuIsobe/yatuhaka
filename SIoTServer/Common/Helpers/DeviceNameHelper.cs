using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class DeviceNameHelper
    {
        private static string[] _prefixes = new string[]
        {
            @"_GW[\da-zA-Z]+_",
            @"_SM[\da-zA-Z]+_",
            @"_SN[\da-zA-Z]+_"
        };

        private static string CheckTail(string deviceIdOrg)
        {
            string deviceId = string.Empty;

            // 先頭チェック
            if("_" == deviceIdOrg.Substring(0, 1))
            {
                deviceId = deviceIdOrg;
            }
            else
            {
                deviceId = "_" + deviceIdOrg;
            }

            // 末尾チェック
            // 末尾に"_"が付いているか
            if ("_" == deviceId.Substring(deviceId.Length - 1))
            {
                // no action
            }
            else
            {
                deviceId = deviceId + "_";
            }
            return deviceId;
        }

        private static string GetKeyword(int index, string deviceId)
        {
            Regex rgx = new Regex(_prefixes[index]);

            MatchCollection matches = rgx.Matches(deviceId);
            if (matches.Count > 0)
            {
                // 前後の区切り文字_を消す
                // 先頭の区切り文字を消す
                return matches[0].Value.Replace("_", "").Substring(2);
            }

            return null;
        }

        public static string GetGatewayName(string deviceId)
        {
            return DeviceNameHelper.GetKeyword(0, DeviceNameHelper.CheckTail(deviceId));
        }

        internal static string GetSensorModuleName(string deviceId)
        {
            return DeviceNameHelper.GetKeyword(1, DeviceNameHelper.CheckTail(deviceId));
        }

        internal static string GetSensorName(string deviceId)
        {
            return DeviceNameHelper.GetKeyword(2, DeviceNameHelper.CheckTail(deviceId));
        }

        /// <summary>
        /// 順番に、ゲートウェイ、センサーモジュール、センサー名を消していって
        /// 残ったのがセンサーの種類
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        internal static string GetKindSensor(string deviceId)
        {
            string _gw_sm_sn_sk_ = DeviceNameHelper.CheckTail(deviceId);
            string _sm_sn_sk_ = _gw_sm_sn_sk_.Replace("_GW" + DeviceNameHelper.GetGatewayName(_gw_sm_sn_sk_) + "_", "");
            string _sn_sk_ = DeviceNameHelper.CheckTail(_sm_sn_sk_).Replace("_SM" + DeviceNameHelper.GetSensorModuleName(_gw_sm_sn_sk_) + "_", "");
            string _sk_ = DeviceNameHelper.CheckTail(_sn_sk_).Replace("_SN" + DeviceNameHelper.GetSensorName(_gw_sm_sn_sk_) + "_", "");
            
            return DeviceNameHelper.CheckTail(_sk_).Replace("_", "");
        }
    }
}
