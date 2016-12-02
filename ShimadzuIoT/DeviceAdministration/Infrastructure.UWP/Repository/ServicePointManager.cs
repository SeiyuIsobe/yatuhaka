namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    internal class ServicePointManager
    {
        public static System.Func<object, object, object, object, bool> ServerCertificateValidationCallback { get; internal set; }
    }
}