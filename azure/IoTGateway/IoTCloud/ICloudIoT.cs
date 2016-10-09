using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTCloud
{
    interface ICloudIoT
    {
        string GetCloudName();
        void SendSensorData();
        void InitSensor();
        void Connect();
        void Publish(string message);
    }
}
