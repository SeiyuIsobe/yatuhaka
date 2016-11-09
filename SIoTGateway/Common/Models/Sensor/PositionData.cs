using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor
{
    public class PositionData
    {
        /// <summary>
        /// 緯度
        /// </summary>
        public double Latitude
        {
            get
            {
                return _latitude;
            }

            set
            {
                _latitude = value;
            }
        }

        /// <summary>
        /// 経度
        /// </summary>
        public double Longitude
        {
            get
            {
                return _longitude;
            }

            set
            {
                _longitude = value;
            }
        }

        private double _latitude = 0.0;
        private double _longitude = 0.0;

        public string GetData()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        }

        public PositionData(double latitude, double longitude)
        {
            _latitude = latitude;
            _longitude = longitude;
        }
    }
}
