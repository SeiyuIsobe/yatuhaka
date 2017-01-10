using SIotGatewayCore.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using SIotGatewayCore.Logging;
using ShimadzuIoT.Sensors;
using uPLibrary.Networking.M2Mqtt.Messages;
using ShimadzuIoT.Sensors.Atomopshere.Telemetry.Data;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;
using Newtonsoft.Json;
using SIotGatewayCore.Devices;
using ShimadzuIoT.Sensors.Telemetry.Data;

namespace ShimadzuIoT.Sensors.Atomopshere.Telemetry
{
    /// <summary>
    /// 加速度センサー
    /// </summary>
    public class RemoteMonitorTelemetry : SensorsBase
    {
        private OperationValue _operationValue = null;

        public RemoteMonitorTelemetry(ILogger logger, IDevice device)
            :base(logger, device)
        {
            _mqtt.MqttMsgPublishReceived += OnMqttMsgPublishReceived;

            // 加速度センサー制御用パラメータ
            _operationValue = (OperationValue)(base.OperationValue);

        }

        private RemoteMonitorTelemetryData _monitorData = null;

        override async protected void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var msg = Encoding.UTF8.GetString(e.Message);
            var topic = e.Topic;

            _monitorData = JsonConvert.DeserializeObject<RemoteMonitorTelemetryData>(msg);

            //_monitorData = new RemoteMonitorTelemetryData();
            //_monitorData.DeviceId = _deviceId;
            //_monitorData.X = data.X;
            //_monitorData.Y = data.Y;
            //_monitorData.Z = data.Z;

            // 即送信
            if(0 == base.ElapsedTime)
            {
                if (null != _sendMessageAsync && true == base.TelemetryActive)
                {
                    await _sendMessageAsync(_monitorData);
                }
            }
            

            NotifyReceivedTelemetry(msg);
        }

        override async protected void OnTimer(object sender)
        {
            // 定期的送信
            if(0 < base.ElapsedTime)
            {
                if(null != _monitorData)
                {
                    if (null != _sendMessageAsync && true == base.TelemetryActive)
                    {
                        await _sendMessageAsync(_monitorData);
                    }
                }
            }
        }
    }
}
