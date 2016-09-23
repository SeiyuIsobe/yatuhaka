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
using IoTGateway.Common;

namespace SiRSensors
{
    [Obsolete("使用禁止", true)]
    struct Acceleration
    {
        public double X;
        public double Y;
        public double Z;

        public bool IsEqual(Acceleration acc)
        {
            if (Math.Abs(this.X - acc.X) > 0.001) return false;
            if (Math.Abs(this.Y - acc.Y) > 0.001) return false;
            if (Math.Abs(this.Z - acc.Z) > 0.001) return false;

            return true;
        }
    };

    public class AccelOverI2C : ISensor
    {
        private const byte ACCEL_I2C_ADDR = 0x53;           /* 7-bit I2C address of the ADXL345 with SDO pulled low */
        private const byte ACCEL_REG_POWER_CONTROL = 0x2D;  /* Address of the Power Control register */
        private const byte ACCEL_REG_DATA_FORMAT = 0x31;    /* Address of the Data Format register   */
        private const byte ACCEL_REG_X = 0x32;              /* Address of the X Axis data register   */
        private const byte ACCEL_REG_Y = 0x34;              /* Address of the Y Axis data register   */
        private const byte ACCEL_REG_Z = 0x36;              /* Address of the Z Axis data register   */

        private I2cDevice I2CAccel;
        private Timer periodicTimer;

        public AccelOverI2C()
        {
            
        }

        ~AccelOverI2C()
        {
            /* Cleanup */
            I2CAccel.Dispose();
        }

        public void Init()
        {
            /* Initialize the I2C bus, accelerometer, and timer */
            InitI2CAccel();
        }

        private async void InitI2CAccel()
        {

            var settings = new I2cConnectionSettings(ACCEL_I2C_ADDR);
            settings.BusSpeed = I2cBusSpeed.FastMode;
            var controller = await I2cController.GetDefaultAsync();

            if(null == controller)
            {
                if (null != StatusChanged)
                {
                    _accelEvent.Status = Status.Error;
                    _accelEvent.ExceptionMessage = "Failed to GetDefaultAsync method.";
                    StatusChanged(this, _accelEvent);
                }

                return;
            }

            I2CAccel = controller.GetDevice(settings);    /* Create an I2cDevice with our selected bus controller and I2C settings */


            /* 
             * Initialize the accelerometer:
             *
             * For this device, we create 2-byte write buffers:
             * The first byte is the register address we want to write to.
             * The second byte is the contents that we want to write to the register. 
             */
            byte[] WriteBuf_DataFormat = new byte[] { ACCEL_REG_DATA_FORMAT, 0x01 };        /* 0x01 sets range to +- 4Gs                         */
            byte[] WriteBuf_PowerControl = new byte[] { ACCEL_REG_POWER_CONTROL, 0x08 };    /* 0x08 puts the accelerometer into measurement mode */

            /* Write the register settings */
            try
            {
                I2CAccel.Write(WriteBuf_DataFormat);
                I2CAccel.Write(WriteBuf_PowerControl);
            }
            /* If the write fails display the error and stop running */
            catch (Exception ex)
            {
                if(null != StatusChanged)
                {
                    _accelEvent.Status = Status.Error;
                    _accelEvent.ExceptionMessage = "Failed to communicate with device: " + ex.Message;
                    StatusChanged(this, _accelEvent);
                }
            }

            // 状態を得るため1度だけ実行
            try
            {
                GetAccel();

                /* Now that everything is initialized, create a timer so we read data every 100mS */
                periodicTimer = new Timer(this.TimerCallback, null, 0, this.Interval);

                if(null != AccelRunning)
                {
                    _accelEvent.Status = Status.Running;
                    AccelRunning(this, _accelEvent);
                }
            }
            catch
            {
                if (null != StatusChanged)
                {
                    _accelEvent.Status = Status.Error;
                    _accelEvent.ExceptionMessage = "Failed to get Accel data";
                    StatusChanged(this, _accelEvent);
                }

                throw new Exception();
            }
            
        }

        //private Acceleration _accel;
        private AccelaData _accel;
        private Status _status = Status.Unknown;
        private AccelEventArgs _accelEvent = new AccelEventArgs();

        private void TimerCallback(object state)
        {
            GetAccel();
        }

        private void GetAccel(bool eventcall = true)
        {
            /* Read and format accelerometer data */
            try
            {
                AccelaData accel = ReadI2CAccel();

                if (true == accel.IsEqual(_accel))
                {
                    // 何もしない
                }
                else
                {
                    if(true == eventcall)
                    {
                        AccelEventArgs e = new AccelEventArgs
                        {
                            X = accel.X,
                            Y = accel.Y,
                            Z = accel.Z,
                        };

                        if (null != ValueChanged)
                        {
                            ValueChanged(this, e);
                        }
                    }
                }

                // IsEqualの結果によらず更新
                _accel = accel;
            }
            catch (Exception ex)
            {
            }
        }

        private AccelaData ReadI2CAccel()
        {
            const int ACCEL_RES = 1024;         /* The ADXL345 has 10 bit resolution giving 1024 unique values                     */
            const int ACCEL_DYN_RANGE_G = 8;    /* The ADXL345 had a total dynamic range of 8G, since we're configuring it to +-4G */
            const int UNITS_PER_G = ACCEL_RES / ACCEL_DYN_RANGE_G;  /* Ratio of raw int values to G units                          */

            byte[] RegAddrBuf = new byte[] { ACCEL_REG_X }; /* Register address we want to read from                                         */
            byte[] ReadBuf = new byte[6];                   /* We read 6 bytes sequentially to get all 3 two-byte axes registers in one read */

            /* 
             * Read from the accelerometer 
             * We call WriteRead() so we first write the address of the X-Axis I2C register, then read all 3 axes
             */
            I2CAccel.WriteRead(RegAddrBuf, ReadBuf);

            /* 
             * In order to get the raw 16-bit data values, we need to concatenate two 8-bit bytes from the I2C read for each axis.
             * We accomplish this by using the BitConverter class.
             */
            short AccelerationRawX = BitConverter.ToInt16(ReadBuf, 0);
            short AccelerationRawY = BitConverter.ToInt16(ReadBuf, 2);
            short AccelerationRawZ = BitConverter.ToInt16(ReadBuf, 4);

            /* Convert raw values to G's */
            AccelaData accel = new AccelaData(
                (double)AccelerationRawX / UNITS_PER_G,
                (double)AccelerationRawY / UNITS_PER_G,
                (double)AccelerationRawZ / UNITS_PER_G
            );

            return accel;
        }

        public event EventHandler ValueChanged;
        public event EventHandler StatusChanged;
        public event EventHandler AccelRunning;

        public int Interval { get; set; } = 1000;

        public string Data
        {
            get
            {
                return _accel.GetData();
            }
        }
    }

    //public class AccelEventArgs : EventArgs
    //{
    //    private double _x;
    //    private double _y;
    //    private double _z;

    //    public double X
    //    {
    //        get
    //        {
    //            return _x;
    //        }

    //        set
    //        {
    //            this.PreviousX = _x;
    //            _x = value;
    //        }
    //    }

    //    public double Y
    //    {
    //        get
    //        {
    //            return _y;
    //        }

    //        set
    //        {
    //            this.PreviousY = _y;
    //            _y = value;
    //        }
    //    }

    //    public double Z
    //    {
    //        get
    //        {
    //            return _z;
    //        }

    //        set
    //        {
    //            this.PreviousZ = _z;
    //            _z = value;
    //        }
    //    }

    //    public double PreviousX { get; set; }
    //    public double PreviousY { get; set; }
    //    public double PreviousZ { get; set; }

    //    private Status _status;
    //    public Status Status
    //    {
    //        get { return _status; }
    //        set
    //        {
    //            this.PreviousStatus = _status;
    //            _status = value;

    //            // statusが変化したときはここでメッセージを初期化して
    //            // メッセージの取違ミスが発生しないようにする
    //            ExceptionMessage = string.Empty;
    //        }
    //    }

    //    public Status PreviousStatus { get; set; } = Status.Unknown;

    //    public string ExceptionMessage { get; set; }
    //}

    //public enum Status
    //{
    //    Unknown,
    //    Wait,
    //    Running,
    //    Error
    //}
}
