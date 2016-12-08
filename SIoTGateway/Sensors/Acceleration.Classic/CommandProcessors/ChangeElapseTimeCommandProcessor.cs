using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Newtonsoft.Json;
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
    /// <summary>
    /// 加速度センサーのデータ送信時間間隔を設定するコマンド
    /// </summary>
    public class ChangeElapseTimeCommandProcessor : CommandProcessor
    {
        private const string COMMAND_NAME = "ChangeElapseTime";

        public ChangeElapseTimeCommandProcessor(Device device)
            : base(device)
        {
        }

        public override async Task<CommandProcessingResult> HandleCommandAsync(DeserializableCommand deserializableCommand)
        {
            if (deserializableCommand.CommandName == COMMAND_NAME)
            {
                CommandHistory commandHistory = deserializableCommand.CommandHistory;

                try
                {
                    var device = Device as Device;
                    if(null != device)
                    {
                        var parameters = commandHistory.Parameters;
                    }

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
                return (new ChangeElapseTimeCommandProcessor(null)).MyNameTelemetry;
            }
        }
    }

    [Obsolete("使用禁止", true)]
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

#region ________public int Time { get; set; } = 1000;
        public int Time { get; set; } = 1000;
        [JsonIgnore]
        public static string TimeProperty { get; } = "Time"; // コマンド登録用
        [JsonIgnore]
        public static string Time_ { get; } = "int"; // コマンド登録用
        #endregion
    }
}
