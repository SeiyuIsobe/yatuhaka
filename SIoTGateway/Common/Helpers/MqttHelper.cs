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
        public static MqttClient Connect(string iotEndpoint, string clientID, string topic)
        {
            MqttClient client = null;

            try
            {
                client = new MqttClient(iotEndpoint);
                client.Connect(clientID);
                if (true == client.IsConnected)
                {
                    return client;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }
             
    }
}
