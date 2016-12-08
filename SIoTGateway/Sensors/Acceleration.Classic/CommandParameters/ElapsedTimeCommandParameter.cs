using Newtonsoft.Json;

namespace ShimadzuIoT.Sensors.Acceleration.CommandParameters
{
    /// <summary>
    /// 加速度値をクラウドに送る時間間隔を保持する
    /// </summary>
    public class ElapsedTimeCommandParameter
    {
        // 加速度値をクラウドに送る時間間隔　単位はミリ秒
        // 0：取得して即送る
        public int ElapsedTime { get; set; } = 1000;

        #region コマンド登録用
        /// <summary>
        /// プロパティ名
        /// </summary>
        [JsonIgnore]
        public static string PropertyName { get { return "ElapsedTime"; } }
        /// <summary>
        /// プロパティの型
        /// </summary>
        [JsonIgnore]
        public static string PropertyType { get { return "int"; } }
        #endregion

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
