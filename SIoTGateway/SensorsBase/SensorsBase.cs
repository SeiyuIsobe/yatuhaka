using SIotGatewayCore.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SIotGatewayCore.Logging;
using uPLibrary.Networking.M2Mqtt;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using uPLibrary.Networking.M2Mqtt.Messages;
using SIotGatewayCore.Devices;

namespace ShimadzuIoT.Sensors
{
    public class SensorsBase : ITelemetry
    {
        protected readonly ILogger _logger;
        protected readonly string _deviceId;
        protected readonly IDevice _device;
        protected Func<object, Task> _sendMessageAsync = null;

        #region MQTT
        protected MqttClient _mqtt = null;
        #endregion

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="deviceId"></param>
        public SensorsBase(ILogger logger, string deviceId, IDevice device)
        {
            _logger = logger;

            if(null != device)
            {
                _device = device;
                _deviceId = device.DeviceID;
            }
            else
            {
                _device = null; // nullのまま
                _deviceId = deviceId;
            }

            // MQTTサブスクライバの生成
            _mqtt = MqttHelper.Connect("127.0.0.1", Guid.NewGuid().ToString(), null);
            _mqtt.Subscribe(new string[] { _deviceId }, new byte[] { 0 });
            //_mqtt.MqttMsgPublishReceived += OnMqttMsgPublishReceived;

        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="deviceId"></param>
        public SensorsBase(ILogger logger, string deviceId)
            :this(logger, deviceId, null)
        {
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="deviceId"></param>
        public SensorsBase(ILogger logger, IDevice device)
            :this(logger, null, device)
        {
        }

        public Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            // これ必要
            _sendMessageAsync = sendMessageAsync;

            // このwhile必要か分からん
            // 戻り値がTaskなので必要らしいが・・・
            while (true)
            {
            }
        }

        virtual public void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // ここに実装する場合は共通の処理であること
        }

        protected void NotifyReceivedTelemetry(string msg)
        {
            if(null != ReceivedTelemetry)
            {
                ReceivedTelemetry(msg, null);
            }
        }
    }
}
