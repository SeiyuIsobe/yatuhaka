using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Devices.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.Cooler.Telemetry.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.DataInitialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Devices.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Logging;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Repository;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator.WebJob.SimulatorCore.Transport.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using ShimadzuIoT.Sensors.Acceleration.CommandProcessors;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Simulator
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
            try
            {
                Trace.WriteLine($"以下のデバイスをAzureに登録します");
                Trace.WriteLine($"{_deviceId_debug}");

                var creator = _simulatorContainer.Resolve<IDataInitializer>();
                var list = await creator.GetAllDevicesAsync();

                Trace.WriteLine($"Azureのデバイス数：{list.Count}");

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

                }
                else
                {
                    Trace.WriteLine("既に登録されていました");
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
                Trace.WriteLine("詳細情報を新規登録します");

                DeviceModel dm = res[0] as DeviceModel;

                // nullの場合は登録された直後
                if (null == dm.DeviceProperties.HubEnabledState)
                {
                    // 状態の設定：true→実行中、false→無効
                    // ここでtrueとしたいが、時間差かなにかでDeviceListには登録されない
                    // 現時点では良い方法が思いつかないので手動で実行中にする
                    dm.DeviceProperties.HubEnabledState = false;

                    dm.IsSimulatedDevice = true;

                    AssignCommands(dm);
                }
                else
                {
                    Trace.WriteLine("詳細情報を更新します");

                    dm.DeviceProperties.HubEnabledState = !(dm.DeviceProperties.HubEnabledState);
                }

                await creator.UpdateDeviceAsync(dm);

                Trace.WriteLine("処理が終わりました");
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

        private static void AssignCommands(DeviceModel device)
        {
            //device.Commands.Add(new Command("PingDevice"));
            //device.Commands.Add(new Command("StartTelemetry"));
            //device.Commands.Add(new Command("StopTelemetry"));

            // ChangeElapseTimeCommandProcessor
            device.Commands.Add(
                new Command(
                    ChangeElapseTimeCommandParameter.CommandName,
                    new[]
                    {
                        new Parameter(
                            ChangeElapseTimeCommandParameter.TimeProperty,
                            ChangeElapseTimeCommandParameter.Time_
                            )
                    }
                )
            );


            //device.Commands.Add(new Command("ChangeSetPointTemp", new[] { new Parameter("SetPointTemp", "double"), new Parameter("SetPointHimd", "double") }));
            //device.Commands.Add(new Command("DiagnosticTelemetry", new[] { new Parameter("Active", "boolean") }));
            //device.Commands.Add(new Command("ChangeDeviceState", new[] { new Parameter("DeviceState", "string") }));
        }
    }
}
