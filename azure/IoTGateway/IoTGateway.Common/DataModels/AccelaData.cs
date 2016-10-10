using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTGateway.Common.DataModels
{
    public class AccelaData
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

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

        public bool IsEqualAndUpdate(out AccelaData org, AccelaData acc)
        {
            // 初期化
            org = new AccelaData(0.0, 0.0, 0.0);

            bool result = true;

            // 最初はnull
            if (null == org)
            {
                // Update
                org = acc;
                return false;
            }

            if (Math.Abs(org.X - acc.X) > 0.001) result = false;
            if (Math.Abs(org.Y - acc.Y) > 0.001) result = false;
            if (Math.Abs(org.Z - acc.Z) > 0.001) result = false;

            // Update
            org = acc;

            return result;
        }

        public string GetData()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
        }

        public static AccelaData GetObject(string jsonstring)
        {
            AccelaData me = JsonConvert.DeserializeObject<AccelaData>(jsonstring);
            return me;
        }
    }
}
