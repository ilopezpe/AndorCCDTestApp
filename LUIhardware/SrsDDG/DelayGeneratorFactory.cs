using System;

namespace LuiHardware.SrsDDG
{
    /// <summary>
    /// Instantiate concrete DDG objects from parameters.
    /// </summary>
    public class DelayGeneratorFactory
    {
        public static IDigtalDelayGenerator CreateDelayGenerator(DDGParameters p)
        {
            return (IDigtalDelayGenerator)Activator.CreateInstance(p.Type, p);
        }
    }
}
