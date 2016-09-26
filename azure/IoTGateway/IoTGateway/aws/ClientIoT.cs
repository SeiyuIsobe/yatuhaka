using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using IoTGateway.Common.DataModels;
using IoTGateway.Common;
using IoTGateway.Common.Interfaces;
using uPLibrary.Networking.M2Mqtt;

namespace IoTGateway.aws
{
    public class ClientIoT
    {
        private SensorContainer _sensorContainer = null;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler ReceivedMessage;

        private const string IotEndpoint = "ah6dzf0sp913w.iot.ap-northeast-1.amazonaws.com";
        private const int BrokerPort = 8883;

        public ClientIoT(SensorContainer sensorContainer)
        {
            _sensorContainer = sensorContainer;
        }

        public void InitSensor()
        {

        }

        public void Connect()
        {
            try
            {
                var client = new MqttClient(IotEndpoint);//, BrokerPort, true, MqttSslProtocols.TLSv1_2);
                client.Connect("AccelaIoT");
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            
        }
    }
}
