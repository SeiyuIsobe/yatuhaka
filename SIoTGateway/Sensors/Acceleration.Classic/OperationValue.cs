using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShimadzuIoT.Sensors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;
using ShimadzuIoT.Sensors.Acceleration.CommandParameters;
using ShimadzuIoT.Sensors.Common.CommandParameters;

namespace ShimadzuIoT.Sensors.Acceleration
{
    /// <summary>
    /// 加速度センサー制御用パラメータ
    /// </summary>
    public class OperationValue : OperationValueBase
    {
        public OperationValue()
        {
            this.ElapsedTimeCommandParameter = new ElapsedTimeCommandParameter();
        }

        // 加速度値をクラウドに送る時間間隔　単位はミリ秒
        public ElapsedTimeCommandParameter ElapsedTimeCommandParameter { get; set; }
    }
}
