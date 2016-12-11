using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;
using ShimadzuIoT.Sensors.Common.CommandParameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimadzuIoT.Sensors
{
    /// <summary>
    /// 全てのセンサーが共通に持つ制御用のパラメータ
    /// </summary>
    public class OperationValueBase : IOperationValue
    {
        // センサー値をクラウドに送るかどうかを判断するフラグ
        public IsAvailableCommandParameter IsAvailableCommandParameter { get; set; } = new IsAvailableCommandParameter();

        // センサー値をクラウドに送る際の時間間隔
        public ElapsedTimeCommandParameter ElapsedTimeCommandParameter { get; set; } = new ElapsedTimeCommandParameter();

        public OperationValueBase()
        {
        }
    }
}
