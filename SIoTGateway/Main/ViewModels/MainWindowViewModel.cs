using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Models;

namespace Main.ViewModels
{
    public class MainWindowViewModel
    {
        #region プロパティ
        public Windows.UI.Core.CoreDispatcher Dispatcher { get; set; }


        #endregion

        private Models.SIoTGateway _iotgateway = null;
        
        internal void Run()
        {
            _iotgateway = new Models.SIoTGateway();

            _iotgateway.Start();
        }

        /// <summary>
        /// センサー基板が通電状態になったことを受けた処理
        /// </summary>
        /// <param name="v"></param>
        internal void ActivatedSensor(string sensorModuleID)
        {
            _iotgateway.ActivatedSensor(sensorModuleID);
        }
    }
}
