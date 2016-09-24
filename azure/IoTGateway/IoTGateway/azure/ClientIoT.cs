using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using IoTGateway.Common.DataModels;
using IoTGateway.Common;
using IoTGateway.Common.Interfaces;

namespace IoTGateway.azure
{
    public class ClientIoT
    {
        //IoTHubに接続するためのコネクション文字列
        private const string _deviceCn = "HostName=AccelaIoT.azure-devices.net;DeviceId=device6915d9cd54ff4dc5a076fbc24d068ab3;SharedAccessKey=Bd+Sbri6Na+NzZKkTm0iVG6vGKsGly9sbUw3ds5UMKo=";

        private DeviceClient _client = null;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ClientIoT()
        {
        }

        private SensorContainer _sensorContainer = null;

        public ClientIoT(SensorContainer sensorContainer)
        {
            _sensorContainer = sensorContainer;
        }

        public void SendSensorData()
        {
            foreach(var sensor in _sensorContainer.GetSensor())
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
                    if(null != se)
                    {
                        switch(se.Status)
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
                                if(null != ae)
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

        private async void Publish(string message)
        {
            //作成したメッセージをバイトデータに変換して、Messageオブジェクトに代入
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(message));

            //非同期でメッセージを送信
            await _client.SendEventAsync(eventMessage);
        }

        public void Connect()
        {
            try
            {
                //デバイスクライアントのインスタンス（プロトコルをMQTTに指定）
                DeviceClient client = DeviceClient.CreateFromConnectionString(_deviceCn, TransportType.Amqp);

                //IoTHubからメッセージを受信
                ReceiveCommands(client);

                // 保持
                _client = client;

            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    System.Diagnostics.Debug.WriteLine("Error in sample: {0}", exception);
                }
                return;
            }
            catch (Exception ex)
            {
                //Console.WriteLine();
                System.Diagnostics.Debug.WriteLine("Error in sample: {0}", ex.Message);
                return;
            }

            if(null != Connected)
            {
                Connected(this, null);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async void ReceiveCommands(DeviceClient client)
        {
            Message recievedMessage;

            string messageData;

            //メッセージを受信するためのループ
            while (true)
            {
                //一秒ごとにメッセージの受信を確認
                recievedMessage = await client.ReceiveAsync(TimeSpan.FromSeconds(1));

                if (recievedMessage != null)
                {
                    //届いたメッセージを文字列に変換
                    messageData = Encoding.ASCII.GetString(recievedMessage.GetBytes());

                    //コンソール出力
                    //System.Diagnostics.Debug.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);

                    // 連絡
                    if(null != ReceivedMessage)
                    {
                        ReceivedMessage(this, new ReceivedMessageArgs { Message = messageData });
                    }

                    //受信完了を待つ
                    await client.CompleteAsync(recievedMessage);
                }
            }
        }

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler ReceivedMessage;
    }
}
