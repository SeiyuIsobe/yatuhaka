﻿using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using ShimadzuIoTGatewayCore.Devices;
using ShimadzuIoTGatewayCore.Logging;

namespace ShimadzuIoTGatewayCore.Transport.Factory
{
    public class IotHubTransportFactory : ITransportFactory
    {
        private ILogger _logger;
        private IConfigurationProvider _configurationProvider;

        public IotHubTransportFactory(ILogger logger,
            IConfigurationProvider configurationProvider)
        {
            _logger = logger;
            _configurationProvider = configurationProvider;
        }

        public ITransport CreateTransport(IDevice device)
        {
            return new IoTHubTransport(_logger, _configurationProvider, device);
        }
    }
}