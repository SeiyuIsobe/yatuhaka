using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using IoTGateway.Common.DataModels;
using IoTGateway.Common;
using IoTGateway.Common.Interfaces;
using Microsoft.Azure.Devices.Client;
using System.Diagnostics;

namespace IoTCloud.azure
{
    public class ClientIoT : BaseCloudIoT, ICloudIoT
    {
        //IoTHubに接続するためのコネクション文字列
        //private const string _deviceCn = "HostName=AccelaIoT.azure-devices.net;DeviceId=device6915d9cd54ff4dc5a076fbc24d068ab3;SharedAccessKey=Bd+Sbri6Na+NzZKkTm0iVG6vGKsGly9sbUw3ds5UMKo=";


        //        ID=devicea67d66dfaf86452f8d0480034316de75
        //PrimaryKey = 26Z1hRXJ7pswjKZc9jAU/AKY/D8H9b7JsFOOgvg/BLM=
        //SecondaryKey=bh2xwPYRtnAnxyS9OpUEWKNoFb8qPgGjD/SV2gLs8Pg=
        //private const string _deviceCn = "HostName=AccelaIoT.azure-devices.net;DeviceId=devicea67d66dfaf86452f8d0480034316de75;SharedAccessKey=26Z1hRXJ7pswjKZc9jAU/AKY/D8H9b7JsFOOgvg/BLM=";
        private string _deviceCn = null;

        private DeviceClient _client = null;
        private AzureSetting _azuresetting = null;

        override public string GetCloudName()
        {
            return "Azure";
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
            :base(sensorContainer)
        {
            _azuresetting = (AzureSetting)setting;
            _deviceCn = _azuresetting.ConnectingString;
        }

        override public async void Publish(object sensor, string message)
        {
            //作成したメッセージをバイトデータに変換して、Messageオブジェクトに代入
            Message eventMessage = new Message(Encoding.UTF8.GetBytes(message));

            //非同期でメッセージを送信
            await _client.SendEventAsync(eventMessage);
        }

        override public void Connect()
        {
            try
            {
                //デバイスクライアントのインスタンスを生成
                DeviceClient client = DeviceClient.CreateFromConnectionString(_deviceCn, TransportType.Amqp);

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

            NotifyConnected(this, null);

            //IoTHubからメッセージを受信
            //ReceiveCommands(_client);

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
                    NotifyReceiveMessage(this, new ReceivedMessageArgs { Message = messageData });

                    //受信完了を待つ
                    await client.CompleteAsync(recievedMessage);
                }
            }
        }
    }
}
