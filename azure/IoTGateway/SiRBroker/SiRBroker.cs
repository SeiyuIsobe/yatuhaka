using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;

namespace SiRBroker
{
    [Obsolete("使用禁止", true)]
    public sealed class SiRBroker
    {
        private MqttBroker _broker = null;
        public Windows.UI.Core.CoreDispatcher Dispatcher { get; set; }

        public async void Start()
        {
            if (null == _broker)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    _broker = new MqttBroker();
                    _broker.Start();
                });
            }
            else
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    _broker.Stop();
                    _broker = null;
                });
            }
        }
    }
}
