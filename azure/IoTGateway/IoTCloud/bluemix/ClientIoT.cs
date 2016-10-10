using IBMWatsonIoTP;
using IoTGateway.Common.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTCloud.bluemix
{
    public class ClientIoT : BaseCloudIoT, ICloudIoT
    {
        /// <summary>
        /// Watson IoT PlatformのID
        /// このIDは自分で作れない、サービスを作成すると自動的に作られる
        /// </summary>
        private string _orgId = "ymn9fh";

        /// <summary>
        /// デバイスに紐つけられているパスワード
        /// デバイスタイプを作成するときに設定したもの
        /// </summary>
        private string _authToken = "AppSukekiyo";

        /// <summary>
        /// デバイスタイプ
        /// </summary>
        private string _deviceType = "SukekiyoApp";

        /// <summary>
        /// デバイスID
        /// </summary>
        private string _deviceId = "64006a567f07";
        
        private DeviceClient _client = null;

        override public string GetCloudName()
        {
            return "Bluemix";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ClientIoT()
        { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ClientIoT(SensorContainer sensorContainer)
            : base(sensorContainer)
        {
        }

        override public void Publish(object sensor, string message)
        {
            try
            {
                if (sensor is SiRSensors.AccelOnBoard || sensor is SiRSensors.AccelOverI2C)
                {
                    _client.publishEvent("test", "json", message, 0);
                }
            }
            catch { }
        }

        override public void Connect()
        {
            if (null == _client)
            {
                _client = new DeviceClient(_orgId, _deviceType, _deviceId, "token", _authToken);

                //_client.ConnectionClosed += (sender, e) =>
                //{
                //    if (null != ConnectionClosed)
                //    {
                //        ConnectionClosed(sender, e);
                //    }
                //};
            }

            // 接続
            _client.connect();

            if(true == _client.isConnected())
            {
                NotifyConnected(this, null);
            }
        }
    }
}
