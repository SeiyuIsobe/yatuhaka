using Newtonsoft.Json;
using ShimadzuIoT.Sensors.Acceleration.Devices;
using ShimadzuIoTGatewayCore.CommandProcessors;
using ShimadzuIoTGatewayCore.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShimadzuIoT.Sensors.Acceleration.CommandProcessors
{
    /// <summary>
    /// 加速度センサーのデータ送信時間間隔を設定するコマンド
    /// </summary>
    public class ChangeElapseTimeCommandProcessor : CommandProcessor
    {
        private const string MYNAME_TELEMETRY = "ChangeElapseTime";

        public ChangeElapseTimeCommandProcessor(Device device)
            : base(device)
        {
        }

        public override async Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == MYNAME_TELEMETRY)
            {
                try
                {
                    var device = Device as Device;
                    
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
                return MyNameTelemetry;
            }
        }
    }

    public class ChangeElapseTimeCommandParameter : ChangeElapseTimeCommandProcessor
    {
        public ChangeElapseTimeCommandParameter()
            :base(null)
        {
        }

        public static ChangeElapseTimeCommandParameter ToObject(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<ChangeElapseTimeCommandParameter>(json);
            }
            catch { }
            return null;
        }

        public static string CommandName
        {
            get
            {
                return new ChangeElapseTimeCommandParameter().MyNameTelemetry;
            }
        }

#region public int Time { get; set; } = 1000;
        public int Time { get; set; } = 1000;
        [JsonIgnore]
        public static string TimeProperty { get; private set; } = "Time";
        [JsonIgnore]
        public static string Time_ { get; private set; } = "int";
#endregion
    }
}
