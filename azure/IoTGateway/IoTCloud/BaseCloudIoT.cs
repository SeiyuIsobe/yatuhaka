using IoTGateway.Common;
using IoTGateway.Common.DataModels;
using IoTGateway.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTCloud
{
    public class BaseCloudIoT : ICloudIoT
    {
        private SensorContainer _sensorContainer = null;

        public BaseCloudIoT()
        {

        }

        public BaseCloudIoT(SensorContainer sensorContainer)
        {
            _sensorContainer = sensorContainer;
        }

        #region 仮想関数
        virtual public string GetCloudName()
        {
            throw new NotImplementedException();
        }

        virtual public void SendSensorData()
        {
            foreach (var sensor in _sensorContainer.GetSensor())
            {
                Publish(sensor, sensor.Data);
            }
        }

        virtual public void InitSensor()
        {
            // センサーの初期化
            foreach (var sensor in SensorContainer.GetSensor())
            {
                //
                // イベントの準備
                //
                // ステータスが変化したときのイベント
                sensor.StatusChanged += (sender, e) =>
                {
                    SensorEventArgs se = e as SensorEventArgs;
                    if (null != se)
                    {
                        switch (se.Status)
                        {
                            case Status.Running:

                                break;

                            case Status.Error:

                                AccelEventArgs ae = e as AccelEventArgs;
                                if (null != ae)
                                {
                                    System.Diagnostics.Debug.WriteLine("-> " + ae.ExceptionMessage);
                                }

                                break;
                        };
                    }
                };

                // 値が変化したときのイベント
                sensor.ValueChanged += (sender, e) =>
                {
                    // クラウドに送信
                    Publish(sender, ((ISensor)sender).Data);
                };

                // 初期化を実行
                sensor.Init();
            }
        }

        virtual public void Publish(object sensor, string mess){}

        virtual public void Connect()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region プロパティ
        public SensorContainer SensorContainer
        {
            get { return _sensorContainer; }
        }
        #endregion

        #region イベント
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler ReceivedMessage;
        #endregion

        protected void NotifyConnected(object sender, EventArgs e)
        {
            if (null != Connected)
            {
                Connected(sender, e);
            }
        }

        protected void NotifyReceiveMessage(object sender, EventArgs e)
        {
            if (null != ReceivedMessage)
            {
                ReceivedMessage(sender, e);
            }
        }

        protected void NotifyDisconnected(object sender, EventArgs e)
        {
            if (null != Disconnected)
            {
                Disconnected(sender, e);
            }
        }
    }
}
