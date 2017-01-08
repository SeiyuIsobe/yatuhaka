using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace SIoTBroker
{
    public sealed class SIoTBroker
    {
        private MqttBroker _broker = null;

        public void Start()
        {
            if (null == _broker)
            {
                _broker = new MqttBroker();
                _broker.Start();
            }
            else
            {
                _broker.Stop();
                _broker = null;
            }
        }

        public void Stop()
        {
            if (null != _broker) Start();
        }
    }
}
