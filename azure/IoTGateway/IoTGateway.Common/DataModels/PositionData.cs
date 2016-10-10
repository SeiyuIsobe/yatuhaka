using Newtonsoft.Json;
using System;

namespace IoTGateway.Common.DataModels
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

        public static PositionData GetObject(string jsonstring)
        {
            //System.Diagnostics.Debug.WriteLine($"::> {jsonstring}");
            PositionData me = JsonConvert.DeserializeObject<PositionData>(jsonstring);

            // 関係ないJSONでもデシリアライズしてしまうので
            // 関係ないものはnullで返すようにする
            if (me.Longitude == 0.0 && me.Latitude == 0.0) return null;

            return me;
        }
    }
}
