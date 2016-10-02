using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using Windows.ApplicationModel.Background;

namespace SiRBroker
{
    public sealed class SiRBrokerTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral = null;
        private System.Threading.Timer _timer = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            System.Diagnostics.Debug.WriteLine("-> task run.");

            Start();

            //
            // Get the deferral object from the task instance, and take a reference to the taskInstance;
            //
            _deferral = taskInstance.GetDeferral();

            System.Diagnostics.Debug.WriteLine("-> task running.");

            taskInstance.Progress = 555;

        }

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
    }
}
