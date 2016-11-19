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

        public event EventHandler ReceivedSensorModuleName;
        public event EventHandler ReceivedDeviceNames;

        public SensorModuleWatcher()
        {
            _mqtt = MqttHelper.Connect("127.0.0.1", Guid.NewGuid().ToString(), null);
            _mqtt.Subscribe(new string[] { $"IamSensorModule" }, new byte[] { 0 });
            _mqtt.MqttMsgPublishReceived += (sender, e) =>
            {
                var msg = Encoding.UTF8.GetString(e.Message);
                var topic = e.Topic;

                SensorModule data = JsonConvert.DeserializeObject<SensorModule>(msg);

                if (null != ReceivedSensorModuleName)
                {
                    ReceivedSensorModuleName(data, null);
                }
            };
        }

        public SensorModuleWatcher(string moduleID)
        {
            // MQTT
            _mqtt = MqttHelper.Connect("127.0.0.1", Guid.NewGuid().ToString(), null);
            _mqtt.Subscribe(new string[] { $"SendDeviceNames/{moduleID}" }, new byte[] { 0 });
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
            System.Diagnostics.Debug.WriteLine($"-> SensorModuleWatcher");
        }
    }
}
