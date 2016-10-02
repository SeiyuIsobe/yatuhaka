using SiRSensors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IoTGateway.Common.DataModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IoTGateway.Common;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using System.Threading.Tasks;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace IoTGateway
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ViewModels.MainWindowViewModel _mainwindowVM = null;
        private ApplicationTrigger _trigger = null;

        public MainPage()
        {
            this.InitializeComponent();

            _mainwindowVM = new ViewModels.MainWindowViewModel();
            _mainwindowVM.Dispatcher = Dispatcher;

            this.DataContext = _mainwindowVM;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            switch(_mainwindowVM.GetCloudName())
            {
                case "AWS":
                    #region AWS
                    //
                    // AWSはnode-redを使うので自身のMQTTブローカをバックグラウンドで走らせる
                    //

                    #region MQTT broker
                    // 一旦削除
                    TaskRegistHelper.UnregisterBackgroundTasks("SiRBrokerTask");
                    _trigger = new ApplicationTrigger();
                    #endregion

                    #region MQTT broker
                    try
                    {
                        var registration = TaskRegistHelper.RegisterBackgroundTask(
                            "SiRBroker.SiRBrokerTask",
                            "SiRBrokerTask",
                            _trigger,
                            null);

                        await registration;

                        // タスク進捗のイベント
                        ((IBackgroundTaskRegistration)registration.Result).Progress += (ss, ee) =>
                        {
                            int prg = (int)(ee.Progress);

                            if (prg == 555) // タスクがスタートしたという合図
                            {
                                System.Diagnostics.Debug.WriteLine("-> 555");
                                _mainwindowVM.Init();
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

                    //// 10秒後に実行
                    //System.Diagnostics.Debug.WriteLine("-> 10秒後に実行");
                    //await Task.Delay(1000 * 10);

                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
                    {
                        // Reset the completion status
                        var settings = ApplicationData.Current.LocalSettings;
                        settings.Values.Remove("SiRBrokerTask");

                        //Signal the ApplicationTrigger
                        var result = await _trigger.RequestAsync();
                    });
                    #endregion
                    break;

                case "Azure":
                    #region Azure
                    _mainwindowVM.Init();
                    #endregion

                    break;
            };
        }
    }
}
