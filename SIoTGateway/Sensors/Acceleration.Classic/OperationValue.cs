using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShimadzuIoT.Sensors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;

namespace ShimadzuIoT.Sensors.Acceleration
{
    public class OperationValue : OperationValueBase
    {
        // 加速度値をクラウドに送る時間間隔　単位はミリ秒
        // 0：取得して即送る
        public int ElapsedTime { get; set; } = 1000;
    }
}
