using ShimadzuIoTGatewayCore.Devices;

namespace ShimadzuIoTGatewayCore.Transport.Factory
{
    public interface ITransportFactory
    {
        ITransport CreateTransport(IDevice device);
    }
}
