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

        private const string IotEndpoint = "127.0.0.1";
        private string _clientID = "123456789";
        private string _topic = "Sakisaki";

        override public string GetCloudName()
        {
            return "AWS";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ClientIoT(SensorContainer sensorContainer)
            : base(sensorContainer)
        {
        }

        override public void Connect()
        {
            try
            {
                //// 30秒後にブローカーに接続
                //System.Diagnostics.Debug.WriteLine("-> 30秒後にブローカーに接続");
                //await Task.Delay(1000 * 30);

                _client = new MqttClient(IotEndpoint);
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

        override public void Publish(string message)
        {
            _client.Publish(_topic, Encoding.UTF8.GetBytes(message));
        }
    }
}
