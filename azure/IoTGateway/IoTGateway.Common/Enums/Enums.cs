using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTGateway.Common
{
    public enum Status
    {
        Unknown,
        Wait,
        Running,
        Error
    }

    public enum CloudVendor
    {
        Unknown,
        AWS,
        Azure,
        Bluemix,
        M2X
    }
}
