using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using IoTGateway.Common.DataModels;
using IoTGateway.Common;
using IoTGateway.Common.Interfaces;

namespace IoTCloud.aws
{
    public class ClientIoT : BaseCloudIoT, ICloudIoT
    {
        private MqttClient _client = null;

        private string _iotEndpoint = string.Empty;
        private string _clientID = string.Empty;
        private string _topic = string.Empty;

        private AwsSetting _awssetting = null;

        override public string GetCloudName()
        {
            return "AWS";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ClientIoT(SensorContainer sensorContainer, ISettingCloud setting)
            : base(sensorContainer)
        {
            _awssetting = (AwsSetting)setting;
            _iotEndpoint = _awssetting.IoTEndpoint;
            _clientID = _awssetting.CliendID;
            _topic = _awssetting.TargetTopic;
        }

        override public void Connect()
        {
            try
            {
                //// 30秒後にブローカーに接続
                //System.Diagnostics.Debug.WriteLine("-> 30秒後にブローカーに接続");
                //await Task.Delay(1000 * 30);

                _client = new MqttClient(_iotEndpoint);
                _client.Connect(_clientID);
                if (true == _client.IsConnected)
                {
                    NotifyConnected(this, null);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

        }

        override public void Publish(object sensor, string message)
        {
            _client.Publish(_topic, Encoding.UTF8.GetBytes(message));
        }
    }
}
