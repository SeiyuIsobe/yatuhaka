using SIotGatewayCore.Devices;

namespace SIotGatewayCore.Transport.Factory
{
    public interface ITransportFactory
    {
        ITransport CreateTransport(IDevice device);
    }
}
