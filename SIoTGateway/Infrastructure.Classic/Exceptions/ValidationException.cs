using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if !WINDOWS_UWP
using System.Security.Permissions;
#endif

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
#if !WINDOWS_UWP
    [Serializable]
#endif
    public class ValidationException : DeviceAdministrationExceptionBase
    {
        public ValidationException(string deviceId) : base(deviceId)
        {
            Errors = new List<string>();
        }

        public ValidationException(string deviceId, Exception innerException) : base(deviceId, innerException)
        {
            Errors = new List<string>();
        }

#if !WINDOWS_UWP
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected ValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            Errors = (IList<string>)info.GetValue("Errors", typeof(IList<string>));
        }
#endif
        public IList<string> Errors { get; set; }

#if !WINDOWS_UWP
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            info.AddValue("Errors", Errors, typeof(IList<string>));
            base.GetObjectData(info, context);
        }
#endif
    }
}
