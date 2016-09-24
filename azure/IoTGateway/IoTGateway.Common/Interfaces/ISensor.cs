using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTGateway.Common.Interfaces
{
    public interface ISensor
    {
        void Init();
        string Data { get; }
        event EventHandler StatusChanged;
        event EventHandler ValueChanged;
    }
}
