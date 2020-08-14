using LuiHardware.Object;
using System;
using System.Runtime.Serialization;

namespace LuiHardware.Shutter
{
    [DataContract]
    public class BeamFlagsParameters : LuiObjectParameters<BeamFlagsParameters>
    {
        [DataMember]
        public string PortName { get; set; }

        [DataMember]
        public int Delay { get; set; } = BeamFlags.DefaultDelay;

        public BeamFlagsParameters(Type Type, string PortName)
            : base(Type)
        {
            this.PortName = PortName;
        }

        public BeamFlagsParameters(BeamFlagsParameters other)
            : base(other)
        {

        }

        public BeamFlagsParameters(Type Type)
            : base(Type)
        {

        }

        public BeamFlagsParameters()
            : base()
        {

        }

        public override void Copy(BeamFlagsParameters other)
        {
            base.Copy(other);
            this.PortName = other.PortName;
            this.Delay = other.Delay;
        }

        public override bool NeedsReinstantiation(BeamFlagsParameters other)
        {
            bool needs = base.NeedsReinstantiation(other);
            if (needs) return true;

            if (Type == typeof(BeamFlags) || Type.IsSubclassOf(typeof(BeamFlags)))
            {
                needs |= other.PortName != PortName;
            }
            return needs;
        }

        public override bool NeedsUpdate(BeamFlagsParameters other)
        {
            bool needs = other.Delay != Delay;
            return needs;
        }
    }
}
