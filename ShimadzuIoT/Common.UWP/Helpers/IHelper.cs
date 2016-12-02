using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers
{
    public interface ITelemetryFactoryHelper
    {
        object CallTelemetryFactory(string deviceId);
    }

    public interface IDeviceFactoryHelper
    {
        object CallDeviceFactory(string deviceId);
    }
}
