using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTCloud
{
    public interface ISettingCloud
    {

    }

    public class AzureSetting : ISettingCloud
    {
        /// <summary>
        /// 接続文字列
        /// </summary>
        public string ConnectingString { get; set; }
    }

    public class BluemixSettng : ISettingCloud
    {
        /// <summary>
        /// Watson IoT PlatformのID
        /// このIDは自分で作れない、サービスを作成すると自動的に作られる
        /// </summary>
        public string OrganaizeID { get; set; }

        /// <summary>
        /// デバイスに紐つけられているパスワード
        /// デバイスタイプを作成するときに設定したもの
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// デバイスタイプ
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// デバイスID
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// トピック
        /// </summary>
        public string TargetTopic { get; set; }
    }

    public class M2XSetting : ISettingCloud
    {
        /// <summary>
        /// エンドポイント
        /// </summary>
        public string M2Xaddress { get; set; } = "https://api-m2x.att.com/v2/devices";

        /// <summary>
        /// デバイスID
        /// </summary>
        public string DeviceID { get; set; }

        /// <summary>
        /// 位置情報元
        /// </summary>
        public string LocationName { get; set; }

        /// <summary>
        /// デバイスIDと紐づくAPIキー
        /// </summary>
        public string APIKey { get; set; }

        /// <summary>
        /// ストリーム名
        /// </summary>
        public string StreamName { get; set; }
    }

    public class AwsSetting : ISettingCloud
    {
        /// <summary>
        /// AWSの間を仲介するMQTTブローカーのアドレス
        /// </summary>
        public string IoTEndpoint { get; set; }

        /// <summary>
        /// クライアントID
        /// </summary>
        public string CliendID { get; set; }

        /// <summary>
        /// トピック
        /// </summary>
        public string TargetTopic { get; set; }
    }
}
