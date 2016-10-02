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
    public class ClientIoT
    {
        private MqttClient _client = null;
        private SensorContainer _sensorContainer = null;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler ReceivedMessage;

        private const string IotEndpoint = "127.0.0.1";
        private string _clientID = "123456789";
        private string _topic = "Sakisaki";

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ClientIoT()
        {
        }

        public string GetCloudName()
        {
            return "AWS";
        }

        public ClientIoT(SensorContainer sensorContainer)
        {
            _sensorContainer = sensorContainer;
        }

        public void SendSensorData()
        {
            foreach (var sensor in _sensorContainer.GetSensor())
            {
                Publish(sensor.Data);
            }
        }

        public void InitSensor()
        {
            // センサーの初期化
            foreach (var sensor in _sensorContainer.GetSensor())
            {
                // 準備
                sensor.StatusChanged += (sender, e) =>
                {
                    SensorEventArgs se = e as SensorEventArgs;
                    if (null != se)
                    {
                        switch (se.Status)
                        {
                            case Status.Running:

                                ((ISensor)sender).ValueChanged += (s2, e2) =>
                                {
                                    // クラウドに送信
                                    Publish(((ISensor)s2).Data);
                                };

                                break;

                            case Status.Error:

                                AccelEventArgs ae = e as AccelEventArgs;
                                if (null != ae)
                                {
                                    System.Diagnostics.Debug.WriteLine("-> " + ae.ExceptionMessage);
                                }

                                break;
                        };
                    }
                };

                // 初期化を実行
                sensor.Init();
            }

            foreach (var sensor in _sensorContainer.GetSensor())
            {
                sensor.ValueChanged += (sender, e) =>
                {
                    // クラウドに送信
                    Publish(sensor.Data);
                };
            }
        }

        public async void Connect()
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
                    if (null != Connected)
                    {
                        Connected(this, null);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

        }

        private void Publish(string message)
        {
            _client.Publish(_topic, Encoding.UTF8.GetBytes(message));
        }
    }
}
