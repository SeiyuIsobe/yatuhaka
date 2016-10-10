using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTCloud
{
    public interface ICloudIoT
    {
        string GetCloudName();
        void SendSensorData();
        void InitSensor();
        void Connect();
        void Publish(object sensor, string message);

        event EventHandler Connected;
        event EventHandler Disconnected;
        event EventHandler ReceivedMessage;
    }
}
