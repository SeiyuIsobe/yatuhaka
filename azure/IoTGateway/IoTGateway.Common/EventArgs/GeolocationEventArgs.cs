using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTGateway.Common
{
    public class GeolocationEventArgs : SensorEventArgs
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
