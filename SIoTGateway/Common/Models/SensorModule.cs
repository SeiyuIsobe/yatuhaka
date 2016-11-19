using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class SensorModule
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }

        public static SensorModule ToObject(string json)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<SensorModule>(json);
        }
    }
}
