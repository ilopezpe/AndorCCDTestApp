/// <summary>
///  Interface for a camera. Any abstract or concrete camera class should
///  implement this interface.
/// </summary>

namespace LuiHardware.Camera
{
    /// <summary>
    /// Interface for all camera classes.
    /// Cameras which do not support certain features should return null for 
    /// all relevant properties.
    /// </summary>
    public interface ICamera
    {
        /// <summary>
        /// The serial number of the camera in use. 
        /// </summary>
        int Serial { get; }

        /// <summary>
        /// The number of camera currently connected. 
        /// </summary>
        int numCameras { get; }

        /// <summary>
        /// The number of physical pixels in horizontal dimension of the detector. 
        /// </summary>
        int XDim { get; }

        /// <summary>
        /// The number of physical pixels in the vertical dimension of the detector.
        /// </summary>
        int YDim { get; }

        /// <summary>
        /// The number of rows of an image given current camera settings.
        /// </summary>
        int GetTrackHeight { get; }

        /// <summary>
        /// This sets the acquistion mode.
        /// <list type="table">
        /// <item>1 Single Scan</item>
        /// <item>2 Accumulation</item>
        /// <item>3 Kinetics</item>
        /// <item>4 Fast Kinetics</item>
        /// <item>5 Acquires continuously until aborted</item>
        /// </list>
        /// </summary>
        int AcquisitionMode { get; set; }

        /// <summary>
        /// This viariable will set the trigger mode of the camera
        /// <list type="table">
        /// <item>0 Internal</item>
        /// <item>1 External</item>
        /// <item>6 External Start</item>
        /// <item>7 External Exposure</item>
        /// <item>8 External FVB EM</item>
        /// <item>10 Software Trigger</item>
        /// </list>
        /// </summary>
        int TriggerMode { get; set; }

        /// <summary>
        /// This variable will set the trigger mode of the internal delay generator 
        /// <list type="table">
        /// <item>0 internal</item>
        /// <item>1 external</item>
        /// </list>
        /// </summary>
        int DDGTriggerMode { get; set; }

        /// <summary>
        ///  This variable sets the photocathode gating mode.
        /// <list type="table">
        /// <item>0 Fire ANDed with the Gate input.</item>
        /// <item>1 Gating controlled from Fire pulse only.</item>
        /// <item>2 Gating controlled from SMB Gate input only.</item>
        /// <item>3 Gating ON continuously.</item>
        /// <item>4 Gating OFF continuously.</item>
        /// <item>5 Gate using DDG (iStar only).</item>
        /// </list>
        /// </summary>
        int GateMode { get; set; }

        /// <summary>
        /// This variable will set the readout mode
        /// <list type="table">
        /// <item>0 Full vertical binning</item>
        /// <item>1 Multi-track</item>
        /// <item>2 Random-track</item>
        /// <item>3 Single-track</item>
        /// <item>4 Image</item>
        /// </list>
        /// </summary>
        int ReadMode { get; set; }

        /// <summary>
        /// True if ccd possesses an image intensifier, i.e. ICCD.
        /// </summary>
        bool HasIntensifier { get; }

        /// <summary>
        ///  This variable sets the MCP gating mode
        /// <list type="table">
        /// <item>0 Off</item>
        /// <item>1 On</item>
        /// </list>
        /// </summary>
        int Gating { get; set; }

        /// <summary>
        /// This variable stores the highest value for the MCP Gain
        /// </summary>
        int MCPGainMax { get; }

        /// <summary>
        /// This variable stores the lowest value for the MCP Gain
        /// </summary>
        int MCPGainMin { get; }

        /// <summary>
        /// This variable stores the value for the MCP Gain
        /// </summary>
        int Gain { get; set; }

        ///<summary>
        /// This variable stores the size in bits of the available dynamic range
        ///</summary>
        int BitDepth { get; }

        ///<summary>
        /// Number of A-D Channels available
        ///</summary>
        int NumberADChannels { get; }

        ///<summary>
        /// This variable stores the current A-D Channel
        ///</summary>
        int ADChannel { get; }

        /// <summary>
        /// This variable stores the lowest value for the horizontal readout sped 
        /// Used for imaging
        /// </summary>
        float HSSpeedMax { get; }

        /// <summary>
        /// This variable stores the highest value for the horizontal readout speed
        /// Used for spectroscopy
        /// </summary>
        float HSSpeedMin { get; }

        /// <summary>
        /// This variable stores the value for the horizontal readoutspeed
        /// </summary>
        float HSSpeed { get; set; }

        /// <summary>
        /// Wavelength values along camera's horizontal access.
        /// TODO: Probably should be in Spectrograph API.
        /// </summary>
        double[] Calibration { get; set; }

        /// <summary>
        /// Specifies current image capture area along with any hardware binning.
        /// </summary>
        ImageSize Image { get; set; }

        /// <summary>
        /// Acquire with FVB using current settings.
        /// </summary>
        /// <returns>New data array.</returns>
        int[] Acquire();

        /// <summary>
        /// Acquire with FVB using current settings.
        /// </summary>
        /// <param name="DataBuffer">Existing data array to repopulate.</param>
        /// <returns>Camera status code.</returns>
        uint Acquire(int[] DataBuffer);

        /// <summary>
        /// Acquire data with current settings.
        /// </summary>
        /// <param name="data">Existing data array to repopulate</param>
        /// <param name="TrackCenter">Track center</param>
        /// <param name="TrackHeight">Track height</param>
        /// <returns>Camera status code</returns>
        uint Acquire(int[] data, int TrackCenter, int TrackHeight);

        void Close();
    }
}
