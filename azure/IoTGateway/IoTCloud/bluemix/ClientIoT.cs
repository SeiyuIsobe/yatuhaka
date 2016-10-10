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
        private string _orgId = string.Empty;
        private string _authToken = string.Empty;
        private string _deviceType = string.Empty;
        private string _deviceId = string.Empty;
        private string _targettopic = string.Empty;

        private DeviceClient _client = null;
        private BluemixSettng _bluemixsetting = null;

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
        public ClientIoT(SensorContainer sensorContainer, ISettingCloud setting)
            : base(sensorContainer)
        {
            _bluemixsetting = (BluemixSettng)setting;
            _orgId = _bluemixsetting.OrganaizeID;
            _authToken = _bluemixsetting.AuthToken;
            _deviceId = _bluemixsetting.DeviceID;
            _deviceType = _bluemixsetting.DeviceType;
            _targettopic = _bluemixsetting.TargetTopic;
        }

        override public void Publish(object sensor, string message)
        {
            try
            {
                if (sensor is SiRSensors.AccelOnBoard || sensor is SiRSensors.AccelOverI2C)
                {
                    _client.publishEvent(_targettopic, "json", message, 0);
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
