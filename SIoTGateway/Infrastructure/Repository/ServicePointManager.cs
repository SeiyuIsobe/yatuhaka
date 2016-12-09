using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    internal class ServicePointManager
    {
        public static System.Func<object, object, object, object, bool> ServerCertificateValidationCallback { get; internal set; }
    }
}
