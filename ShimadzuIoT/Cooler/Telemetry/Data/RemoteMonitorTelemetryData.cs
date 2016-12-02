namespace ShimadzuIoTGatewayCore.Cooler.Telemetry.Data
{
    public class RemoteMonitorTelemetryData
    {
        public string DeviceId { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }
        public double? ExternalTemperature { get; set; }
        // for Accelaration
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }
}
