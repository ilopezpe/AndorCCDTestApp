using LuiHardware.Gpib;
using LuiHardware.Object;
using System;
using System.Runtime.Serialization;

namespace LuiHardware.SrsDDG
{
    /// <summary>
    /// Stores parameters for instantiation of a DDG and provides
    /// fpr their serialization to XML.
    /// </summary>
    [DataContract]
    public class DDGParameters : LuiObjectParameters<DDGParameters>
    {
        [DataMember]
        public byte GpibAddress { get; set; }

        [DataMember]
        public GpibProviderParameters GpibProvider { get; set; }

        public override LuiObjectParameters[] Dependencies
        {
            get
            {
                if (GpibProvider != null)
                    return new LuiObjectParameters[] { GpibProvider };
                return new LuiObjectParameters[0];
            }
        }

        public DDGParameters(Type Type)
            : base(Type)
        {

        }

        public DDGParameters()
            : base()
        {

        }

        public DDGParameters(DDGParameters other)
            : base(other)
        {

        }

        public override void Copy(DDGParameters other)
        {
            base.Copy(other);
            this.GpibAddress = other.GpibAddress;
            this.GpibProvider = other.GpibProvider;
        }

        public override bool NeedsReinstantiation(DDGParameters other)
        {
            bool needs = base.NeedsReinstantiation(other);
            if (needs) return true;

            if (Type == typeof(SrsDDG) || Type.IsSubclassOf(typeof(SrsDDG)))
            {
                needs |= (GpibProvider != other.GpibProvider || (GpibProvider != null && !GpibProvider.Equals(other.GpibProvider)));
            }
            return needs;
        }

        public override bool NeedsUpdate(DDGParameters other)
        {
            bool needs = false;

            if (Type == typeof(SrsDDG) || Type.IsSubclassOf(typeof(SrsDDG)))
            {
                needs |= other.GpibAddress != GpibAddress;
            }

            return needs;
        }
    }
}
