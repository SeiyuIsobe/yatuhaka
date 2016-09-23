using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTGateway.Common
{
    public class ReceivedMessageArgs : System.EventArgs
    {
        public string Message { get; set; }
    }
}
