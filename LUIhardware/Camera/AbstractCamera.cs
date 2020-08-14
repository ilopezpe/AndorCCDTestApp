using LuiHardware.Object;
using System;
using System.Linq;

namespace LuiHardware.Camera
{
    public abstract class AbstractCamera : LuiObject<CameraParameters>, ICamera
    {
        public abstract int Serial { get; set; }
        public abstract int numCameras { get; set; }
        public abstract int XDim { get; set; }

        public abstract int YDim { get; set; }

        public abstract int GetTrackHeight { get; }

        public abstract int AcquisitionMode { get; set; }

        public abstract int TriggerMode { get; set; }

        public abstract int DDGTriggerMode { get; set; }

        public abstract int GateMode { get; set; }

        public abstract int ReadMode { get; set; }

        public abstract bool HasIntensifier { get; }

        public abstract int Gating { get; set; }

        public abstract int MCPGainMax { get; set; }

        public abstract int MCPGainMin { get; set; }

        public abstract int Gain { get; set; }

        public abstract int NumberADChannels { get; set; }

        public abstract int ADChannel { get; set; }

        public abstract float HSSpeedMax { get; set; }

        public abstract float HSSpeedMin { get; set; }

        public abstract float HSSpeed { get; set; }

        public abstract int BitDepth { get; set; }

        public abstract int[] Acquire();

        public abstract uint Acquire(int[] DataBuffer);

        public abstract uint Acquire(int[] DataBuffer, int TrackCenter, int TrackHeight);

        private int _SaturationLevel;
        public virtual int SaturationLevel
        {
            get { return _SaturationLevel; }
            set
            {
                if (value >= Math.Pow(2, BitDepth))
                    throw new ArgumentException("Saturation level may not exceed 2^BitDepth - 1.");
                _SaturationLevel = value;
            }
        }
        public abstract ImageSize Image { get; set; }

        public double[] Calibration { get; set; }

        protected void LoadCalibration(string CalFile)
        {
            Calibration = Enumerable.Range(0, (int)XDim).Select(x => (double)x).ToArray();
        }

        public override void Update(CameraParameters p)
        {
            LoadCalibration(p.CalFile);
            ReadMode = p.ReadMode;
            Image = p.Image;
            SaturationLevel = p.SaturationLevel;
            if (HasIntensifier) Gain = p.InitialGain;
        }

        public abstract void Close();
    }
}





