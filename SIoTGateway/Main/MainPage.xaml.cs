using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
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
            //StartBroker();

            //断念
            //GetDeviceModel();

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

                }

                while (true) { }
            });
        }

        private void GetDeviceModel()
        {
            var url = "https://localhost:44305/api/v1/devices/GW6210833_SM0771254175_SN19760824_DKAccel_958";
            var req = WebRequest.Create(url);
            var res = req.GetResponseAsync();

            //using (var resStream = res.GetResponseStream())
            //{
            //}
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
}
