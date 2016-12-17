using Autofac;
using Main.DataInitialization;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Newtonsoft.Json;
using SIoTBroker;
using SIotGatewayCore.Devices.Factory;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Transport.Factory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Main.Models
{
    class SIoTGateway
    {
        private readonly Dictionary<string, CancellationTokenSource> _cancellationTokens;
        private SensorModuleWatcher _sensormoduleWatcher = null;

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        public SIoTGateway()
        {
            // キャンセルトークン
            _cancellationTokens = new Dictionary<string, CancellationTokenSource>();

            // センサー基盤から送られてくるデバイス名の一覧を受信する
            _sensormoduleWatcher = new SensorModuleWatcher();

            // センサー基盤の参加通知受信イベント
            _sensormoduleWatcher.ReceivedSensorModuleName += async (sender, e) =>
            {
                SensorModule obj = sender as SensorModule;
                if(null != obj)
                {
                    // NICTから日本標準時を取得
                    // 注意事項：NICTによれば最低16秒は開けて呼び出すこととある
                    // http://www.nict.go.jp/JST/http.html
                    await this.GetNICTTime();

                    // センサー基盤に時刻を通知
                    SendGatewayTime(_gatewayTime);

                    // センサー基盤オブジェクトの生成
                    // センサー基盤のIDを設定する
                    SensorManageModule sensormodule = new SensorManageModule(obj);
                    sensormodule.DataInitializer = this.DataInitializer;
                    sensormodule.ReceivedTelemetry += (ss, ee) =>
                    {
                        if (null != ReceivedTelemetry)
                        {
                            ReceivedTelemetry(ss, null);
                        }
                    };

                    // キャンセル操作
                    var deviceCancellationToken = new CancellationTokenSource();

                    // 開始
                    await Task.Run(
                        async () =>
                        {
                            await sensormodule.Start(deviceCancellationToken.Token);
                        }, 
                        deviceCancellationToken.Token
                    );

                    // もういらない？
                    //_sensorModuleList.Add(sensormodule);
                }
            };


            

            BuildContainer();

         
        }

        internal async void SendGatewayTime(DateTime gatewayTime)
        {
            await MqttHelper.SendGatewayTimeAsync(gatewayTime);
        }

        internal void ActivatedSensor(string sensorModuleID)
        {
            

            Stop();

            

            //Start();
        }

        private List<SensorManageModule> _sensorModuleList = new List<SensorManageModule>();
        private SIoTBroker.SIoTBroker _broker = new SIoTBroker.SIoTBroker();

        public void Stop()
        {
            foreach(KeyValuePair<string, CancellationTokenSource> kvp in _cancellationTokens)
            {
                kvp.Value.Cancel();
            }

            _cancellationTokens.Clear();
        }

        private IContainer _gatewayContainer;
        public void BuildContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new GatewayModule());
            _gatewayContainer = builder.Build();
        }

        public IDataInitializer DataInitializer
        {
            get
            {
                if (null == _gatewayContainer) return null;

                return _gatewayContainer.Resolve<IDataInitializer>();
            }
        }

        private DateTime _gatewayTime;

        private async Task GetNICTTime()
        {
            var url = "https://ntp-a1.nict.go.jp/cgi-bin/json";
            var req = WebRequest.Create(url);
            var res = await req.GetResponseAsync();

            using (var resStream = res.GetResponseStream())
            {
                using (var sr = new StreamReader(resStream))
                {
                    using (var jsontextreader = new JsonTextReader(sr))
                    {
                        var des = (new JsonSerializer()).Deserialize<NitcTime>(jsontextreader);
                        _gatewayTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(Convert.ToDouble(des.st))
                            .ToLocalTime();
                    }
                }

            }
        }
    }
}
