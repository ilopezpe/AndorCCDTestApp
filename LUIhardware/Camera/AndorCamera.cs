using ATMCD32CS; // copy dlls to output folder
using LuiHardware.Object;
using System;

namespace LuiHardware.Camera
{
    /// <summary>
    /// Class representing a generic Andor camera.
    /// Specific Andor camera types should inherit from this class.
    /// </summary>
    public class AndorCamera : AbstractCamera
    {
        public const int ReadModeFVB = 0;
        public const int ReadModeMultiTrack = 1;
        public const int ReadModeRandomTrack = 2;
        public const int ReadModeSingleTrack = 3;
        public const int ReadModeFullImage = 4;

        public const int AcquisitionModeSingle = 1;
        public const int AcquisitionModeAccumulate = 2;

        public int defaultGain = 10;

        public override int ReadMode { get; set; } = 0;
        public override int AcquisitionMode { get; set; } = AcquisitionModeSingle;

        public override int Serial { get; set; }
        public override int numCameras { get; set; }

        public uint ret;
        public AndorSDK sdk = new AndorSDK();   // Initialize Andor SDK
        public AndorSDK.AndorCapabilities caps; // Initialize capabilities

        public override int XDim { get; set; } = 1024;
        public override int YDim { get; set; } = 255;
        public override int TriggerMode { get; set; } = 7;
        public override int DDGTriggerMode { get; set; } = 1;
        public override bool HasIntensifier { get; } = true;
        public override int GateMode { get; set; } = 2;
        public override int Gating { get; set; } = 1;
        public override int MCPGainMin { get; set; } = 1;
        public override int MCPGainMax { get; set; } = 4095;
        public override int Gain { get; set; } = 10;

        public override int NumberADChannels { get; set; }
        public override int ADChannel { get; set; } = 0; // there is only one AD channel available

        public override float HSSpeedMin { get; set; }
        public override float HSSpeedMax { get; set; }
        public override float HSSpeed { get; set; }

        public override int BitDepth { get; set; }
        private int _SaturationLevel;
        public override int SaturationLevel
        {
            get { return _SaturationLevel; }
            set
            {
                if (value >= Math.Pow(2, BitDepth))
                    throw new ArgumentException("Saturation level may not exceed 2^BitDepth - 1.");
                _SaturationLevel = value;
            }
        }

        public override ImageSize Image { get; set; }

        public AndorCamera() { }

        public AndorCamera(LuiObjectParameters p) : this(p as CameraParameters) { }

        public AndorCamera(CameraParameters p)
        {

            int _numCameras = 0;
            ret = sdk.GetAvailableCameras(ref _numCameras);
            if (ret == AndorSDK.DRV_SUCCESS && _numCameras == 1)
            {
                numCameras = _numCameras;
                int handle = 0;
                ret = sdk.GetCameraHandle(0, ref handle);
                if (ret == AndorSDK.DRV_SUCCESS)
                {
                    ret = sdk.SetCurrentCamera(handle);
                    if (ret == AndorSDK.DRV_SUCCESS)
                    {
                        ret = sdk.Initialize(p.Dir);
                        if (ret != AndorSDK.DRV_SUCCESS)
                        {
                            Log.Debug("ANDOR: " + ErrorCodes.Decoder(ret));
                            sdk.ShutDown();
                        }
                        else
                        {
                            int _Serial = 0;
                            sdk.GetCameraSerialNumber(ref _Serial);
                            Serial = _Serial;

                            // get properties
                            ret = sdk.GetCapabilities(ref caps);
                            if (ret == AndorSDK.DRV_SUCCESS && (AndorSDK.AC_FEATURES_FTEXTERNALEXPOSURE & caps.ulTriggerModes) != 1)
                            {
                                Log.Debug("ANDOR: " + "Does not support external exposure");
                            }

                            int nAD = 0;
                            sdk.GetNumberADChannels(ref nAD);
                            NumberADChannels = nAD; // number of ADCs 

                            int _BitDepth = 0;
                            sdk.GetBitDepth(_BitDepth, ref _BitDepth); // dynamic range
                            BitDepth = _BitDepth;

                            int _XDim = 0, _YDim = 0;
                            sdk.GetDetector(ref _XDim, ref _YDim); // detector dimensions
                            XDim = _XDim; YDim = _YDim;

                            int _MCPGainMin = 0; int _MCPGainMax = 0;
                            sdk.GetMCPGainRange(ref _MCPGainMin, ref _MCPGainMax); // gain settings
                            MCPGainMin = _MCPGainMin; MCPGainMax = _MCPGainMax;

                            // set properties
                            SetUpAndor();
                        }
                    }
                }
            }
        }


        // This function sets up the camera 
        public void SetUpAndor()
        {
            sdk.SetADChannel(ADChannel);

            int _NumberHSSpeeds = 0;
            sdk.GetNumberHSSpeeds(ADChannel, 0, ref _NumberHSSpeeds);

            float _HSSpeedMax = 0;
            sdk.GetHSSpeed(ADChannel, 0, 0, ref _HSSpeedMax);
            HSSpeedMax = _HSSpeedMax;

            float _HSSpeedMin = 0;
            sdk.GetHSSpeed(ADChannel, 0, _NumberHSSpeeds - 1, ref _HSSpeedMin);
            HSSpeedMin = _HSSpeedMin;

            sdk.SetHighCapacity(0); // Note: 1 disables high sensitivity. 0 enables high sensitivity

            // sdk.SetImage(hbin, vbin, hstart, hend,vstart,vend)
            sdk.SetImage(1, 1, 1, XDim, 1, YDim); // Use full sensor area

            sdk.SetAcquisitionMode(AcquisitionMode);
            sdk.SetReadMode(ReadMode);

            // These functions control the camera's mode of operation
            sdk.SetMCPGating(Gating);
            sdk.SetTriggerMode(TriggerMode);
            sdk.SetDDGTriggerMode(DDGTriggerMode);

            sdk.SetGateMode(GateMode);

            sdk.SetMCPGain(defaultGain); // This functions controls the voltage of the intensifier mcp

            sdk.SetHSSpeed(0, 0);

            int vsspeed_i = 0;
            float vsspeed = 0;
            sdk.GetFastestRecommendedVSSpeed(ref vsspeed_i, ref vsspeed);
            sdk.SetVSSpeed(vsspeed_i);
        }

        /// <summary>
        /// Number of rows in acquisition data given current camera settings.
        /// </summary>
        public override int GetTrackHeight
        {
            get
            {
                if (ReadMode == ReadModeFVB)
                {
                    return YDim;
                }
                else if (ReadMode == ReadModeSingleTrack)
                {
                    return 1; //Image.Height;
                }
                else
                {
                    throw new NotImplementedException("Unsupported read mode.");
                }
            }
        }

        public uint SetMCPGain(int gain)
        {
            uint ret = sdk.SetMCPGain(gain);
            if (ret != AndorSDK.DRV_SUCCESS) return ret;
            return 0;
        }

        public override int[] Acquire()
        {
            uint npx = (uint)XDim;
            int[] data = new int[npx];
            uint ret = Acquire(data);
            if (ret != AndorSDK.DRV_SUCCESS) { }
            return data;
        }

        public override uint Acquire(int[] data)
        {
            uint ret;
            uint npx = (uint)data.Length;

            sdk.SetReadMode(ReadModeFVB);
            sdk.StartAcquisition();
            sdk.WaitForAcquisition();
            ret = sdk.GetAcquiredData(data, npx);
            if (ret != AndorSDK.DRV_SUCCESS) { return ret; }

            //Log.Debug("Camera returned " + DecodeStatus(ret));
            //ThrowIfSaturated(data);
            return ret;
        }

        public override uint Acquire(int[] data, int TrackCenter, int TrackHeight)
        {
            uint ret;
            uint npx = (uint)data.Length;

            sdk.SetReadMode(ReadModeSingleTrack); //single-track ReadMode
            ret = sdk.SetSingleTrack(TrackCenter, TrackHeight);
            if (ret != AndorSDK.DRV_SUCCESS) { return ret; }
            sdk.StartAcquisition();
            sdk.WaitForAcquisition();
            ret = sdk.GetAcquiredData(data, npx);
            if (ret != AndorSDK.DRV_SUCCESS) { return ret; }
            return ret;
        }

        protected void ThrowIfSaturated(int[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] >= SaturationLevel)
                {
                    var ex = new InvalidOperationException("Sensor saturation detected.");
                    ex.Data["Pixel"] = i;
                    ex.Data["Value"] = data[i];
                    throw ex;
                }
            }
        }
        public override void Close()
        {
            if (sdk != null) sdk.ShutDown();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

    }
}




