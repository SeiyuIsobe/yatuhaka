using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Interfaces;
using SiRSensors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace SensorModuleSimulator
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private AccelOnBoard _sensor = null;
        private AccelOverI2C _accelSensor = null;

        public MainPage()
        {
            this.InitializeComponent();

            this.DataContext = this;

            this.Connected += (sender, e) =>
            {
                this.MessageFromCloud = "接続しました";

                _sensor = new AccelOnBoard();

                _sensor.ValueChanged += async (s2, e2) =>
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

                        if (null != _client)
                        {
                            Publish("hogehoge", ((ISensor)s2).Data);
                        }
                    }
                };
                _sensor.Init();

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

                        if (null != _client)
                        {
                            Publish("hogehoge", ((ISensor)s2).Data);
                        }
                    }
                };
                _accelSensor.Init();
                #endregion

            };

            this.ErrorConnect += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("-> ブローカーに接続できませんでした");
                System.Diagnostics.Debug.WriteLine("-> 10秒後に再トライします");

                this.MessageFromCloud = "10秒後に再トライします : " + DateTime.Now.ToString();

                _periodicTimer = new Timer((s) =>
                {
                    _periodicTimer.Dispose();
                    _periodicTimer = null;

                    System.Diagnostics.Debug.WriteLine("-> 接続を再トライします");

                    Connect();

                }, null, 10000, Timeout.Infinite);
            };
        }

        private Timer _periodicTimer;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Task.Run(() => Task.Delay(5000)).Wait();

            Connect();
            
        }

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

        #region プロパティ
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
        #endregion

        private MqttClient _client = null;
        private string _iotEndpoint = "172.31.62.176";
        private string _clientID = "123456789";
        private string _topic = string.Empty;

        public void Connect()
        {
            try
            {
                _client = new MqttClient(_iotEndpoint);
                _client.Connect(_clientID);
                if (true == _client.IsConnected)
                {
                    if (null != Connected) Connected(null, null);
                }
            }
            catch (Exception e)
            {
                _client = null;

                if (null != ErrorConnect) ErrorConnect(null, null);
            }

            //MqttHelper.Connect();

        }

        public void Publish(string topic, string message)
        {
            _client.Publish(topic, Encoding.UTF8.GetBytes(message));

            System.Diagnostics.Debug.WriteLine($"-> {message}");
        }

        public event EventHandler Connected;
        public event EventHandler ErrorConnect;

        private string _message = string.Empty;
        public string MessageFromCloud
        {
            get
            {
                return _message;
            }

            set
            {
                _message = value;
                NotifyPropertyChanged();
            }
        }
    }
}
