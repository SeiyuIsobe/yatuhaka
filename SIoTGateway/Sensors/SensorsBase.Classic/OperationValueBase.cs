using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimadzuIoT.Sensors
{
    public class OperationValueBase : IOperationValue
    {
        // 受信した加速度値をクラウドに送るかどうかを判定するフラグ
        // true：送る、false：送らない
        public bool IsAvailable { get; set; } = true;
    }
}
