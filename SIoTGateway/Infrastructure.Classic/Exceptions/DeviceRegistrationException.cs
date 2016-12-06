using System;
using System.Globalization;
using System.Runtime.Serialization;
#if !WINDOWS_UWP
using System.Security.Permissions;
#endif

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    #if !WINDOWS_UWP
    [Serializable]
#endif
    public class DeviceRegistrationException : DeviceAdministrationExceptionBase
    {
        public DeviceRegistrationException(string deviceId, Exception innerException) : base(deviceId, innerException)
        {
        }

        #if !WINDOWS_UWP
        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceRegistrationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif

        public override string Message
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.DeviceRegistrationExceptionMessage, 
                    DeviceId);
            }
        }
    }
}
