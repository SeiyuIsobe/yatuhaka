using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using ShimadzuIoT.Sensors.Acceleration.CommandProcessors;
using ShimadzuIoT.Sensors.Common.CommandParameters;
using ShimadzuIoT.Sensors.Common.CommandProcessors;
using SIotGatewayCore.Devices;
using SIotGatewayCore.Devices.Factory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceRegister
{
    class Program
    {
        static CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        static IContainer _simulatorContainer;

        // デバイス・テレメトリーファクトリー解決装置
        private static DeviceFactoryResolver _deviceFactoryResolver = new DeviceFactoryResolver();

        static void Main(string[] args)
        {
            #region ここに使うセンサーのファクトリーを登録する
            // デバイスファクトリーの解決装置
            _deviceFactoryResolver.Add(new ShimadzuIoT.Sensors.Acceleration.Devices.Factory.DeviceFactory());
            _deviceFactoryResolver.Add(new ShimadzuIoT.Sensors.Atomopshere.Devices.Factory.DeviceFactory());
            _deviceFactoryResolver.Add(new ShimadzuIoT.Sensors.Temperature.Devices.Factory.DeviceFactory());
            _deviceFactoryResolver.Add(new ShimadzuIoT.Sensors.Microphone.Devices.Factory.DeviceFactory());
            #endregion
            // 引数チェック
            if(0 == args.Length)
            {
                Console.WriteLine("センサー名を指定してください");
                Console.Write(">");
                _deviceId_debug = Console.ReadLine();
            }
            else
            {
                _deviceId_debug = args[0];
            }

            BuildContainer();

            RegistDevice();

            RunAsync().Wait();
        }

        private static void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new SimulatorModule());
            _simulatorContainer = builder.Build();
        }

        private static string _deviceId_debug = string.Empty;
        private static IDevice _targetDevice = null;

        private async static void RegistDevice()
        {
            try
            {
                Console.WriteLine($"以下のデバイスをAzureに登録します");
                Console.WriteLine($"--> {_deviceId_debug}");
                Console.WriteLine($"よろしいですか？（Y/N）");
                Console.Write($">");
                var k = Console.ReadLine();
                while (!("Y" == k || "y" == k || "N" == k || "n" == k))
                {
                    Console.WriteLine($"よろしいですか？（Y/N）");
                    Console.Write($">");
                    k = Console.ReadLine();
                }
                if ("Y" == k || "y" == k)
                {
                    // no action
                }
                else if("N" == k || "n" == k)
                {
                    Console.WriteLine("処理が終わりました");
                    Console.WriteLine($"画面は手動で閉じてください");

                    return;
                }

                var df = _deviceFactoryResolver.Resolve(_deviceId_debug);
                _targetDevice = ((IDeviceFactory)df).CreateDevice(null, null, null, null, null);

                var creator = _simulatorContainer.Resolve<IDataInitializer>();
                var list = await creator.GetAllDevicesAsync(); // DocumentDBより登録デバイスを取得する

                Console.WriteLine($"Azureのデバイス数：{list.Count}");

                var deviceId = _deviceId_debug;

                var res = list.FindAll(d => d.DeviceProperties.DeviceID == deviceId);
                if (null != res && res.Count == 0)
                {
                    Console.WriteLine($"Azure IoTにデバイスを新規登録します");

                    //
                    // デバイスの自動登録
                    //
                    await creator.RegistDeviceId(deviceId);

                    // 一定時間待つ
                    await Task.Delay(3000);

                }
                else
                {
                    Console.WriteLine("既に登録されていました");
                    Console.WriteLine("登録情報を初期化しますか？（Y/N）");
                    Console.Write($">");
                    var kk = Console.ReadLine();
                    while (!("Y" == kk || "y" == kk || "N" == kk || "n" == kk))
                    {
                        Console.WriteLine($"登録情報を初期化しますか？（Y/N）");
                        Console.Write($">");
                        kk = Console.ReadLine();
                    }
                    if ("Y" == kk || "y" == kk)
                    {
                        // no action
                    }
                    else if ("N" == kk || "n" == kk)
                    {
                        Console.WriteLine("処理が終わりました");
                        Console.WriteLine($"画面は手動で閉じてください");

                        return;
                    }
                }

                //
                // デバイスの詳細情報登録
                //
                await RegistDeviceDetail();

                Console.WriteLine("処理が終わりました");
                Console.WriteLine($"画面は手動で閉じてください");

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"->{e.Message}");
            }
        }

        private static async Task RegistDeviceDetail()
        {
            var creator = _simulatorContainer.Resolve<IDataInitializer>();
            var list = await creator.GetAllDevicesAsync();
            var deviceId = _deviceId_debug;

            var res = list.FindAll(d => d.DeviceProperties.DeviceID == deviceId);
            if (null != res && res.Count > 0)
            {
                DeviceModel dm = res[0] as DeviceModel;

                // nullの場合は登録された直後
                if (null == dm.DeviceProperties.HubEnabledState)
                {
                    Console.WriteLine("詳細情報を新規登録します");

                    // 状態の設定：true→実行中、false→無効
                    // ここでtrueとしたいが、時間差かなにかでDeviceListには登録されない
                    // 現時点では良い方法が思いつかないので手動で実行中にする
                    dm.DeviceProperties.HubEnabledState = false;

                    dm.IsSimulatedDevice = true;

                    // バージョン
                    dm.Version = "1.0";

                    // テレメトリー
                    _targetDevice.AssignTelemetry(dm);

                    // コマンド
                    _targetDevice.AssignCommands(dm);

                    // センサー制御値
                    _targetDevice.AssignOperationValue(dm);
                }
                else
                {
                    Console.WriteLine("詳細情報を更新します");

                    //dm.DeviceProperties.HubEnabledState = !(dm.DeviceProperties.HubEnabledState);

                    // バージョン
                    dm.Version = "1.0";
                }

                await creator.UpdateDeviceAsync(dm);

                
            }
        }

        static async Task RunAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                Trace.TraceInformation("Running");
                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), _cancellationTokenSource.Token);
                }
                catch (TaskCanceledException) { }
            }
        }
    }
}
