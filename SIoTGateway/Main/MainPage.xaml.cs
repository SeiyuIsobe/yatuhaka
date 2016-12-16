using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace Main
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ViewModels.MainWindowViewModel _mainwindowVM = null;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // TODO:public async Task StartAsync(CancellationToken token)を参考に
            // Taskをリストにして、Task.WhenAlで実体化する
            //
            // SerialMQTTConverter
            var deviceCancellationToken = new CancellationTokenSource();
            await StartAsync(deviceCancellationToken.Token);

        }

        /// <summary>
        /// Starts the send event loop and runs the receive loop in the background
        /// to listen for commands that are sent to the device
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken token)
        {
            try
            {
                var loopTasks = new List<Task>
                {
                    StartBrokerAsync(token),
                    StartSerialMqttConverterAsync(token)
                };

                // Wait both the send and receive loops
                await Task.WhenAll(loopTasks.ToArray());

                #region
                //await Task.Run(async () =>
                //{
                //    SIoTBroker.SIoTBroker broker = new SIoTBroker.SIoTBroker();
                //    broker.Start();

                //    if (null == _mainwindowVM)
                //    {
                //        _mainwindowVM = new Main.ViewModels.MainWindowViewModel();
                //        _mainwindowVM.Dispatcher = Dispatcher;

                //        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //        {
                //            this.DataContext = _mainwindowVM;
                //            _mainwindowVM.Run();

                //            GetMyIp();
                //        });
                //    }

                //    while (true) { }
                //});
                #endregion

            }
            catch (Exception ex)
            {
                
            }
        }

        private async Task StartBrokerAsync(CancellationToken token)
        {
            await Task.Run(async () =>
            {
                SIoTBroker.SIoTBroker broker = new SIoTBroker.SIoTBroker();
                broker.Start();

                if (null == _mainwindowVM)
                {
                    _mainwindowVM = new Main.ViewModels.MainWindowViewModel();
                    _mainwindowVM.Dispatcher = Dispatcher;

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        this.DataContext = _mainwindowVM;
                        _mainwindowVM.Run();

                        GetMyIp();
                    });

                    //// NICTから日本標準時を取得
                    //GetNICTTime().Wait();

                    //// 時刻をセンシング基盤に通知
                    //_mainwindowVM.SendGatewayTime(_gatewayTime);
                }

                while (true) { }
            });
        }

        private async Task StartSerialMqttConverterAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                SerialMqttConverter.XBeeMqttConverter converter = new SerialMqttConverter.XBeeMqttConverter();
                converter.Start();

                while (true) { }
            });
        }

        //private DateTime _gatewayTime;

        //private async Task GetNICTTime()
        //{
        //    var url = "https://ntp-a1.nict.go.jp/cgi-bin/json";
        //    var req = WebRequest.Create(url);
        //    var res = await req.GetResponseAsync();

        //    using (var resStream = res.GetResponseStream())
        //    {
        //        using (var sr = new StreamReader(resStream))
        //        {
        //            using (var jsontextreader = new JsonTextReader(sr))
        //            {
        //                var des = (new JsonSerializer()).Deserialize<NitcTime>(jsontextreader);
        //                _gatewayTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(des.st))
        //                    .ToLocalTime();
        //            }
        //        }

        //    }
        //}

        private async void GetDeviceModel()
        {
        }

        private void _sensor_attach_Click(object sender, RoutedEventArgs e)
        {
            _mainwindowVM.ActivatedSensor("aaa");
        }

        private ApplicationTrigger _trigger = null;

        /// <summary>
        /// MQTTをバックグラウンドで起動する
        /// </summary>
        /// <remarks>
        /// 注意！！！
        /// 開発でローカル環境においてMQTTブローカには繋がらない場合がある
        /// Progressイベントの中でタスクがスタートしたという合図のあとは繋がる
        /// </remarks>
        private async void StartBroker()
        {
            // 一旦削除
            TaskRegistHelper.UnregisterBackgroundTasks("SIoTBroker");
            _trigger = new ApplicationTrigger();

            #region MQTT broker
            try
            {
                var registration = TaskRegistHelper.RegisterBackgroundTask(
                    "SIoTBroker.SIoTBroker",
                    "SIoTBroker",
                    _trigger,
                    null);

                await registration;

                // タスク進捗のイベント
                ((IBackgroundTaskRegistration)registration.Result).Progress += async (ss, ee) =>
                {
                    int prg = (int)(ee.Progress);

                    if (prg == 555) // タスクがスタートしたという合図
                    {
                        if (null == _mainwindowVM)
                        {
                            _mainwindowVM = new ViewModels.MainWindowViewModel();
                            _mainwindowVM.Dispatcher = Dispatcher;

                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                this.DataContext = _mainwindowVM;
                                _mainwindowVM.Run();

                                GetMyIp();
                            });
                            
                        }
                    }
                };

                // タスク終了のイベント
                ((IBackgroundTaskRegistration)registration.Result).Completed += (ss, se) =>
                {
                    System.Diagnostics.Debug.WriteLine("-> task registration is completed.");
                };

            }
            catch (Exception ex)
            {
                // バックグラウンドタスクの登録に失敗した
                // 必要ならば対処する
                throw;
            }
            #endregion

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                // Reset the completion status
                var settings = ApplicationData.Current.LocalSettings;
                settings.Values.Remove("SIoTBroker");

                //Signal the ApplicationTrigger
                var result = await _trigger.RequestAsync();
            });
        }

        private async void GetMyIp()
        {
            var hostname = Dns.GetHostName();

            var ips = await Dns.GetHostAddressesAsync(Dns.GetHostName());
            foreach (IPAddress ip in ips)
            {
                //System.Diagnostics.Debug.WriteLine($"-> {ip.ToString()}");
                if(ip.ToString().IndexOf("172.") == 0 || ip.ToString().IndexOf("192.") == 0)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                    {
                        _mainwindowVM.GetMyIp(ip.ToString());
                    });
                    return;
                }
            }
        }
    }

    public class NitcTime
    {
        public string id { get; set; }
        public decimal it { get; set; }
        public decimal st { get; set; }
        public int leap { get; set; }
        public decimal next { get; set; }
        public int step { get; set; }
    }
}
