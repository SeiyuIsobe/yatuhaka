using Newtonsoft.Json;

namespace ShimadzuIoT.Sensors.Common.CommandParameters
{
    /// <summary>
    /// 加速度値をクラウドに送る・送らないフラグを保持する
    /// </summary>
    public class IsAvailableCommandParameter
    {
        // true：送る／false：送らない
        public bool IsAvailable { get; set; } = true;

        #region コマンド登録用
        [JsonIgnore]
        public static string PropertyName { get { return "IsAvailable"; } } // プロパティ名
        [JsonIgnore]
        public static string PropertyType { get { return "bool"; } } // 型
        #endregion

        public override string ToString()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this);
        }
    }
}
