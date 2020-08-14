using LuiHardware.Object;
using System;
using System.Runtime.Serialization;

namespace LuiHardware.SyringePump
{
    public class SyringePumpParameters : LuiObjectParameters<SyringePumpParameters>
    {

        [DataMember]
        public string PortName { get; set; }

        public SyringePumpParameters(Type Type, string PortName) : base(Type)
        {
            this.PortName = PortName;
        }

        public SyringePumpParameters(Type Type)
            : base(Type)
        {

        }

        public SyringePumpParameters()
            : base()
        {

        }

        public SyringePumpParameters(SyringePumpParameters other)
            : base(other)
        {

        }

        public override void Copy(SyringePumpParameters other)
        {
            base.Copy(other);
            this.PortName = other.PortName;
        }

        public override bool NeedsReinstantiation(SyringePumpParameters other)
        {
            bool needs = base.NeedsReinstantiation(other);
            if (needs) return true;

            if (Type == typeof(HarvardPump) || Type.IsSubclassOf(typeof(HarvardPump)))
            {
                needs |= other.PortName != PortName;
            }
            return needs;
        }

        public override bool NeedsUpdate(SyringePumpParameters other)
        {
            return false;
        }
    }
}
