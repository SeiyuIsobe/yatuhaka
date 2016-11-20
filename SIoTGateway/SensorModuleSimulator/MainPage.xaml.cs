using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Interfaces;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
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

            this.Connected += async (sender, e) =>
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    this.MessageFromCloud = "接続しました";
                });

                Init();
            };

            this.ErrorConnect += async (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("-> ブローカーに接続できませんでした");

                await RetryConnect();
            };

            this.Disconnected += async (sender, e) =>
            {
                _sensor = null;
                _accelSensor = null;

                System.Diagnostics.Debug.WriteLine("-> ブローカーとの接続が切れました");

                await RetryConnect();
            };

        }

        private async Task RetryConnect()
        {
            System.Diagnostics.Debug.WriteLine("-> 3秒後に再トライします");

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                this.MessageFromCloud = "3秒後に再トライします : " + DateTime.Now.ToString();
            });

            _periodicTimer = new Timer((s) =>
            {
                _periodicTimer.Dispose();
                _periodicTimer = null;

                System.Diagnostics.Debug.WriteLine("-> 接続を再トライします");

                Connect();

            }, null, 3000, Timeout.Infinite);
        }

        private Timer _periodicTimer;
        private bool _retryIgnore = true;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //Task.Run(() => Task.Delay(5000)).Wait();

            // 初回に一回
            // ここで失敗すれば入力を待つ
            Connect();
        }

        private void Init()
        {
            // センサー基盤名をGWに送る
            SendSensorModuleName();

            // デバイス名をGWに送る
            //SendDeviceNames();

            InitSensor();

        }

        private void InitSensor()
        {

            #region 加速度センサー
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
            #endregion

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
        }

        private void SendSensorModuleName()
        {
            SensorList list = new SensorList();
            list.Sensors.Add("GW6210833_SM0771254175_SN19710613_DKCooler_958");
            list.Sensors.Add("GW6210833_SM0771254175_SN19760824_DKCooler_958");

            var sensor = new SensorModule() { Name = "SM0771254175" };
            sensor.Sensors = list;

            Publish("IamSensorModule", sensor.ToString());

            // 10秒ぐらい待たなければ相手が用意出来てない？？？
            Task.Delay(10000).Wait();
        }

        private void SendDeviceNames()
        {
            SensorList list = new SensorList();
            list.Sensors.Add("_SM19710613_SNm54321_DKCooler_");

            Publish("SendDeviceNames/SM19710613", list.ToString());

            Task.Delay(1000).Wait();
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
        private string _iotEndpoint = "192.168.11.10";
        //private string _iotEndpoint = "172.31.62.176";
        private string _clientID = "123456789";
        private string _topic = string.Empty;

        public async void Connect()
        {
            try
            {
                _client = new MqttClient(_iotEndpoint);
                _client.Connect(_clientID);
                if (true == _client.IsConnected)
                {
                    _client.ConnectionClosed += (sender, e) =>
                    {
                        _client = null;

                        if (null != Disconnected) Disconnected(null, null);
                    };

                    if (null != Connected) Connected(null, null);
                    
                }
            }
            catch (Exception e)
            {
                _client = null;

                // 最初の一回の接続失敗はリトライしない
                if(true == _retryIgnore)
                {
                    // リトライしない
                    // 最初の一回だけのフラグを下す
                    _retryIgnore = false;

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.MessageFromCloud = $"接続失敗：{_iotEndpoint}";
                    });
                }
                else
                {
                    if (null != ErrorConnect) ErrorConnect(null, null);
                }
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
        public event EventHandler Disconnected;

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

        private void _ipclear_Click(object sender, RoutedEventArgs e)
        {
            _ip.Text = string.Empty;
        }

        private async void _ipaccept_Click(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                _iotEndpoint = _ip.Text;

                Connect();
            });
        }

        private void _sougouDC_ip_Click(object sender, RoutedEventArgs e)
        {
            _ip.Text = _sougouDC_ip.Content.ToString();
        }

        //private void _aterm_ip_Click(object sender, RoutedEventArgs e)
        //{
        //    _ip.Text = _aterm_ip.Content.ToString();
        //}

        private void _b0_Click(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            var number = b.Content.ToString();

            _ip.Text = (_ip.Text + number);
        }
    }
}
