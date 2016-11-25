using ShimadzuIoT.Sensors.Telemetry.Data;

namespace ShimadzuIoT.Sensors.Acceleration.Telemetry.Data
{
    public class RemoteMonitorTelemetryData : RemoteMonitorTelemetryDataBase
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
