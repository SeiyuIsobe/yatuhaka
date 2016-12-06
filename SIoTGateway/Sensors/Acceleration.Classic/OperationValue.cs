using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShimadzuIoT.Sensors;

namespace ShimadzuIoT.Sensors.Acceleration
{
    public class OperationValue : IOperationValue
    {
        // 加速度値をクラウドに送る時間間隔　単位はミリ秒
        // 0：取得して即送る
        public int ElapsedTime { get; set; } = 1000;
    }
}
