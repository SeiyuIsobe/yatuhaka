using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class TelemetryFactoryResolver
    {
        private List<ITelemetryFactoryHelper> _list = new List<ITelemetryFactoryHelper>();

        public void Add(ITelemetryFactoryHelper sender)
        {
            _list.Add(sender);
        }

        public object Resolve(string deviceId)
        {
            foreach(ITelemetryFactoryHelper h in _list)
            {
                var obj = h.CallTelemetryFactory(deviceId);
                if (null != obj) return obj;
            }

            return null;
        }
    }
}
