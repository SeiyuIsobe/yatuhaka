using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Interfaces;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models.Sensor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.I2c;
using Windows.Devices.Sensors;

namespace SiRSensors
{
    public class AccelOnBoard : ISensor
    {
        #region ISensor
        public string Data
        {
            get
            {
                return _accel.GetData();
            }
        }

        public event EventHandler StatusChanged;
        public event EventHandler ValueChanged;
        #endregion

        private Accelerometer _accelerometer = null;
        private AccelaData _accel;
        private Status _status = Status.Unknown;
        private AccelEventArgs _accelEvent = new AccelEventArgs();

        public void Init()
        {
            InitDevice();
        }

        public async void InitAsync()
        {
            await Task.Run(() => { InitDevice(); });
        }

        private void InitDevice()
        {
            // 加速度計を使う
            _accelerometer = Accelerometer.GetDefault();
            if (null != _accelerometer)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = 1000; // 100ミリにする

                _accelerometer.ReadingChanged += _accelerometer_ReadingChanged;
            }

            // 状態の連絡
            if (null != _accelerometer)
            {
                if (null != StatusChanged)
                {
                    _accelEvent.Status = Status.Running;
                    StatusChanged(this, _accelEvent);
                }
            }
            else
            {
                if (null != StatusChanged)
                {
                    _accelEvent.Status = Status.Error;
                    _accelEvent.ExceptionMessage = "Failed to get Accel Device";
                    StatusChanged(this, _accelEvent);
                }
            }
        }

        private long _prereportcount = 0;

        private void _accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs e)
        {
            var et = this.GetElapsedTime();

            // なんとなく加速度が変化したイベントがReportIntervalより早く来ている
            // 気がするので、念のため前回の経過時間を保持しておいて
            // ReportIntervalより経過指定かどうかを確かめることにする
            if (et - _prereportcount >= _accelerometer.ReportInterval)
            {
                AccelerometerReading reading = e.Reading;

                AccelaData accel = new AccelaData(
                    reading.AccelerationX,
                    reading.AccelerationY,
                    reading.AccelerationZ
                );

                _accel = accel; // 保持

                if (null != ValueChanged)
                {
                    AccelEventArgs ee = new AccelEventArgs
                    {
                        X = accel.X,
                        Y = accel.Y,
                        Z = accel.Z
                    };
                    
                    ValueChanged(this, ee);
                }

                _prereportcount = et;
            }
        }

        private Stopwatch _stopwatch = null;

        private long GetElapsedTime()
        {
            if (null == _stopwatch)
            {
                _stopwatch = Stopwatch.StartNew();
                return 0;
            }

            return _stopwatch.ElapsedMilliseconds;
        }
    }
}
