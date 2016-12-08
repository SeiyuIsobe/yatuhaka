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
    /// センサーデータをクラウドに送るコマンド
    /// </summary>
    public class StartCommandProcessor : CommandProcessor
    {
        private const string COMMAND_NAME = "StartTelemetry";

        public StartCommandProcessor(DeviceBase device)
            : base(device)
        {

        }

        public async override Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == COMMAND_NAME)
            {
                try
                {
                    var device = Device as DeviceBase;
                    device.OnStartTelemetryCommand();
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
                return (new StartCommandProcessor(null)).MyNameTelemetry;
            }
        }
    }

    [Obsolete("使用禁止", true)]
    public class StartCommandProcessorParameter : StartCommandProcessor
    {
        public StartCommandProcessorParameter()
            :base(null)
        {
        }

        public static string CommandName
        {
            get
            {
                return new StartCommandProcessorParameter().MyNameTelemetry;
            }
        }
    }
}
