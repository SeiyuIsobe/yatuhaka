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
        private static MqttClient _client = null;

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
                _client = null;
            }

            return null;
        }

        public static MqttClient Connect(string clientID)
        {
            try
            {
                if (null == _client)
                {
                    _client = new MqttClient("127.0.0.1");
                    _client.Connect(clientID);
                }
                else
                {
                    // no action
                }

                if (true == _client.IsConnected)
                {
                    return _client;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                _client = null;
            }

            return null;

        }

        public static Task SendGatewayTimeAsync(DateTime time)
        {
            try
            {
                if (null == _client)
                {
                    _client = new MqttClient("127.0.0.1");
                    _client.Connect(new Guid().ToString());
                }
                else
                {
                    // no action
                }

                if (true == _client.IsConnected)
                {
                    var enc = Encoding.UTF8.GetBytes($@"{{st:""{time.ToString()}""}}");
                    _client.Publish("GatewayTime", enc);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                _client = null;
            }

            return Task.FromResult(0);
        }

    }
}
