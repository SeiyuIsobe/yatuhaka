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
        private Uri _resourceAddress = new Uri("https://api-m2x.att.com/v2/devices/812c5fc4cdbd6bb2f0c2cbf88463052d/streams/Accela_X/value");
        private Uri _locationAddress = new Uri("https://api-m2x.att.com/v2/devices/812c5fc4cdbd6bb2f0c2cbf88463052d/location");
        private string _locationName = "Seiyu Phone";
        private CancellationTokenSource _cts;

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
        public ClientIoT(SensorContainer sensorContainer)
            : base(sensorContainer)
        {
        }

        override public void Connect()
        {
            CreateHttpClient(ref _httpClient);
            _cts = new CancellationTokenSource();

            #region M2Xはヘッダが必要
            // キーの値はデバイスのPRIMARY API KEY
            _httpClient.DefaultRequestHeaders.Add("X-M2X-KEY", "cc8281e30c022a5f4c0c49823d2a55fa");
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
