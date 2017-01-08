using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Main
{
    class Program
    {
        static private ViewModels.MainWindowViewModel _mainwindowVM = null;

        static void Main(string[] args)
        {
            // TODO:public async Task StartAsync(CancellationToken token)を参考に
            // Taskをリストにして、Task.WhenAlで実体化する
            //
            // SerialMQTTConverter
            var deviceCancellationToken = new CancellationTokenSource();
            Task.Run(async () =>
            {
                await StartAsync(deviceCancellationToken.Token);
            });

            while (true) ;
        }

        /// <summary>
        /// Starts the send event loop and runs the receive loop in the background
        /// to listen for commands that are sent to the device
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        static public async Task StartAsync(CancellationToken token)
        {
            try
            {
                var loopTasks = new List<Task>
                {
                    StartBrokerAsync(token),
                    //StartSerialMqttConverterAsync(token)
                };

                // Wait both the send and receive loops
                await Task.WhenAll(loopTasks.ToArray());
            }
            catch (Exception ex)
            {

            }
        }

        static private async Task StartBrokerAsync(CancellationToken token)
        {
            await Task.Run(() =>
            {
                SIoTBroker.SIoTBroker broker = new SIoTBroker.SIoTBroker();
                broker.Start();

                if (null == _mainwindowVM)
                {
                    _mainwindowVM = new Main.ViewModels.MainWindowViewModel();

                    _mainwindowVM.Run();

                    GetMyIp();

                }

                while (true) { }
            });
        }

        static private async void GetMyIp()
        {
            var hostname = Dns.GetHostName();

            var ips = await Dns.GetHostAddressesAsync(Dns.GetHostName());
            foreach (IPAddress ip in ips)
            {
                //System.Diagnostics.Debug.WriteLine($"-> {ip.ToString()}");
                if (ip.ToString().IndexOf("172.") == 0 || ip.ToString().IndexOf("192.") == 0)
                {
                    _mainwindowVM.GetMyIp(ip.ToString());
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
