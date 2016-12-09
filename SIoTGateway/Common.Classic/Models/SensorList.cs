using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class SensorList
    {
        public List<string> Sensors { get; set; } = new List<string>();

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static SensorList ToObject(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SensorList>(json);
        }
    }
}
