using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public static class MqttHelper
    {
        public static void Connect()
        {
            MqttClient client = null;
            string iotEndpoint = "192.168.11.12";
            string clientID = "123456789";
            string topic = string.Empty;

            try
            {
                client = new MqttClient(iotEndpoint);
                client.Connect(clientID);
                if (true == client.IsConnected)
                {
                    //NotifyConnected(this, null);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }
             
    }
}
