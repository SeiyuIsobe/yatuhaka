using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Interfaces
{
    public interface ISensor
    {
        void Init();
        string Data { get; }
        event EventHandler StatusChanged;
        event EventHandler ValueChanged;
    }
}
