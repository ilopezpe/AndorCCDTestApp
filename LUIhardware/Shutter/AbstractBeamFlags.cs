namespace LuiHardware.Shutter
{
    /// <summary>
    /// Base class for all beam flag classes.
    /// </summary>
    /// 
    public abstract class AbstractBeamFlags : IBeamFlags
    {
        public BeamFlagPosition ProbeState { get; set; }
        public BeamFlagPosition LaserState { get; set; }

        virtual public BeamFlagPosition ToggleLaser()
        {
            switch (LaserState)
            {
                case BeamFlagPosition.Closed:
                    OpenLaser();
                    break;
                case BeamFlagPosition.Opened:
                    CloseLaser();
                    break;
            }
            return LaserState;
        }

        virtual public BeamFlagPosition ToggleProbe()
        {
            switch (ProbeState)
            {
                case BeamFlagPosition.Closed:
                    OpenProbe();
                    break;
                case BeamFlagPosition.Opened:
                    CloseProbe();
                    break;
            }
            return ProbeState;
        }

        virtual public void OpenLaser()
        {
            LaserState = BeamFlagPosition.Opened;
        }

        virtual public void CloseLaser()
        {
            LaserState = BeamFlagPosition.Closed;
        }

        virtual public void OpenProbe()
        {
            ProbeState = BeamFlagPosition.Opened;
        }

        virtual public void CloseProbe()
        {
            ProbeState = BeamFlagPosition.Closed;
        }

        virtual public void ToggleLaserAndProbe()
        {
            ToggleProbe();
            ToggleLaser();
        }

        virtual public void OpenLaserAndProbe()
        {
            LaserState = BeamFlagPosition.Opened;
            ProbeState = BeamFlagPosition.Opened;
        }

        virtual public void CloseLaserAndProbe()
        {
            LaserState = BeamFlagPosition.Closed;
            ProbeState = BeamFlagPosition.Closed;
        }

        //public override void Update(BeamFlagsParameters p)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
