using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator;
using ShimadzuIoTGatewayCore.Cooler.Telemetry.Data;
using ShimadzuIoTGatewayCore.Logging;
using ShimadzuIoTGatewayCore.Telemetry;
using SiRSensors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using uPLibrary.Networking.M2Mqtt;
using System.Text;
using Newtonsoft.Json;

namespace ShimadzuIoTGatewayCore.Cooler.Telemetry
{
    public class RemoteMonitorTelemetry : ITelemetry
    {
        private readonly ILogger _logger;
        private readonly string _deviceId;

        private const int REPORT_FREQUENCY_IN_SECONDS = 5;
        private const int PEAK_FREQUENCY_IN_SECONDS = 90;

        private SampleDataGenerator _temperatureGenerator;
        private SampleDataGenerator _humidityGenerator;
        private SampleDataGenerator _externalTemperatureGenerator;

        #region 使うセンサー
        private AccelOverI2C _accelSensor = null;
        private AccelOnBoard _accelOnBoard = null;
        private GpsOnBoard _gpsOnBoard = null;
        #endregion

        public bool ActivateExternalTemperature { get; set; }

        public bool TelemetryActive { get; set; }

        #region 加速度センサー
        private double AccelaX { get; set; } = 0.0;
        private double AccelaY { get; set; } = 0.0;
        private double AccelaZ { get; set; } = 0.0;
        #endregion

        #region MQTT
        private MqttClient _mqtt = null;
        #endregion

        private Func<object, Task> _sendMessageAsync = null;

        #region 受信イベント
        public event EventHandler ReceivedTelemetry;
        #endregion

        public RemoteMonitorTelemetry(ILogger logger, string deviceId)
        {
            _logger = logger;
            _deviceId = deviceId;

            ActivateExternalTemperature = false;
            TelemetryActive = true;

            int peakFrequencyInTicks = Convert.ToInt32(Math.Ceiling((double)PEAK_FREQUENCY_IN_SECONDS /  REPORT_FREQUENCY_IN_SECONDS));

            _temperatureGenerator = new SampleDataGenerator(33, 36, 42, peakFrequencyInTicks);
            _humidityGenerator = new SampleDataGenerator(20, 50);
            _externalTemperatureGenerator = new SampleDataGenerator(-20, 120);

            // MQTT
            _mqtt = MqttHelper.Connect("127.0.0.1", deviceId, null);
            _mqtt.Subscribe(new string[] { deviceId }, new byte[] { 0 });
            _mqtt.MqttMsgPublishReceived += async (sender, e) =>
            {
                var msg = Encoding.UTF8.GetString(e.Message);
                var topic = e.Topic;

                AccelaData data = JsonConvert.DeserializeObject<AccelaData>(msg);
                if (null == data)
                {

                }
                else
                {
                    var monitorData = new RemoteMonitorTelemetryData();
                    monitorData.DeviceId = _deviceId;
                    monitorData.Temperature = data.X;
                    monitorData.Humidity = data.Y;

                    if (null != _sendMessageAsync)
                    {
                        await _sendMessageAsync(monitorData);
                    }

                    if (null != ReceivedTelemetry)
                    {
                        ReceivedTelemetry(msg, null);
                    }
                }
                
            };


            // 加速度センサー
            _accelOnBoard = new AccelOnBoard();
            _accelOnBoard.ValueChanged += (sender, e) =>
            {
                this.AccelaX = ((AccelEventArgs)e).X;
                this.AccelaY = ((AccelEventArgs)e).Y;
                this.AccelaZ = ((AccelEventArgs)e).Z;
            };
            _accelOnBoard.InitAsync();
        }

        public Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            // これ必要
            _sendMessageAsync = sendMessageAsync;

            while (true)
            {
            }
        }

        // 使わない
        public async Task botu_SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            var monitorData = new RemoteMonitorTelemetryData();
            //string messageBody; 何に使われている？
            while (!token.IsCancellationRequested)
            {
                if (TelemetryActive)
                {
                    monitorData.DeviceId = _deviceId;
                    monitorData.Temperature = _temperatureGenerator.GetNextValue();
                    monitorData.Humidity = _humidityGenerator.GetNextValue();

                    //messageBody = "Temperature: " + Math.Round(monitorData.Temperature, 2)
                    //    + " Humidity: " + Math.Round(monitorData.Humidity, 2);

                    if (ActivateExternalTemperature)
                    {
                        monitorData.ExternalTemperature = _externalTemperatureGenerator.GetNextValue();
                        //messageBody += " External Temperature: " + Math.Round((double)monitorData.ExternalTemperature, 2);
                    }
                    else
                    {
                        monitorData.ExternalTemperature = null;
                    }

                    //_logger.LogInfo("Sending " + messageBody + " for Device: " + _deviceId);

                    await sendMessageAsync(monitorData);
                }
                await Task.Delay(TimeSpan.FromSeconds(REPORT_FREQUENCY_IN_SECONDS), token);
            }
        }

        public async Task horyu_SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            
            string messageBody = string.Empty;

            // センサーデータをポーリングで取得
            while (!token.IsCancellationRequested)
            {
                var monitorData = new AccelaData(this.AccelaX, this.AccelaY, this.AccelaZ);
                monitorData.DeviceId = _deviceId;

                if (TelemetryActive)
                {
                    await sendMessageAsync(monitorData);
                }
                await Task.Delay(TimeSpan.FromSeconds(REPORT_FREQUENCY_IN_SECONDS), token); // 指定時間スリープ
            }
        }

        public void ChangeSetPointTemperature(double newSetPointTemperature)
        {
            _temperatureGenerator.ShiftSubsequentData(newSetPointTemperature);
        }

        public void SetSendMessageAsyncFunction(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            _sendMessageAsync = sendMessageAsync;
        }
    }
}