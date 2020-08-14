using LuiHardware.Object;

namespace LuiHardware.SyringePump
{
    /// <summary>
    /// Base class for all syringe pumps.
    /// </summary>
    public abstract class AbstractPump : LuiObject<SyringePumpParameters>, ISyringePump
    {
        public PumpState CurrentState { get; set; }

        public virtual PumpState Toggle()
        {
            switch (CurrentState)
            {
                case PumpState.Open:
                    SetClosed();
                    break;
                case PumpState.Closed:
                    SetOpen();
                    break;
            }
            return CurrentState;
        }

        public virtual void SetOpen()
        {
            CurrentState = PumpState.Open;
            //TODO Which is which?
        }

        public virtual void SetClosed()
        {
            CurrentState = PumpState.Closed;
        }

        public virtual PumpState GetState()
        {
            return CurrentState;
        }

        public override void Update(SyringePumpParameters p)
        {
            throw new System.NotImplementedException();
        }

    }
}
