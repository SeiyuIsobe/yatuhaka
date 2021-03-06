﻿using SIotGatewayCore.Telemetry;
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
        protected OperationValueBase _operationValue = null;

        private Timer _timer = null;

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
        public SensorsBase(ILogger logger, IDevice device)
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
                _deviceId = string.Empty;
            }

            // センサー制御用パラメータ（共通）
            _operationValue = (OperationValueBase)device.OperationValue;
            if(null != _operationValue)
            {
                // センサーデータを送る・送らないフラグ
                TelemetryActive = _operationValue.IsAvailableCommandParameter.IsAvailable;

                // 送信の時間間隔
                ElapsedTime = _operationValue.ElapsedTimeCommandParameter.ElapsedTime;

            }

            // MQTTサブスクライバの生成
            _mqtt = MqttHelper.Connect("127.0.0.1", Guid.NewGuid().ToString(), null);
            _mqtt.Subscribe(new string[] { _deviceId }, new byte[] { 0 });
            //_mqtt.MqttMsgPublishReceived += OnMqttMsgPublishReceived;

            // タイマーの生成
            _timer = new Timer(OnTimer, null, ElapsedTime, ElapsedTime);

        }

        virtual public Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            // これ必要
            _sendMessageAsync = sendMessageAsync;

            //このwhile必要か分からん
            //// 戻り値がTaskなので必要らしいが・・・
            while (true)
            {
            }
        }

        virtual protected void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            // ここに実装する場合は共通の処理であること
        }

        virtual protected void OnTimer(object state)
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

        public void SetSendMessageAsyncFunction(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            _sendMessageAsync = sendMessageAsync;
        }

        public bool TelemetryActive { get; set; } = true;
        public int ElapsedTime { get; set; } = 1000;

        virtual public object OperationValue
        {
            get
            {
                return _operationValue;
            }
        }

        public async Task SendDeviceModelAsync(object devicemodel)
        {
            await _sendMessageAsync(devicemodel);
        }


    }
}
