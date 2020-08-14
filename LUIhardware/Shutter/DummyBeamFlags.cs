﻿using LuiHardware.Object;

namespace LuiHardware.Shutter
{
    /// <summary>
    /// Dummy beam flags object implemented using no-ops.
    /// </summary>
    public class DummyBeamFlags : AbstractBeamFlags
    {
        public DummyBeamFlags(LuiObjectParameters p) : this() { }

        public DummyBeamFlags()
        {

        }

        //protected override void Dispose(bool disposing)
        //{
        //    // Do nothing.
        //}
    }
}
