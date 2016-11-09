﻿using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using SIoTGateway.Cooler.Devices.Factory;
using SIoTGateway.Cooler.Telemetry.Factory;
using SIotGatewayCore.Devices.Factory;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Transport.Factory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Main.Models
{
    class SIoTGateway
    {
        private readonly Dictionary<string, CancellationTokenSource> _cancellationTokens;

        public SIoTGateway()
        {
            _cancellationTokens = new Dictionary<string, CancellationTokenSource>();
            
            _sensorModuleList.Add(new SensorManageModule() { ModuleID = "aaa" });
        }

        internal void ActivatedSensor(string sensorModuleID)
        {
            

            Stop();

            

            //Start();
        }

        private List<SensorManageModule> _sensorModuleList = new List<SensorManageModule>();

        /// <summary>
        /// IoTゲートウェイサービスを開始する
        /// </summary>
        public async void Start()
        {
            await Task.Run(async () =>
            {
                var startDeviceTasks = new List<Task>();

                foreach (SensorManageModule smm in _sensorModuleList)
                {
                    var deviceCancellationToken = new CancellationTokenSource();

                    _cancellationTokens.Add("aaa", deviceCancellationToken);
                    startDeviceTasks.Add(smm.Start(deviceCancellationToken.Token));
                }

                await Task.WhenAll(startDeviceTasks);
            });
        }

        public void Stop()
        {
            foreach(KeyValuePair<string, CancellationTokenSource> kvp in _cancellationTokens)
            {
                kvp.Value.Cancel();
            }

            _cancellationTokens.Clear();
        }
    }
}