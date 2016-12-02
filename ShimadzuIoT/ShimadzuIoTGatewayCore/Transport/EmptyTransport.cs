using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShimadzuIoTGatewayCore.Logging;

namespace ShimadzuIoTGatewayCore.Transport
{
    public class EmptyTransport : ITransport
    {
        private readonly ILogger _logger;

        public EmptyTransport(ILogger logger)
        {
            _logger = logger;
        }

        public void Open()
        {
            return;
        }

        public Task CloseAsync()
        {
            return Task.Run(() => { });
        }

        public async Task SendEventAsync(dynamic eventData)
        {
            var eventId = Guid.NewGuid();
            await SendEventAsync(eventId, eventData);
        }

        public async Task SendEventAsync(Guid eventId, dynamic eventData)
        {
            _logger.LogInfo("SendEventAsync called:");
            _logger.LogInfo("SendEventAsync: EventId: " + eventId.ToString());
            _logger.LogInfo("SendEventAsync: message: " + eventData.ToString());

            await Task.Run(() => { return; });
        }

        public async Task SendEventBatchAsync(IEnumerable<Microsoft.Azure.Devices.Client.Message> messages)
        {
            _logger.LogInfo("SendEventBatchAsync called");

            await Task.Run(() => { return; });
        }

        public async Task<DeserializableCommand> ReceiveAsync()
        {
            _logger.LogInfo("ReceiveAsync: waiting...");
            return await Task.Run(() => new DeserializableCommand(new Microsoft.Azure.Devices.Client.Message()));
        }

        public async Task SignalAbandonedCommand(DeserializableCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            await Task.Run(() => { });
        }

        public async Task SignalCompletedCommand(DeserializableCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            await Task.Run(() => { });
        }

        public async Task SignalRejectedCommand(DeserializableCommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException("command");
            }

            await Task.Run(() => { });
        }
    }
}
