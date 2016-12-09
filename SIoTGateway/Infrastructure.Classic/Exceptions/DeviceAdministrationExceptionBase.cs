using System;
using System.Globalization;
using System.Runtime.Serialization;
#if !WINDOWS_UWP
using System.Security.Permissions;
#endif

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    /// <summary>
    /// Simple base class for device administration based exceptions
    /// </summary>
#if !WINDOWS_UWP
    [Serializable]
#endif
    public abstract class DeviceAdministrationExceptionBase : Exception
    {
        // TODO: Localize this, if neccessary.
        private const string deviceIdMessageFormatString = "DeviceId: {0}";

        public string DeviceId { get; set; }

        public DeviceAdministrationExceptionBase() : base()
        {
        }

        public DeviceAdministrationExceptionBase(string deviceId) 
            : base(string.Format(CultureInfo.CurrentCulture, deviceIdMessageFormatString, deviceId))
        {
            DeviceId = deviceId;
        }

        public DeviceAdministrationExceptionBase(string deviceId, Exception innerException)
            : base(string.Format(CultureInfo.CurrentCulture, deviceIdMessageFormatString, deviceId), innerException)
        {
            DeviceId = deviceId;
        }

        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceAdministrationExceptionBase(SerializationInfo info, StreamingContext context)
#if !WINDOWS_UWP
            : base(info, context)
#endif
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            this.DeviceId = info.GetString("DeviceId");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
#if !WINDOWS_UWP
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
#endif
#if WINDOWS_UWP
        public void GetObjectData(SerializationInfo info, StreamingContext context)
#endif
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("DeviceId", DeviceId);
#if !WINDOWS_UWP
            base.GetObjectData(info, context);
#endif
        }
    }
}
