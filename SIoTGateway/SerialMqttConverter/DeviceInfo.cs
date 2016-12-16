using NETMF.OpenSource.XBee.Api;

namespace SerialMqttConverter
{
    class DeviceInfo
    {
        public string deviceId { get; set; }
        public string conditionType { get; set; }
        public string settingValue { get; set; }
        public XBeeAddress address { get; set; }
    }
}
