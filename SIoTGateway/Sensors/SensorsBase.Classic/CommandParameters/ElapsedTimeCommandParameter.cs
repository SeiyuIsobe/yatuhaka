using Newtonsoft.Json;

namespace ShimadzuIoT.Sensors.Common.CommandParameters
{
    public class ElapsedTimeCommandParameter
    {
        // 時間間隔　ミリ秒
        // 0：取得して即送る
        public int ElapsedTime { get; set; } = 1000;

        #region コマンド登録用
        [JsonIgnore]
        public static string PropertyName { get { return "ElapsedTime"; } } // プロパティ名
        [JsonIgnore]
        public static string PropertyType { get { return "int"; } } // 型
        #endregion

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
