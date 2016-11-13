using System;
using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    public sealed class SerializationInfo
    {
        public object GetValue(string name, Type type)
        {
            return new object();
        }

        internal void AddValue(string v, IList<string> errors, Type type)
        {
            throw new NotImplementedException();
        }

        internal string GetString(string v)
        {
            throw new NotImplementedException();
        }

        internal void AddValue(string v, string commandName)
        {
            throw new NotImplementedException();
        }
    }
}