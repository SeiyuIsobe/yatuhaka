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
        private AccelOnBoard _accelOnBoard = null;
        private GpsOnBoard _gpsOnBoard = null;
        #endregion

        //
        // センサーコンテナー
        //
        private SensorContainer _sensorContainer = null;

        private ClientIoT _client = null;

        public MainWindowViewModel()
        {
            // センサーコンテナーの生成
            _sensorContainer = new SensorContainer();
        }

        public void Init()
        {
            #region 加速度センサー

            #region ラズパイ直結の加速度センサー、I2Cで通信する
            _accelSensor = new AccelOverI2C();
            _accelSensor.Interval = 1000;
            _accelSensor.ValueChanged += async (s2, e2) =>
            {
                var e3 = e2 as AccelEventArgs;
                if (null != e3)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.XAxis = e3.X;
                        this.YAxis = e3.Y;
                        this.ZAxis = e3.Z;
                    });
                }
            };
            // センサーをコンテナーに入れる
            _sensorContainer.Add(_accelSensor);
            #endregion

            #region スマフォなど加速度センサー内蔵タイプ
            _accelOnBoard = new AccelOnBoard();
            _accelOnBoard.ValueChanged += async (s2, e2) =>
            {
                var e3 = e2 as AccelEventArgs;
                if (null != e3)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.XAxis = e3.X;
                        this.YAxis = e3.Y;
                        this.ZAxis = e3.Z;
                    });
                }
            };
            // センサーをコンテナーに入れる
            _sensorContainer.Add(_accelOnBoard);
            #endregion

            #region GPS
            _gpsOnBoard = new GpsOnBoard();
            _gpsOnBoard.ValueChanged += async (s2, e2) =>
            {
                var e3 = e2 as GeolocationEventArgs;
                if(null != e3)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.Latitude = e3.Latitude;
                        this.Longitude = e3.Longitude;
                    });
                }
            };
            // センサーをコンテナーに入れる
            _sensorContainer.Add(_gpsOnBoard);
            #endregion
            #endregion

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
                NotifyPropertyChanged();
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
                NotifyPropertyChanged();
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
