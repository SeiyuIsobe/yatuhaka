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
    public class DeviceNotRegisteredException : DeviceAdministrationExceptionBase
    {
        public DeviceNotRegisteredException(string deviceId) : base(deviceId)
        {
        }

        #if !WINDOWS_UWP
        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceNotRegisteredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
#endif

        public override string Message
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.DeviceNotRegisteredExceptionMessage, 
                    DeviceId);
            }
        }
    }
}
