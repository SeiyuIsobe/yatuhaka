using System;
using Windows.Security.Cryptography;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    internal class RNGCryptoServiceProvider : IDisposable
    {
        public void Dispose()
        {
        }

        internal void GetBytes(byte[] primaryRawRandomBytes)
        {
            byte[] temp = new byte[primaryRawRandomBytes.Length];

            var buffer = CryptographicBuffer.GenerateRandom((uint)primaryRawRandomBytes.Length);
            CryptographicBuffer.CopyToByteArray(buffer, out temp);

            for (int i = 0; i < primaryRawRandomBytes.Length; i++) primaryRawRandomBytes[i] = temp[i];
        }
    }
}