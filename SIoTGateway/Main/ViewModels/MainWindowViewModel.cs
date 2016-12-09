using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Main.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        #region プロパティ
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public Windows.UI.Core.CoreDispatcher Dispatcher { get; set; }

        public string SelfIP
        {
            get
            {
                return _myIp;
            }

            set
            {
                if (value == string.Empty) return;
                _myIp = value;
                NotifyPropertyChanged();
            }
        }
        #endregion

        private Models.SIoTGateway _iotgateway = null;
        
        internal void Run()
        {
            _iotgateway = new Models.SIoTGateway();
            _iotgateway.ReceivedTelemetry += async (sender, e) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.RecivedTelemetryData = sender.ToString();
                });                
            };

            // ゲートウェイサービス開始
            //_iotgateway.Start();
        }

        /// <summary>
        /// センサー基板が通電状態になったことを受けた処理
        /// </summary>
        /// <param name="v"></param>
        internal void ActivatedSensor(string sensorModuleID)
        {
            _iotgateway.ActivatedSensor(sensorModuleID);
        }

        internal void SendGatewayTime(DateTime gatewayTime)
        {
            _iotgateway.SendGatewayTime(gatewayTime);
        }

        public void GetMyIp(string ip)
        {
            this.SelfIP = ip;
        }

        private string _myIp = "127.0.0.1";

        private string _recivedTelemetryData = string.Empty;
        public string RecivedTelemetryData
        {
            get
            {
                return _recivedTelemetryData;
            }

            set
            {
                //System.Diagnostics.Debug.WriteLine($"-> {value}");
                _recivedTelemetryData = value;
                NotifyPropertyChanged();
            }
        }
    }
}
