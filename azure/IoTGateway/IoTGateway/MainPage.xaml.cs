using IoTGateway.azure;
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

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace IoTGateway
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AccelOverI2C _accelSensor = null;
        private ClientIoT _client = null;

        public MainPage()
        {
            this.InitializeComponent();

            #region 加速度センサー
            _accelSensor = new AccelOverI2C();

            _accelSensor.AccelRunning += (sender, e) =>
            {

            };

            _accelSensor.StatusChanged += (sender, e) =>
            {
                System.Diagnostics.Debug.Write("-> " + ((AccelEventArgs)e).ExceptionMessage);
            };

            _accelSensor.AccelChanged += (sender, e) =>
            {
                var ee = e as AccelEventArgs;
                System.Diagnostics.Debug.WriteLine("{0}, {1}, {2}", ee.X, ee.Y, ee.Z);
            };

            _accelSensor.Interval = 1000;
            _accelSensor.Init();
            #endregion

            #region Azure
            _client = new ClientIoT();
            _client.Connect();
            #endregion

        }

        private void _sendButton_Click(object sender, RoutedEventArgs e)
        {
            _client.Publish("kei");
        }
    }
}
