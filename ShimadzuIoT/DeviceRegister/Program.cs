using Autofac;
using DeviceRegister.DataInitialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
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

        static void Main(string[] args)
        {
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

        private static string _deviceId_debug = "GW6210833_SM0771254175_SN19760824_DKAccel_958";

        private async static void RegistDevice()
        {
            var creator = _simulatorContainer.Resolve<IDataInitializer>();
            var list = await creator.GetAllDevicesAsync();
            var deviceId = _deviceId_debug;

            var res = list.FindAll(d => d.DeviceProperties.DeviceID == deviceId);
            if (null != res && res.Count == 0)
            {
                //
                // デバイスの自動登録
                //
                await creator.RegistDeviceId(deviceId);

                // 一定時間待つ
                await Task.Delay(3000);

                //
                // デバイスの詳細情報登録
                //
                await RegistDeviceDetail();
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
                    // 状態の設定：true→実行中、false→無効
                    // ここでtrueとしたいが、時間差かなにかでDeviceListには登録されない
                    // 現時点では良い方法が思いつかないので手動で実行中にする
                    dm.DeviceProperties.HubEnabledState = false;

                    dm.IsSimulatedDevice = true;
                }
                else
                {
                    dm.DeviceProperties.HubEnabledState = !(dm.DeviceProperties.HubEnabledState);
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
