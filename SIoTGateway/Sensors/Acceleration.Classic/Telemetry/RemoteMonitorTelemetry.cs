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
using ShimadzuIoT.Sensors.Acceleration.Telemetry.Data;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;
using Newtonsoft.Json;
using SIotGatewayCore.Devices;

namespace ShimadzuIoT.Sensors.Acceleration.Telemetry
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

            _operationValue = (OperationValue)(base.OperationValue);

            base.TelemetryActive = _operationValue.IsAvailableCommandParameter.IsAvailable; // 送る・送らないフラグ
        }

        override async public void OnMqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var msg = Encoding.UTF8.GetString(e.Message);
            var topic = e.Topic;

            AccelaData data = JsonConvert.DeserializeObject<AccelaData>(msg);

            var monitorData = new RemoteMonitorTelemetryData();
            monitorData.DeviceId = _deviceId;
            monitorData.X = data.X;
            monitorData.Y = data.Y;
            monitorData.Z = data.Z;

            if (null != _sendMessageAsync && true == base.TelemetryActive)
            {
                await _sendMessageAsync(monitorData);
            }

            NotifyReceivedTelemetry(msg);
        }
    }
}
