using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public class DeviceFactoryResolver
    {
        private List<IDeviceFactoryHelper> _list = new List<IDeviceFactoryHelper>();

        public void Add(IDeviceFactoryHelper sender)
        {
            _list.Add(sender);
        }

        public object Resolve(string deviceId)
        {
            foreach (IDeviceFactoryHelper h in _list)
            {
                var obj = h.CallDeviceFactory(deviceId);
                if (null != obj) return obj;
            }

            return null;
        }
    }
}
