using log4net;
using LuiHardware.Gpib;
using LuiHardware.Object;
using System.Linq;

namespace LuiHardware.SrsDDG
{
    /// <summary>
    /// Base class for Stanford Instruments DDGs.
    /// </summary>
    public abstract class SrsDDG : AbstractDDG
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IGpibProvider GPIBProvider { get; set; }
        public byte GPIBAddress { get; set; }

        public SrsDDG(LuiObjectParameters p, params ILuiObject[] dependencies) :
            this(p as DDGParameters, dependencies)
        { } //TODO just take IGpibProvider instead of params array.

        public SrsDDG(DDGParameters p, params ILuiObject[] dependencies)
        {
            Init(p.GpibAddress, dependencies);
        }

        private void Init(byte _GpibAddress, params ILuiObject[] dependencies)
        {
            GPIBProvider = (IGpibProvider)dependencies.First(d => d is IGpibProvider);
            GPIBAddress = _GpibAddress;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Nothing to dispose of.
            }
        }

        public override void Update(DDGParameters p)
        {
            GPIBAddress = p.GpibAddress;
        }

    }
}
