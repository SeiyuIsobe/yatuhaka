using IoTGateway.Common.DataModels;
using IoTGateway.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using IoTGateway.Common;
using Windows.Devices.Sensors;
using Windows.Devices.Geolocation;

namespace SiRSensors
{
    public class GpsOnBoard : ISensor
    {
        #region ISensor
        public string Data
        {
            get
            {
                return _positionData.GetData();
            }
        }

        public event EventHandler StatusChanged;
        public event EventHandler ValueChanged;

        public void Init()
        {
            InitDevice();
        }
        #endregion

        private Geolocator _watcher = null;

        private async void InitDevice()
        {
            // GPSを使う
            if (null == _watcher)
            {
                _watcher = new Geolocator();
                if (null != _watcher)
                {
                    _watcher.MovementThreshold = 20;
                    _watcher.PositionChanged += this._watcher_PositionChanged;
                    _watcher.StatusChanged += this._watcher_StatusChanged;

                    Geoposition pos = await _watcher.GetGeopositionAsync();

                }

            }

            // 状態の連絡
            if (null != _watcher)
            {
                if (null != StatusChanged)
                {
                    var e = new GeolocationEventArgs();
                    e.Status = Status.Running;
                    StatusChanged(this, e);
                }
            }
            else
            {
                if (null != StatusChanged)
                {
                    var e = new GeolocationEventArgs();
                    e.Status = Status.Error;
                    e.ExceptionMessage = "Fail to use GPS.";
                    StatusChanged(this, e);
                }
            }
        }

        private PositionData _positionData = null;

        private void _watcher_PositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            Geoposition pos = e.Position;

            _positionData = new PositionData(
                pos.Coordinate.Point.Position.Latitude,
                pos.Coordinate.Point.Position.Longitude
            );

            System.Diagnostics.Debug.WriteLine("{0},{1}", pos.Coordinate.Point.Position.Latitude, pos.Coordinate.Point.Position.Longitude);

            if(null != ValueChanged)
            {
                GeolocationEventArgs ee = new GeolocationEventArgs
                {
                    Latitude = _positionData.Latitude,
                    Longitude = _positionData.Longitude
                };

                ValueChanged(this, ee);
            }
        }

        private void _watcher_StatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
        }

        private string GetStatusString(PositionStatus status)
        {
            var strStatus = "";

            switch (status)
            {
                case PositionStatus.Ready:
                    strStatus = "Location is available.";
                    break;

                case PositionStatus.Initializing:
                    strStatus = "Geolocation service is initializing.";
                    break;

                case PositionStatus.NoData:
                    strStatus = "Location service data is not available.";
                    break;

                case PositionStatus.Disabled:
                    strStatus = "Location services are disabled. Use the " +
                                "Settings charm to enable them.";
                    break;

                case PositionStatus.NotInitialized:
                    strStatus = "Location status is not initialized because " +
                                "the app has not yet requested location data.";
                    break;

                case PositionStatus.NotAvailable:
                    strStatus = "Location services are not supported on your system.";
                    break;

                default:
                    strStatus = "Unknown PositionStatus value.";
                    break;
            }

            return (strStatus);

        }
    }
}
