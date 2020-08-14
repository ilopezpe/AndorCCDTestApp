using System;

namespace LuiHardware.Object
{
    public class LuiObjectParametersEventArgs : EventArgs
    {
        public LuiObjectParameters Argument { get; set; }
        public LuiObjectParametersEventArgs(LuiObjectParameters p)
            : base()
        {
            Argument = p;
        }
    }
}
