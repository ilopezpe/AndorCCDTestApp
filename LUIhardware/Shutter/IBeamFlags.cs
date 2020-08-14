namespace LuiHardware.Shutter
{
    public enum BeamFlagPosition { Opened, Closed }
    /// <summary>
    /// Interface for all beam flag classes
    /// </summary>
    public interface IBeamFlags
    {
        /// <summary>
        /// This holds the current state of the probe beam flag
        /// </summary>
        BeamFlagPosition ProbeState { get; }

        /// <summary>
        /// This holds the current state of the laser beam flag 
        /// </summary>
        BeamFlagPosition LaserState { get; }

        /// <summary>
        /// This commands the probe beam flag to switch position
        /// </summary>
        /// <returns>ProbeState</returns>
        BeamFlagPosition ToggleProbe();

        /// <summary>
        /// This commands the laser beam flag to switch position
        /// </summary>
        /// <returns>LaserState</returns>
        BeamFlagPosition ToggleLaser();

        /// <summary>
        /// This commands both probe and laser beam flags to switch position
        /// </summary>
        void ToggleLaserAndProbe();

        /// <summary>
        /// This commands a new state of the laser flag
        /// </summary>
        void OpenLaser();

        /// <summary>
        /// This commands a new state of the laser beam flag
        /// </summary>
        void CloseLaser();

        /// <summary>
        /// This commands a new state of the probe beam flag
        /// </summary>
        void OpenProbe();

        /// <summary>
        /// This commands a new state of the probe beam flag
        /// </summary>
        void CloseProbe();

        /// <summary>
        /// This opens the laser and probe beam flags simultaneously
        /// </summary>
        void OpenLaserAndProbe();

        /// <summary>
        /// This closes the laser and probe beam flags simultaneously
        /// </summary>
        void CloseLaserAndProbe();
    }
}
