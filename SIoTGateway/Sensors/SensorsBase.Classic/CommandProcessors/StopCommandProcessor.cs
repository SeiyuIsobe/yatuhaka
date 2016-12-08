using SIotGatewayCore.CommandProcessors;
using SIotGatewayCore.Devices;
using SIotGatewayCore.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimadzuIoT.Sensors.Common.CommandProcessors
{
    /// <summary>
    /// センサーデータをクラウドに送らないコマンド
    /// </summary>
    public class StopCommandProcessor : CommandProcessor
    {
        private const string COMMAND_NAME = "StopTelemetry";

        public StopCommandProcessor(DeviceBase device)
            : base(device)
        {

        }

        public override async Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == COMMAND_NAME)
            {
                try
                {
                    var device = Device as DeviceBase;
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

        public static string CommandName
        {
            get
            {
                return (new StopCommandProcessor(null)).MyNameTelemetry;
            }
        }
    }

    [Obsolete("使用禁止", true)]
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
