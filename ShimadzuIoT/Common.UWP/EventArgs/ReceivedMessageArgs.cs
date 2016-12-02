using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common
{
    public class ReceivedMessageArgs : System.EventArgs
    {
        public string Message { get; set; }
    }
}
