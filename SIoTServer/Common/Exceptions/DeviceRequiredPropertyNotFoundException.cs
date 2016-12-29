using System;
using System.Runtime.Serialization;
#if !WINDOWS_UWP
using System.Security.Permissions;
#endif

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Exceptions
{
#if WINDOWS_UWP
    public class DeviceRequiredPropertyNotFoundException : Exception
    {
        public DeviceRequiredPropertyNotFoundException(string message) : base(message)
        {
        }

        public DeviceRequiredPropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
#endif
#if !WINDOWS_UWP
    /// <summary>
    /// Exception thrown when required device properties are not found.
    /// 
    /// Note that this cannot inherit from the DeviceAdminExceptionBase as we 
    /// may not know the DeviceID in this case.
    /// </summary>
    [Serializable]
    public class DeviceRequiredPropertyNotFoundException : Exception
    {
        public DeviceRequiredPropertyNotFoundException(string message) : base(message)
        {
        }

        public DeviceRequiredPropertyNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected DeviceRequiredPropertyNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }

            base.GetObjectData(info, context);
        }
    }
#endif
}
