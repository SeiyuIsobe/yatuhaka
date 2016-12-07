using ShimadzuIoT.Sensors.Acceleration.Devices;
using SIotGatewayCore.CommandProcessors;
using SIotGatewayCore.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimadzuIoT.Sensors.Acceleration.CommandProcessors
{
    public class StopCommandProcessor : CommandProcessor
    {
        private const string COMMAND_NAME = "StopTelemetry";

        public StopCommandProcessor(Device device)
            : base(device)
        {

        }

        public override async Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == COMMAND_NAME)
            {
                try
                {
                    var device = Device as Device;
                    device.OnStopTelemetryCommnad();
                    return CommandProcessingResult.Success;
                }
                catch (Exception)
                {
                    return CommandProcessingResult.RetryLater;
                }
            }
            else if (NextCommandProcessor != null)
            {
                return await NextCommandProcessor.HandleCommandAsync(deserializableCommand);
            }

            return CommandProcessingResult.CannotComplete;
        }

        protected string MyNameTelemetry
        {
            get
            {
                return COMMAND_NAME;
            }
        }
    }

    public class StopCommandProcessorParameter : StopCommandProcessor
    {
        public StopCommandProcessorParameter()
            : base(null)
        {
        }

        public static string CommandName
        {
            get
            {
                return new StopCommandProcessorParameter().MyNameTelemetry;
            }
        }
    }
}
