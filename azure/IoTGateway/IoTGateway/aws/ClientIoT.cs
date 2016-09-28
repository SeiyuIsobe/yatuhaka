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

        public async void Connect()
        {
            // https://www.nuget.org/packages/Chilkat.uwp/
            Chilkat.Socket socket = new Chilkat.Socket();

            bool success;
            success = socket.UnlockComponent("Anything for 30-day trial");
            if (success != true)
            {
                System.Diagnostics.Debug.WriteLine(socket.LastErrorText);
                return;
            }

            //  Create an instance of a certificate store object, load a PFX file,
            //  locate the certificate we need, and use it for signing.
            //  (a PFX file may contain more than one certificate.)
            Chilkat.CertStore certStore = new Chilkat.CertStore();
            //  The 1st argument is the filename, the 2nd arg is the
            //  PFX file's password:
            success = certStore.LoadPfxFile("395106240d.pfx", "395106240");
            if (success != true)
            {
                System.Diagnostics.Debug.WriteLine(certStore.LastErrorText);
                return;
            }

            //CN=AWS IoT Certificate

            Chilkat.Cert cert = null;
            cert = certStore.FindCertBySubjectCN("AWS IoT Certificate");
            if (cert == null)
            {
                System.Diagnostics.Debug.WriteLine(certStore.LastErrorText);
                return;
            }

            success = socket.SetSslClientCert(cert);

            bool ssl = true;
            int maxWaitMillisec = 20000;

            //  The SSL server hostname may be an IP address, a domain name,
            //  or "localhost".  You'll need to change this:
            string sslServerHost;
            sslServerHost = IotEndpoint;
            int sslServerPort = BrokerPort;

            //  Connect to the SSL server:
            success = await socket.ConnectAsync(sslServerHost, sslServerPort, ssl, maxWaitMillisec);
            if (success != true)
            {
                System.Diagnostics.Debug.WriteLine(socket.LastErrorText);
                return;
            }

            //  Set maximum timeouts for reading an writing (in millisec)
            socket.MaxReadIdleMs = 20000;
            socket.MaxSendIdleMs = 20000;

            //  Send a "Hello Server! -EOM-" message:
            success = await socket.SendStringAsync("Hello Server! -EOM-");
            if (success != true)
            {
                System.Diagnostics.Debug.WriteLine(socket.LastErrorText);
                return;
            }
        }

        //public void Connect()
        //{
        //    try
        //    {
        //        var client = new MqttClient(IotEndpoint, BrokerPort, true, MqttSslProtocols.TLSv1_2);
        //        client.Connect("AccelaIoT");
        //    }
        //    catch(Exception e)
        //    {
        //        System.Diagnostics.Debug.WriteLine(e.Message);
        //    }

        //}
    }
}
