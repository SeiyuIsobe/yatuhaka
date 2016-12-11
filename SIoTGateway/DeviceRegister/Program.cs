using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using ShimadzuIoT.Sensors.Acceleration.CommandProcessors;
using ShimadzuIoT.Sensors.Common.CommandParameters;
using ShimadzuIoT.Sensors.Common.CommandProcessors;
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
            #endregion

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

        private static string _deviceId_debug = "GW6210833_SM0771254175_SN19760824_ACCE";

        private async static void RegistDevice()
        {
            try
            {
                Console.WriteLine($"以下のデバイスをAzureに登録します");
                Console.WriteLine($"--> {_deviceId_debug}");

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
                }

                //
                // デバイスの詳細情報登録
                //
                await RegistDeviceDetail();
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

                    // テレメトリー
                    AssignTelemetry(dm);

                    // コマンド
                    AssignCommands(dm);

                    // センサー制御値
                    AssignOperationValue(dm);
                }
                else
                {
                    Console.WriteLine("詳細情報を更新します");

                    dm.DeviceProperties.HubEnabledState = !(dm.DeviceProperties.HubEnabledState);

                    // センサー制御値（デバッグ用）
                    AssignOperationValue(dm);
                }

                await creator.UpdateDeviceAsync(dm);

                Console.WriteLine("処理が終わりました");
            }
        }

        private static void AssignTelemetry(DeviceModel dm)
        {
            dm.Telemetry.Add(new Telemetry("X", "X", "double"));
            dm.Telemetry.Add(new Telemetry("Y", "Y", "double"));
            dm.Telemetry.Add(new Telemetry("Z", "Z", "double"));
        }

        /// <summary>
        /// デバイス名からデバイスを特定し、デフォルト値をセットする
        /// </summary>
        /// <param name="dm"></param>
        private static void AssignOperationValue(DeviceModel dm)
        {
            // デバイス名
            var deviceId = dm.DeviceProperties.DeviceID;

            // デバイスを特定
            var df = _deviceFactoryResolver.Resolve(deviceId);
            var device = ((IDeviceFactory)df).CreateDevice(null, null, null, null, null);

            // デフォルト値をセットする
            dm.OperationValue = device.OperationValueDefault;
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

        /// <summary>
        /// コマンド登録
        /// </summary>
        /// <param name="device"></param>
        private static void AssignCommands(DeviceModel device)
        {

            device.Commands.Add(
                new Command(
                    StartCommandProcessor.CommandName
                    ));

            device.Commands.Add(
                new Command(
                    StopCommandProcessor.CommandName
                    ));

            // ChangeElapseTimeCommandProcessor
            device.Commands.Add(
                new Command(
                    ChangeElapseTimeCommandProcessor.CommandName,
                    new[]
                    {
                        new Parameter(
                            ElapsedTimeCommandParameter.PropertyName,
                            ElapsedTimeCommandParameter.PropertyType
                            )
                    }
                )
            );
        }
    }
}
