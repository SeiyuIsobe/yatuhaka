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
using IoTGateway.Common.DataModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IoTGateway.Common;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 を参照してください

namespace IoTGateway
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

            _mainwindowVM = new ViewModels.MainWindowViewModel();
            _mainwindowVM.Dispatcher = Dispatcher;

            this.DataContext = _mainwindowVM;

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _mainwindowVM.Init();
        }
        
    }
}
