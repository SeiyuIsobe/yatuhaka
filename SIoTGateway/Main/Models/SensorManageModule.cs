using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Repository;
using Newtonsoft.Json;
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
    /// <summary>
    /// センサー基板
    /// </summary>
    public class SensorManageModule
    {
        public string ModuleID { get; private set; }
        private SensorModuleWatcher _sensormoduleWatcher = null;

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        public SensorManageModule(string modeleID)
        {
            this.ModuleID = modeleID;

            // センサー基盤から送られてくるデバイス名の一覧を受信する
            _sensormoduleWatcher = new SensorModuleWatcher(this.ModuleID);
            _sensormoduleWatcher.ReceivedDeviceNames += (sender, e) =>
            {
                var sensorlist = SensorList.ToObject(sender.ToString());

                // 本来ならここでクラウドにデバイス名を登録したいところだが
                // どうも現時点ではUWP用の.NETが対応してないらしいので出来ない。
                // デバイス名は手動で登録してもらうようにする
                // 受信したものは既に登録されているかどうかも構わずクラウドに登録する
                foreach (string id in sensorlist.Sensors)
                {
                    DeviceModel device = DeviceCreatorHelper.BuildDeviceStructure(id, true, null);

                    // サンプルの通り以下のように登録したいが出来ない
                    //var generator = new  SecurityKeyGenerator
                    //SecurityKeys generatedSecurityKeys = (new SecurityKeyGenerator()).
                    //_securityKeyGenerator.CreateRandomKeys();
                    //await this.AddDeviceToRepositoriesAsync(device, generatedSecurityKeys);

                    var device_json = JsonConvert.SerializeObject(device);

                    #region REST
                    // RESTを直接たたくようにしてみたが、開発環境では証明書絡みでエラーが出る
                    // クラウドのRESTをたたくとどうなるかは未確認
                    //HttpClient httpClient = new HttpClient();
                    //CancellationTokenSource _cts = new CancellationTokenSource();

                    //var credentials = Encoding.ASCII.GetBytes("myUsername:myPassword");
                    //httpClient.DefaultRequestHeaders.Authorization = new Windows.Web.Http.Headers.HttpCredentialsHeaderValue("Basic", Convert.ToBase64String(credentials));

                    //var response = await httpClient.PostAsync(
                    //    new Uri("https://localhost:44305/api/v1/devices"),
                    //    new HttpStringContent(device_json, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json")
                    //    ).AsTask(_cts.Token);
                    #endregion
                }
            };
        }


        /// <summary>
        /// IoTゲートウェイサービスを開始する
        /// </summary>
        public async Task Start(CancellationToken token)
        {
            //
            // BulkDeviceTesterに依存関係を注入する
            // 
            var logger = new TraceLogger();

            // Azureへの接続先を管理するオブジェクトを生成する
            var configProvider = new ConfigurationProvider();

            // BLOBのテーブルに接続するオブジェクトを生成する
            var tableStorageClientFactory = new AzureTableStorageClientFactory();

            // テレメトリー
            var telemetryFactory = new CoolerTelemetryFactory(logger);
            telemetryFactory.ReceivedTelemetry += (sender, e) =>
            {
                if(null != ReceivedTelemetry)
                {
                    ReceivedTelemetry(sender, e);
                }
            };

            // Azure IoT Hubに接続するオブジェクトを生成する
            var transportFactory = new IotHubTransportFactory(logger, configProvider);

            IVirtualDeviceStorage deviceStorage = null;
            var useConfigforDeviceList = Convert.ToBoolean(configProvider.GetConfigurationSettingValueOrDefault("UseConfigForDeviceList", "False"), CultureInfo.InvariantCulture);

            // デバイスを登録しているAzure StrageのTableサービスに接続するオブジェクトを生成する
            deviceStorage = new VirtualDeviceTableStorage(configProvider, tableStorageClientFactory);

            //
            // ここでCoolerDeviceFactoryに決め打ってしまっているのは良くない
            //
            //IDeviceFactory deviceFactory = new CoolerDeviceFactory();
            // デバイス名に埋め込まれた種類によってデバイスを作成するAnyDeviceFactoryを使うことにする
            IDeviceFactory deviceFactory = new AnyDeviceFactory();

            //// Start Simulator
            var tester = new BulkDeviceTester(transportFactory, logger, configProvider, telemetryFactory, deviceFactory, deviceStorage);
            await Task.Run(() => tester.ProcessDevicesAsync(token), token);
        }
    }
}
