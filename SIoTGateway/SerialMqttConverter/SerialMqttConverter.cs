using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Background;
using NETMF.OpenSource.XBee;
using NETMF.OpenSource.XBee.Api;
using NETMF.OpenSource.XBee.Api.Zigbee;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Newtonsoft.Json;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using System.Diagnostics;
using ShimadzuIoT.Sensors.Telemetry.Data;
using NETMF.OpenSource.XBee.Util;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace SerialMqttConverter
{
    public sealed class XBeeMqttConverter : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;

        // メンバー変数
        private XBeeApi _xbee = null;
        private MqttClient _client = null;
        private List<DeviceInformation> _deviceList = new List<DeviceInformation>();
        private List<DeviceInfo> _deviceInfoList = new List<DeviceInfo>();   // デバイス情報一覧

        // 定数
        private const string BROKER_IP_ADDRESS = "127.0.0.1";   // ブローカーのIPアドレス 
        private const string GATEWAY_ID = "GW1";                    // ゲートウェイ基板ID
        private const string COMMAND_ID_CONDITIONSET = "10";        // データ取得条件設定のコマンドID
        private const string COMMAND_ID_TIMESYNC = "20";            // 現在時刻設定のコマンドID
        private const string COMMAND_ID_DEVICEIDINFO = "30";        // ID通知のコマンドID
        private const string COMMAND_ID_SENSORDATA = "40";          // センサーデータ通知のコマンドID
        private const string MQTT_TOPIC_DEVICEIDINFO = "IamSensorModule";   // センサーデータ通知のコマンドID
        private const string MQTT_TOPIC_TIMESYNC = "timeSync";      // センサーデータ通知のコマンドID
        private const string MQTT_TOPIC_CONDITIONSET = "conditionSet";      // センサーデータ通知のコマンドID
        private const string COM_PORT = "COM18";                    // ポート番号
        private const int BAUD_RATE = 9600;                         // ボーレート
        private const int DATA_SIZE_BYTE = 5;                       // データ長のバイト数

        public event EventHandler Connected;

        /// <summary>
        /// XBeeとMQTTブローカーへの接続を開始
        /// </summary>
        public void Start()
        {
            if (null == _xbee || null == _client)
            {
                // MQTTブローカーへ接続
                connectBroker();

                // XBeeへ接続
                connectXBee();
            }
        }

        /// <summary>
        /// XBeeとMQTTブローカーから切断
        /// </summary>
        public void Stop()
        {
            // XBeeとの接続を切断
            _xbee.Close();
            _xbee = null;

            // MQTTブローカーから切断
            _client.Disconnect();
            _client = null;
        }

        /// <summary>
        /// 本コンバータの起動
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Start();

            //
            // Get the deferral object from the task instance, and take a reference to the taskInstance;
            //
            _deferral = taskInstance.GetDeferral();

            taskInstance.Progress = 500;
        }

        /// <summary>
        /// XBeeモジュールへ接続
        /// </summary>
        private async void connectXBee()
        {

            string serialSelector = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(serialSelector);
            if (devices != null && devices.Count > 0)
            {
                var device = devices[0];

                Debug.WriteLine("deviceName:" + device.Name);

                var serport = await SerialDevice.FromIdAsync(device.Id);
                if (serport != null)
                {
                    serport.IsDataTerminalReadyEnabled = true;
                    serport.IsRequestToSendEnabled = true;
                    serport.DataBits = 8;
                    serport.StopBits = SerialStopBitCount.One;
                    serport.Parity = SerialParity.None;
                    serport.BaudRate = 9600;

                    try
                    {
                        _xbee = new XBeeApi(serport);
                        _xbee.Open();

                        // 受信イベントの登録
                        _xbee.DataReceived += XBeeDataReceived;

                        if (true == _xbee.IsConnected())
                        {
                            Debug.WriteLine("XBee connected.");

                            if (null != Connected)
                            {
                                Connected(this, null);
                            }
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("XBee Error -> Close");
                        _xbee.Close();
                    }
                    finally
                    {

                    }

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"-> _xbee is NULL!!!!!");
                }
            }
        }

        /// <summary>
        /// XBeeにデータ受信があったとき
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        private void XBeeDataReceived(XBeeApi receiver, byte[] data, XBeeAddress sender)
        {
            string recvData = new string(Encoding.UTF8.GetChars(data));
            string commandID = recvData.Substring(0, 2);
            string payload = recvData.Substring(2);

            Debug.WriteLine("Received: " + recvData);

            // コマンドIDに応じた処理
            switch (commandID)
            {
                case COMMAND_ID_DEVICEIDINFO:
                    catchDevideIdInfo(payload, sender);
                    break;
                case COMMAND_ID_SENSORDATA:
                    catchSensorData(payload);
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// MQTTブローカーとの通信を開始する
        /// </summary>
        private void connectBroker()
        {
            _client = new MqttClient(BROKER_IP_ADDRESS);

            try
            {
                var ret = _client.Connect(Guid.NewGuid().ToString());
            }
            catch
            {
                Debug.WriteLine("can not connect Broker.");
                return;
            }

            _client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            // 現在時刻設定を監視
            _client.Subscribe(new[] { MQTT_TOPIC_TIMESYNC }, new[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            // データ取得条件設定を監視
            _client.Subscribe(new[] { MQTT_TOPIC_CONDITIONSET }, new[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            if (_client.IsConnected)
            {
                Debug.WriteLine("connected Broker");
            }

        }

        /// <summary>
        /// MQTTでメッセージを受信したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string msg = Encoding.UTF8.GetString(e.Message);
            var topic = e.Topic;
            Debug.WriteLine(topic + ", " + msg);

            switch (topic)
            {
                case MQTT_TOPIC_TIMESYNC:
                    catchTimeSync(msg);
                    break;
                case MQTT_TOPIC_CONDITIONSET:
                    catchConditionSet(msg);
                    break;
                default:
                    break;
            }
        }


        /////////////////////////////////////////////////////////////////////////
        ///////// 各コマンド・ステータスでの処理        /////////////////////////
        /////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// センシング基板からID通知を受け取ったとき
        /// </summary>
        /// <param name="data"></param>
        /// <param name="sender"></param>
        private void catchDevideIdInfo(string data, XBeeAddress sender)
        {
            string dataSize = data.Substring(0, 5);
            string[] splitDataArray = data.Substring(5).Split(',');
            string sensingDeviceId = splitDataArray[0];
            List<string> sensorIdList = new List<string>();
            sensorIdList.AddRange(splitDataArray);
            sensorIdList.RemoveAt(0);

            // アドレスとデバイスIDの対応をリストに記録
            SensorList deviceIdList = new SensorList();
            int length = sensorIdList.Count;
            for (int i = 0; i < length; i++)
            {
                string deviceId = GATEWAY_ID + "_" + sensingDeviceId + "_" + sensorIdList[i];
                DeviceInfo deviceInfo = new DeviceInfo { deviceId = deviceId, address = sender };
                _deviceInfoList.Add(deviceInfo);
                deviceIdList.Sensors.Add(deviceId);
            }

            // 制御ソフトへの送信用にデータを加工
            var sensor = new SensorModule() { Name = sensingDeviceId };
            sensor.Sensors = deviceIdList;

            // 制御ソフトへデータ送信
            string msg = JsonConvert.SerializeObject(sensor);
            _client.Publish(MQTT_TOPIC_DEVICEIDINFO, Encoding.UTF8.GetBytes(msg));
        }

        /// <summary>
        /// センシング基板からセンサーデータを受け取ったとき
        /// </summary>
        /// <param name="data"></param>
        private void catchSensorData(string data)
        {
            string dataSize = data.Substring(0, 5);
            string dataCount = data.Substring(5, 1);
            string[] splitDataArray = data.Substring(6).Split(',');
            string sensingSensorId = splitDataArray[0];
            string dataString = splitDataArray[1];
            string[] sensorDataArray = splitDataArray[1].Split('-');
            string timeStamp = splitDataArray[2];

            var sensorData = prepareSendSensorData(sensingSensorId,timeStamp,sensorDataArray);

            // 制御ソフトへデータ送信
            string msg = JsonConvert.SerializeObject(sensorData);
            _client.Publish(GATEWAY_ID + "_" + sensingSensorId, Encoding.UTF8.GetBytes(msg), 0, true);
        }

        private RemoteMonitorTelemetryDataBase prepareSendSensorData(string id, string timeStamp, string[] dataArray)
        {
            var sensorData = new RemoteMonitorTelemetryDataBase();
            sensorData.Timestamp = DateTime.ParseExact(timeStamp, "yyyyMMddHHmmss", null);
            string sensorType = id.Split('_')[2];

            switch (sensorType)
            {
                case "ACCE":
                    sensorData.X = Double.Parse(dataArray[0]);
                    sensorData.Y = Double.Parse(dataArray[1]);
                    sensorData.Z = Double.Parse(dataArray[2]);
                    break;
                case "ATOM":
                    sensorData.Atomos = Double.Parse(dataArray[0]);
                    break;
                case "TEMP":
                    sensorData.Temperature = Double.Parse(dataArray[0]);
                    break;
                case "MIKE":
                    break;
                default:
                    break;
            }

            return sensorData;

        }

        /// <summary>
        /// 制御ソフトから現在時刻設定を受け取ったとき
        /// </summary>
        /// <param name="msg"></param>
        private void catchTimeSync(string msg)
        {
            // センシング基板への送信用データへ加工
            string commandId = COMMAND_ID_TIMESYNC;
            string data = msg;
            string dataSize = String.Format("{0:D5}", data.Length);
            //string dataSize = "00014";
            byte[] payload = Encoding.UTF8.GetBytes(commandId + dataSize + data);

            // センシング基板へブロードキャストで送信
            NETMF.OpenSource.XBee.Api.XBeeAddress64 broadcastAddress
                = NETMF.OpenSource.XBee.Api.XBeeAddress64.Broadcast;
            TxRequest tx = new TxRequest(broadcastAddress, payload);
            _xbee.Send(tx).To(broadcastAddress).NoResponse();

            Debug.WriteLine("send current time to sensing device");

        }

        /// <summary>
        /// 制御ソフトからデータ取得条件設定を受け取ったとき
        /// </summary>
        /// <param name="msg"></param>
        private void catchConditionSet(string msg)
        {
            DeviceInfo info = JsonConvert.DeserializeObject<DeviceInfo>(msg);

            // センシング基板への送信用データへ加工
            string commandId = COMMAND_ID_CONDITIONSET;
            string data = info.deviceId + "-" + info.conditionType + "-" + info.settingValue;
            string dataSize = data.Length.ToString();
            byte[] payload = Encoding.UTF8.GetBytes(commandId + dataSize + data);

            // デバイス情報一覧のデータ取得条件を更新
            int length = _deviceInfoList.Count;
            XBeeAddress address = null;
            for (int i = 0; i < length; i++)
            {
                if (info.deviceId == _deviceInfoList[i].deviceId)
                {
                    _deviceInfoList[i].conditionType = info.conditionType;
                    _deviceInfoList[i].settingValue = info.settingValue;
                    address = _deviceInfoList[i].address;
                    break;
                }
            }

            // センシング基板へ送信
            TxRequest tx = new TxRequest(address, payload);
            _xbee.Send(tx).To(address).NoResponse();
            Debug.WriteLine("send current time to sensing device");

        }

    }


}
