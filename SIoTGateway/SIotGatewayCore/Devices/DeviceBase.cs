﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Factory;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Commands;
using SIotGatewayCore.CommandProcessors;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Telemetry;
using SIotGatewayCore.Telemetry.Factory;
using SIotGatewayCore.Transport;
using SIotGatewayCore.Transport.Factory;
using Newtonsoft.Json;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;
//using Microsoft.Azure.Devices.Common.Exceptions;

namespace SIotGatewayCore.Devices
{
    /// <summary>
    /// Simulates a single IoT device that sends and recieves data from a transport
    /// </summary>
    public class DeviceBase : IDevice
    {
        // pointer to the currently executing event group
        private int _currentEventGroup = 0;

        protected readonly ILogger Logger;
        protected readonly ITransportFactory TransportFactory;
        protected readonly ITelemetryFactory TelemetryFactory;
        protected readonly IConfigurationProvider ConfigProvider;
        protected ITransport Transport;
        protected CommandProcessor RootCommandProcessor;
        protected string _operationValueStream = string.Empty;

        private InitialDeviceConfig _initialDeviceConfig = null;

        public string DeviceID
        {
            get
            {
                if (null == DeviceProperties) return string.Empty;
                return DeviceProperties.DeviceID;
            }
            set { DeviceProperties.DeviceID = value; }
        }

        public string HostName { get; set; }
        public string PrimaryAuthKey { get; set; }

        private DeviceProperties _deviceProperties;
        public DeviceProperties DeviceProperties
        {
            get { return _deviceProperties; }
            set { _deviceProperties = value; }
        }

        public List<Command> Commands { get; set; }

        public List<Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Telemetry> Telemetry { get; set; }

        public List<ITelemetry> TelemetryEvents { get; private set; }
        public bool RepeatEventListForever { get; set; }

        virtual public object OperationValue
        {
            get
            {
                return _operationValueStream; // これは使ってはいけない
            }
        }

        public virtual string OperationValueDefault
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public DeviceModel InitialDevice
        {
            get
            {
                return _initialDevice;
            }

            set
            {
                _initialDevice = value;
            }
        }

        protected object _telemetryController;

        public object TelemetryController
        {
            get
            {
                return _telemetryController;
            }
        }

        public InitialDeviceConfig InitialDeviceConfig
        {
            get
            {
                return _initialDeviceConfig;
            }

            set
            {
                _initialDeviceConfig = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger">Logger where this device will log information to</param>
        /// <param name="transport">Transport where the device will send and receive data to/from</param>
        /// <param name="config">Config to start this device with</param>
        public DeviceBase(ILogger logger, ITransportFactory transportFactory, ITelemetryFactory telemetryFactory, IConfigurationProvider configurationProvider)
        {
            ConfigProvider = configurationProvider;
            Logger = logger;
            TransportFactory = transportFactory;
            TelemetryFactory = telemetryFactory;
            TelemetryEvents = new List<ITelemetry>();
        }

        public void Init(InitialDeviceConfig config)
        {
            if (null == config) return;

            InitDeviceInfo(config);

            Transport = TransportFactory.CreateTransport(this);
            _telemetryController = TelemetryFactory.PopulateDeviceWithTelemetryEvents(this);

            // テレメトリーにデータの送信先を指定する
            foreach (ITelemetry telemetry in TelemetryEvents)
            {
                telemetry.SetSendMessageAsyncFunction(new CancellationToken(), async (object eventData) =>
                {
                    await Transport.SendEventAsync(eventData);
                });
            }

            InitCommandProcessors();

            // 保持
            this.InitialDeviceConfig = config;
        }

        private DeviceModel _initialDevice = null;

        protected virtual void InitDeviceInfo(InitialDeviceConfig config)
        {
            if(null != config)
            {
                DeviceModel initialDevice = JsonConvert.DeserializeObject<DeviceModel>(config.DeviceModelJson);
                DeviceProperties = initialDevice.DeviceProperties;
                Commands = initialDevice.Commands ?? new List<Command>();
                Telemetry = initialDevice.Telemetry ?? new List<Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Telemetry>();
                HostName = config.HostName;
                PrimaryAuthKey = config.Key;
                _operationValueStream = initialDevice.OperationValue; // JSON

                InitialDevice = initialDevice; // DeviceModelを保持
                InitialDevice.KeyOfInitialDeviceConfig = config.Key; // InitialDeviceConfigを保持
            }
        }

        /// <summary>
        /// Builds up a set of commands supported by this device
        /// </summary>
        protected virtual void InitCommandProcessors()
        {
            var pingDeviceProcessor = new PingDeviceProcessor(this);

            RootCommandProcessor = pingDeviceProcessor;
        }

        public async virtual Task SendDeviceInfo()
        {
            Logger.LogInfo("Sending Device Info for device {0}...", DeviceID);
            await Transport.SendEventAsync(GetDeviceInfo());
        }

        /// <summary>
        /// Generates a DeviceInfo packet for a simulated device to send over the wire
        /// </summary>
        /// <returns></returns>
        public virtual DeviceModel GetDeviceInfo()
        {
            DeviceModel device = DeviceCreatorHelper.BuildDeviceStructure(DeviceID, true, null);
            device.DeviceProperties = this.DeviceProperties;
            device.Commands = this.Commands ?? new List<Command>();
            device.Telemetry = this.Telemetry ?? new List<Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Telemetry>();
            device.Version = SampleDeviceFactory.VERSION_1_0;
            device.ObjectType = SampleDeviceFactory.OBJECT_TYPE_DEVICE_INFO;

            // Remove the system properties from a device, to better emulate the behavior of real devices when sending device info messages.
            device.SystemProperties = null;

            return device;
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
                Transport.Open();

                var loopTasks = new List<Task>
                {
                    StartReceiveLoopAsync(token),
                    StartSendLoopAsync(token)
                };

                // Wait both the send and receive loops
                await Task.WhenAll(loopTasks.ToArray());

                // once the code makes it here the token has been canceled
                await Transport.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError("Unexpected Exception starting device: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Iterates through the list of IEventGroups and fires off the events in a given event group before moving to the next.
        /// If RepeatEventListForever is true the device will continue to loop through each event group, if false
        /// once a single pass is made through all event groups the device will stop sending events
        /// </summary>
        /// <param name="token">Cancellation token to cancel out of the loop</param>
        /// <returns></returns>
        private async Task StartSendLoopAsync(CancellationToken token)
        {
            // 今回の実験用では最初に登録した情報のままでいく
            //
            //// スタートアップ
            //ITelemetry startup = TelemetryEvents[0];
            //await startup.SendEventsAsync(token, async (object eventData) =>
            //{
            //    await Transport.SendEventAsync(eventData);
            //});

            //// 2つ目以降はセンサーデータを送信するテレメトリー
            //if(TelemetryEvents.Count > 1)
            //{
            //    // サブスクライブ開始の合図
            //    for(int i = 1; i < TelemetryEvents.Count; i++)
            //    {
            //        TelemetryEvents[i].SetSendMessageAsyncFunction(token, async (object eventData) =>
            //        {
            //            await Transport.SendEventAsync(eventData);
            //        });
            //    }
            //}

            // 意味ないけど無限ループを回す
            do
            {
                await Task.Delay(1000);
            }
            while (RepeatEventListForever && !token.IsCancellationRequested);
        }
        //private async Task StartSendLoopAsync(CancellationToken token)
        //{
        //    try
        //    {
        //        Logger.LogInfo("Booting device {0}...", DeviceID);

        //        do
        //        {
        //            _currentEventGroup = 0;

        //            Logger.LogInfo("Starting events list for device {0}...", DeviceID);

        //            while (_currentEventGroup < TelemetryEvents.Count && !token.IsCancellationRequested)
        //            {
        //                Logger.LogInfo("Device {0} starting IEventGroup {1}...", DeviceID, _currentEventGroup);

        //                var eventGroup = TelemetryEvents[_currentEventGroup];

        //                await eventGroup.SendEventsAsync(token, async (object eventData) =>
        //                {
        //                    await Transport.SendEventAsync(eventData);
        //                });

        //                _currentEventGroup++;
        //            }

        //            Logger.LogInfo("Device {0} finished sending all events in list...", DeviceID);

        //        } while (RepeatEventListForever && !token.IsCancellationRequested);

        //        Logger.LogWarning("Device {0} sent all events and is shutting down send loop. (Set RepeatEventListForever = true on the device to loop forever.)", DeviceID);

        //    }
        //    catch (TaskCanceledException)
        //    {
        //        //do nothing if the task was cancelled
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogError("Unexpected Exception starting device send loop: {0}", ex.ToString());
        //    }

        //    if (token.IsCancellationRequested)
        //    {
        //        Logger.LogInfo("********** Processing Device {0} has been cancelled - StartSendLoopAsync Ending. **********", DeviceID);
        //    }
        //}

        /// <summary>
        /// Starts the loop that listens for events/commands from the IoT Hub to be sent to this device
        /// </summary>
        /// <param name="token">Cancellation token that can stop the loop if needed</param>
        private async Task StartReceiveLoopAsync(CancellationToken token)
        {
            DeserializableCommand command;
            Exception exception;
            CommandProcessingResult processingResult;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    command = null;
                    exception = null;

                    // Pause before running through the receive loop
                    await Task.Delay(TimeSpan.FromSeconds(10), token);
                    Logger.LogInfo("Device {0} checking for commands...", DeviceID);

                    try
                    {
                        // Retrieve the message from the IoT Hub
                        command = await Transport.ReceiveAsync();

                        if (command == null)
                        {
                            continue;
                        }

                        processingResult =
                        await RootCommandProcessor.HandleCommandAsync(command);

                        switch (processingResult)
                        {
                            case CommandProcessingResult.CannotComplete:
                                await Transport.SignalRejectedCommand(command);
                                break;

                            case CommandProcessingResult.RetryLater:
                                await Transport.SignalAbandonedCommand(command);
                                break;

                            case CommandProcessingResult.Success:
                                await Transport.SignalCompletedCommand(command);
                                break;
                        }

                        //Logger.LogInfo(
                        //    "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Result: {4}{0}",
                        //    Console.Out.NewLine,
                        //    DeviceID,
                        //    command.CommandName,
                        //    command.LockToken,
                        //    processingResult);
                    }
                    //catch (IotHubException ex)
                    //{
                    //    exception = ex;

                    //    //Logger.LogInfo(
                    //    //    "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Error Type: {4}{0}Exception: {5}{0}",
                    //    //    Console.Out.NewLine,
                    //    //    DeviceID,
                    //    //    command.CommandName,
                    //    //    command.LockToken,
                    //    //    ex.IsTransient ? "Transient Error" : "Non-transient Error",
                    //    //    ex.ToString());
                    //}
                    catch (Exception ex)
                    {
                        exception = ex;

                        //Logger.LogInfo(
                        //    "Device: {1}{0}Command: {2}{0}Lock token: {3}{0}Exception: {4}{0}",
                        //    Console.Out.NewLine,
                        //    DeviceID,
                        //    command.CommandName,
                        //    command.LockToken,
                        //    ex.ToString());
                    }

                    if ((command != null) &&
                        (exception != null))
                    {
                        await Transport.SignalAbandonedCommand(command);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                //do nothing if the task was cancelled
            }
            catch (Exception ex)
            {
                Logger.LogError("Unexpected Exception starting device receive loop: {0}", ex.ToString());
            }

            Logger.LogInfo("********** Processing Device {0} has been cancelled - StartReceiveLoopAsync Ending. **********", DeviceID);
        }

        /// <summary>
        /// 共通コマンド
        /// </summary>
        public virtual void OnStartTelemetryCommand()
        {
        }

        /// <summary>
        /// 共通コマンド
        /// </summary>
        public virtual void OnStopTelemetryCommnad()
        {
        }

        /// <summary>
        /// 共通コマンド
        /// </summary>
        public virtual void OnChangeElapseTimeCommand(int time)
        {
        }

        public virtual void AssignTelemetry(DeviceModel dm)
        {
            throw new NotImplementedException();
        }

        public virtual void AssignCommands(DeviceModel dm)
        {
            throw new NotImplementedException();
        }

        public virtual void AssignOperationValue(DeviceModel dm)
        {
            throw new NotImplementedException();
        }
    }
}
