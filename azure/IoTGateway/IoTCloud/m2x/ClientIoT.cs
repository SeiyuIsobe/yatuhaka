using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using IoTGateway.Common.DataModels;
using IoTGateway.Common;
using IoTGateway.Common.Interfaces;
using System.Diagnostics;
using Windows.Web.Http;
using Windows.Web.Http.Filters;
using Windows.Web.Http.Headers;
using System.Threading;
using System.Globalization;

namespace IoTCloud.m2x
{
    public class ClientIoT : BaseCloudIoT, ICloudIoT
    {
        private HttpClient _httpClient;
        private Uri _resourceAddress = null;// new Uri("https://api-m2x.att.com/v2/devices/812c5fc4cdbd6bb2f0c2cbf88463052d/streams/Accela_X/value");
        private Uri _locationAddress = null;//new Uri("https://api-m2x.att.com/v2/devices/812c5fc4cdbd6bb2f0c2cbf88463052d/location");
        private CancellationTokenSource _cts;

        private string _m2Xaddress = string.Empty;
        private string _deviceId = string.Empty;
        private string _apiKey = string.Empty;
        private string _streamName = string.Empty;
        private string _locationName = string.Empty;

        private M2XSetting _m2xsetting = null;

        override public string GetCloudName()
        {
            return "M2X";
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ClientIoT()
        { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ClientIoT(SensorContainer sensorContainer, ISettingCloud setting)
            : base(sensorContainer)
        {
            _m2xsetting = (M2XSetting)setting;
            _m2Xaddress = _m2xsetting.M2Xaddress;
            _deviceId = _m2xsetting.DeviceID;
            _apiKey = _m2xsetting.APIKey;
            _streamName = _m2xsetting.StreamName;
            _locationName = _m2xsetting.LocationName;

            _resourceAddress = new Uri($"{_m2Xaddress}/{_deviceId}/streams/{_streamName}/value");
            _locationAddress = new Uri($"{_m2Xaddress}/{_deviceId}/location");
        }

        override public void Connect()
        {
            CreateHttpClient(ref _httpClient);
            _cts = new CancellationTokenSource();

            #region M2Xはヘッダが必要
            // キーの値はデバイスのPRIMARY API KEY
            _httpClient.DefaultRequestHeaders.Add("X-M2X-KEY", _apiKey);
            #endregion

            NotifyConnected(this, null);

        }

        public async override void Publish(object sensor, string mess)
        {
            try
            {
                if(sensor is SiRSensors.AccelOnBoard || sensor is SiRSensors.AccelOverI2C)
                {
                    // ここではX軸に注目
                    AccelaData ad = AccelaData.GetObject(mess);
                    if (null != ad)
                    {
                        // 時刻表示をISOにする
                        var sss = @"{""timestamp"":""" + DateTime.Now.ToString("o") + @""", ""value"": " + ad.X.ToString() + "}";

                        //System.Diagnostics.Debug.WriteLine($"-> {sss}");

                        // 送信
                        var response = await _httpClient.PutAsync(_resourceAddress,
                           new HttpStringContent(sss, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json")).AsTask(_cts.Token);
                    }
                }

                if(sensor is SiRSensors.GpsOnBoard)
                {
                    // ここではGPSに注目
                    PositionData pd = PositionData.GetObject(mess);
                    if (null != pd)
                    {
                        var sss = $"{{\"latitude\":{pd.Latitude}, \"longitude\":{pd.Longitude}, \"name\":\"{_locationName}\"}}";

                        System.Diagnostics.Debug.WriteLine($"-> {sss}");

                        // 送信
                        var response = await _httpClient.PutAsync(_locationAddress,
                           new HttpStringContent(sss, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json")).AsTask(_cts.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
            }
            finally
            {
            }
        }

        internal void CreateHttpClient(ref HttpClient httpClient)
        {
            if (httpClient != null)
            {
                httpClient.Dispose();
            }

            // HttpClient functionality can be extended by plugging multiple filters together and providing
            // HttpClient with the configured filter pipeline.
            IHttpFilter filter = new HttpBaseProtocolFilter();
            filter = new PlugInFilter(filter); // Adds a custom header to every request and response message.
            httpClient = new HttpClient(filter);

            // The following line sets a "User-Agent" request header as a default header on the HttpClient instance.
            // Default headers will be sent with every request sent from this HttpClient instance.
            httpClient.DefaultRequestHeaders.UserAgent.Add(new HttpProductInfoHeaderValue("Sample", "v8"));
        }
    }
}
