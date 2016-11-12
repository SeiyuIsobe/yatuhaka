using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common
{
    public class SensorModuleWatcher
    {
        #region MQTT
        private MqttClient _mqtt = null;
        #endregion

        public event EventHandler ReceivedDeviceNames;

        public SensorModuleWatcher()
        {
            // MQTT
            _mqtt = MqttHelper.Connect("127.0.0.1", Guid.NewGuid().ToString(), "SendDeviceNames");
            _mqtt.Subscribe(new string[] { "SendDeviceNames" }, new byte[] { 0 });
            _mqtt.MqttMsgPublishReceived += (sender, e) =>
            {
                var msg = Encoding.UTF8.GetString(e.Message);
                var topic = e.Topic;

                SensorList data = JsonConvert.DeserializeObject<SensorList>(msg);

                if(null != ReceivedDeviceNames)
                {
                    ReceivedDeviceNames(data, null);
                }
            };
        }
    }
}
