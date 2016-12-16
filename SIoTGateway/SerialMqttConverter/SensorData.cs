using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SerialMqttConverter
{
    class SensorData
    {
        public string timeStamp { get; set; }
        public double[] data { get; set; }
    }
}
