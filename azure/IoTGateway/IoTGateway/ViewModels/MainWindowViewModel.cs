using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTGateway.Common.DataModels;
using IoTGateway.azure;
using SiRSensors;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IoTGateway.Common;

namespace IoTGateway.ViewModels
{
    public class MainWindowViewModel: INotifyPropertyChanged
    {
        #region 使うセンサー
        private AccelOverI2C _accelSensor = null;
        #endregion

        //
        // センサーコンテナー
        //
        private SensorContainer _sensorContainer = null;

        private ClientIoT _client = null;

        public MainWindowViewModel()
        {
        }

        public void Init()
        {
            #region 加速度センサー
            _accelSensor = new AccelOverI2C();
            _accelSensor.ValueChanged += async (sender, e) =>
            {
                var ee = e as AccelEventArgs;
                if (null != ee)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.XAxis = ee.X;
                        this.YAxis = ee.Y;
                        this.ZAxis = ee.Z;
                    });
                }
            };
            _accelSensor.Interval = 1000;
            _accelSensor.Init();
            #endregion

            // センサーをコンテナーに入れる
            _sensorContainer = new SensorContainer();
            _sensorContainer.Add(_accelSensor);

            #region Azure
            _client = new ClientIoT(_sensorContainer);
            _client.Connected += (sender, e) =>
            {
                _client.InitSensor();
            };
            _client.ReceivedMessage += async (sender, e) =>
            {
                // 連絡
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.MessageFromCloud = ((ReceivedMessageArgs)e).Message;
                });
            };
            _client.Connect();
            #endregion
        }

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

        /// <summary>
        /// X軸
        /// </summary>
        private double _xAxis = 0.0;
        public double XAxis
        {
            get
            {
                return _xAxis;
            }

            set
            {
                _xAxis = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Y軸
        /// </summary>
        private double _yAxis = 0.0;
        public double YAxis
        {
            get
            {
                return _yAxis;
            }

            set
            {
                _yAxis = value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Z軸
        /// </summary>
        private double _zAxis = 0.0;
        public double ZAxis
        {
            get
            {
                return _zAxis;
            }

            set
            {
                _zAxis = value;
                NotifyPropertyChanged();
            }
        }

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

        public Windows.UI.Core.CoreDispatcher Dispatcher { get; set; }

        private string _messageFromCloud = string.Empty;
        public string MessageFromCloud
        {
            get
            {
                return _messageFromCloud;
            }

            set
            {
                _messageFromCloud = value;
                NotifyPropertyChanged();
            }
        }

        
        #endregion

        #region イベント
        
        #endregion
    }
}
