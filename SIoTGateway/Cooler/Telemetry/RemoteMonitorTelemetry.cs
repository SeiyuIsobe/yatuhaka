using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.SampleDataGenerator;
using SIoTGateway.Cooler.Telemetry.Data;
using SIotGatewayCore.Logging;
using SIotGatewayCore.Telemetry;
using SiRSensors;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;

namespace SIoTGateway.Cooler.Telemetry
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

        public async Task SendEventsAsync(CancellationToken token, Func<object, Task> sendMessageAsync)
        {
            var monitorData = new RemoteMonitorTelemetryData();
            //string messageBody; 何に使われている？
            while (!token.IsCancellationRequested)
            {
                if (TelemetryActive)
                {
                    monitorData.DeviceId = _deviceId;

                    // 加速度にすげ替え ->
                    //monitorData.Temperature = _temperatureGenerator.GetNextValue();
                    //monitorData.Humidity = _humidityGenerator.GetNextValue();
                    // <-
                    // ->
                    monitorData.Temperature = this.AccelaX;
                    monitorData.Humidity = this.AccelaY;
                    // <-

                    //messageBody = "Temperature: " + Math.Round(monitorData.Temperature, 2)
                    //    + " Humidity: " + Math.Round(monitorData.Humidity, 2);

                    if (ActivateExternalTemperature)
                    {
                        // 加速度にすげ替え ->
                        monitorData.ExternalTemperature = _externalTemperatureGenerator.GetNextValue();
                        // <-
                        // ->
                        //monitorData.ExternalTemperature = this.AccelaZ;
                        // <-

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
    }
}