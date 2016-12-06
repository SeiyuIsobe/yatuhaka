using System;

namespace ShimadzuIoT.Sensors.Telemetry.Data
{
    public class RemoteMonitorTelemetryDataBase
    {
        // Common
        public string DeviceId { get; set; }
        public string ObjectType { get; set; } = null;
        public DateTime Timestamp { get; set; }
        // for Cooler
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double? ExternalTemperature { get; set; }
        // for Accelaration
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
