using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace IoTGateway.azure
{
    public class ClientIoT
    {
        //IoTHubに接続するためのコネクション文字列
        private const string _deviceCn = "HostName=AccelaIoT.azure-devices.net;DeviceId=device6915d9cd54ff4dc5a076fbc24d068ab3;SharedAccessKey=Bd+Sbri6Na+NzZKkTm0iVG6vGKsGly9sbUw3ds5UMKo=";

        //メッセージ送信数
        private static int MESSAGE_COUNT = 1;


        #region M2MQTTを使う
        //クライアント
        private static MqttClient _client;

        //IoTHub名（URL)
        private const string _ioTHubName = "AccelaIoT.azure-devices.net";

        //デバイスID
        private const string _deviceId = "device6915d9cd54ff4dc5a076fbc24d068ab3";

        //MQTTブローカーアドレス
        private string _userName = string.Format("{0}/{1}", _ioTHubName, _deviceId);

        //アクセストークン
        private const string _sasToken = "SharedAccessSignature sr=AccelaIoT.azure-devices.net&sig=6%2FFAWvmTrtDahurpBlGBuZ0UVvv14S8sQTyBaYnGiJA%3D&se=1505829064&skn=iothubowner";

        private static string _topicDevice2Service;
        private static string _topicService2Device;

        private const int MQTT_PORT = 8883;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ClientIoT()
        {
            //MQTT Topic
            _topicService2Device = string.Format("devices/{0}/messages/devicebound/#", _deviceId);

            //メッセージの送信先
            _topicDevice2Service = string.Format("devices/{0}/messages/events", _deviceId);

        }

        public void Connect()
        {
            try
            {
                _client = new MqttClient(_ioTHubName, MQTT_PORT, true, MqttSslProtocols.TLSv1_0);

                //接続
                _client.Connect(_deviceId, _userName, _sasToken);
            }
            catch (Exception ex)
            {
                //Debug.WriteLine("Error in sample: {0}", ex.Message);
                _client = null;
            }

            if(false == _client.IsConnected)
            {
                System.Diagnostics.Debug.WriteLine("-> connect fail.");
                return;
            }

            _client.ConnectionClosed += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("-> Disconnect!!!");

                _client = new MqttClient(_ioTHubName, MQTT_PORT, true, MqttSslProtocols.TLSv1_0);
                _client.Connect(_deviceId, _userName, _sasToken);
            };

            //受信イベントの登録
            _client.MqttMsgPublishReceived += (sender, e) =>
            {
                //受信イベントからメッセージの取り出し
                string msg = Encoding.UTF8.GetString(e.Message);

                System.Diagnostics.Debug.WriteLine("-> Receive: " + msg);
            };

            
            //受信するトピックの登録
            _client.Subscribe(new[] { _topicService2Device }, new[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });

        }

        public void Publish(string message)
        {
            //メッセージの送信（Publish）
            _client.Publish(_topicDevice2Service, Encoding.UTF8.GetBytes(message));
        }

        public async void Botu_Connect()
        {
            try
            {
                //デバイスクライアントのインスタンス（プロトコルをMQTTに指定）
                DeviceClient client = DeviceClient.CreateFromConnectionString(_deviceCn, TransportType.Mqtt);

                //セッションを開く
                await client.OpenAsync();

            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    //Console.WriteLine();
                    //Console.WriteLine("Error in sample: {0}", exception);
                }

                return;
            }
            catch (Exception ex)
            {
                if(null != Disconnected)
                {
                    Disconnected(this, new common.ClientExecptionEventArgs());
                }

                return;
            }

            if (null != Connected)
            {
                Connected(this, new common.ClientExecptionEventArgs());
            }
        }

        public async void Do()
        {

            try
            {
                //デバイスクライアントのインスタンス（プロトコルをMQTTに指定）
                DeviceClient client = DeviceClient.CreateFromConnectionString(_deviceCn, TransportType.Mqtt);

                //セッションを開く
                await client.OpenAsync();

                //IoTHubへメッセージの送信
                //SendEvent(client).Wait();

                //IoTHubからメッセージを受信
                ReceiveCommands(client).Wait();

            }
            catch (AggregateException ex)
            {
                foreach (Exception exception in ex.InnerExceptions)
                {
                    //Console.WriteLine();
                    //Console.WriteLine("Error in sample: {0}", exception);
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine();
                //Console.WriteLine("Error in sample: {0}", ex.Message);
            }
            //Console.WriteLine("Press enter to exit...");
            //Console.ReadLine();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        static async Task SendEvent(DeviceClient client)
        {
            //送信メッセージ用
            string dataBuffer;

            //コンソール出力
            //Console.WriteLine("Device sending {0} messages to IoTHub...\n", MESSAGE_COUNT);

            for (int count = 0; count < MESSAGE_COUNT; count++)
            {
                //メッセージ作成
                dataBuffer = "メッセージ:" + count.ToString();

                //作成したメッセージをバイトデータに変換して、Messageオブジェクトに代入
                Message eventMessage = new Message(Encoding.UTF8.GetBytes(dataBuffer));

                //コンソール出力
                //Console.WriteLine("\t{0} Sending message: {1}, Data: [{2}]", DateTime.Now.ToLocalTime(), count, dataBuffer);

                //非同期でメッセージを送信
                await client.SendEventAsync(eventMessage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        static async Task ReceiveCommands(DeviceClient client)
        {
            //コンソールに出力
            //Console.WriteLine("\nDevice waiting for commands from IoTHub...\n");

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
                    //Console.WriteLine("\t{0}> Received message: {1}", DateTime.Now.ToLocalTime(), messageData);

                    //受信完了を待つ
                    await client.CompleteAsync(recievedMessage);
                }
            }
        }

        public event EventHandler Connected;
        public event EventHandler Disconnected;
    }
}
