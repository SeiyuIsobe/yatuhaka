using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    internal class SecurityPermissionAttribute : Attribute
    {
        public SecurityPermissionAttribute(SecurityAction action)
        { }

        public bool SerializationFormatter { get; set; }
    }
}