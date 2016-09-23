using IoTGateway.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTGateway.Common.DataModels
{
    public class SensorContainer
    {
        private List<ISensor> _list = new List<ISensor>();

        public void Add(ISensor isensor)
        {
            _list.Add(isensor);
        }

        public List<ISensor> List
        {
            get
            {
                return _list;
            }
        }

        public IEnumerable<ISensor> GetSensor()
        {
            foreach(var s in _list)
            {
                yield return s;
            }
        }
    }
}
