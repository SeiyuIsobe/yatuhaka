using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common
{
    public class SensorEventArgs : System.EventArgs
    {
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
