using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;
using ShimadzuIoT.Sensors.Common.CommandParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimadzuIoT.Sensors
{
    public class OperationValueBase : IOperationValue
    {
        // 加速度値をクラウドに送るかどうかを判断するフラグ
        public IsAvailableCommandParameter IsAvailableCommandParameter { get; set; }

        public OperationValueBase()
        {
            this.IsAvailableCommandParameter = new IsAvailableCommandParameter();
        }
    }
}
