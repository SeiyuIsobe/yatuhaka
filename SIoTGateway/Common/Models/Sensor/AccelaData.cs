using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor
{
    public class AccelaData
    {
        #region ここに定義されるプロパティがmessageとなる
        public string DeviceId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        #endregion

        public AccelaData(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public bool IsEqual(AccelaData acc)
        {
            if (null == acc) return false;
            if (Math.Abs(this.X - acc.X) > 0.001) return false;
            if (Math.Abs(this.Y - acc.Y) > 0.001) return false;
            if (Math.Abs(this.Z - acc.Z) > 0.001) return false;

            return true;
        }

        public string GetData()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        }
    }
}
