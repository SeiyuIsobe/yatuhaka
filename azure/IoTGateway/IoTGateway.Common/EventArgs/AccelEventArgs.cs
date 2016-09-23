using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IoTGateway.Common;

namespace IoTGateway.Common
{
    public class AccelEventArgs : System.EventArgs
    {
        private double _x;
        private double _y;
        private double _z;

        public double X
        {
            get
            {
                return _x;
            }

            set
            {
                this.PreviousX = _x;
                _x = value;
            }
        }

        public double Y
        {
            get
            {
                return _y;
            }

            set
            {
                this.PreviousY = _y;
                _y = value;
            }
        }

        public double Z
        {
            get
            {
                return _z;
            }

            set
            {
                this.PreviousZ = _z;
                _z = value;
            }
        }

        public double PreviousX { get; set; }
        public double PreviousY { get; set; }
        public double PreviousZ { get; set; }

        private Status _status;
        public Status Status
        {
            get { return _status; }
            set
            {
                this.PreviousStatus = _status;
                _status = value;

                // statusが変化したときはここでメッセージを初期化して
                // メッセージの取違ミスが発生しないようにする
                ExceptionMessage = string.Empty;
            }
        }

        public Status PreviousStatus { get; set; } = Status.Unknown;

        public string ExceptionMessage { get; set; }
    }
}
