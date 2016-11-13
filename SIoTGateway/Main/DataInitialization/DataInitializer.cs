using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Main.DataInitialization
{
    public class DataInitializer : IDataInitializer
    {
        private readonly IDeviceLogic _deviceLogic;

        public DataInitializer(IDeviceLogic deviceLogic)
        {
            if (deviceLogic == null)
            {
                throw new ArgumentNullException("deviceLogic");
            }

            _deviceLogic = deviceLogic;
        }

        public void BootstrapDevice(string id)
        {
            string ret_string = string.Empty;

            Task.Run(async () => ret_string = await _deviceLogic.BootstrapDevice(id)).Wait();
        }
    }
}
