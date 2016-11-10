using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using Windows.ApplicationModel.Background;

namespace SIoTBroker
{
    public sealed class SIoTBroker : IBackgroundTask
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

        BackgroundTaskDeferral _deferral = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Start();

            //
            // Get the deferral object from the task instance, and take a reference to the taskInstance;
            //
            _deferral = taskInstance.GetDeferral();

            taskInstance.Progress = 555;
        }
    }
}
