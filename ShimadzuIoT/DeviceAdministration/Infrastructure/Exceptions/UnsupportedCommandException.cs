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
    public class UnsupportedCommandException : DeviceAdministrationExceptionBase
    {
        public UnsupportedCommandException(string deviceId, string commandName) : base(deviceId)
        {
            CommandName = commandName;
        }

        #if !WINDOWS_UWP
        // protected constructor for deserialization
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected UnsupportedCommandException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            this.CommandName = info.GetString("CommandName");
        }
#endif
        public string CommandName { get; set; }

        #if !WINDOWS_UWP
        public override string Message
        {
            get
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Strings.UnsupportedCommandExceptionMessage, 
                    DeviceId, 
                    CommandName);
            }
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("CommandName", CommandName);
            base.GetObjectData(info, context);
        }
#endif
    }
}
