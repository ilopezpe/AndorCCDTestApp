using log4net;
using LuiHardware.Camera;
using LuiHardware.Shutter;
using LuiHardware.SrsDDG;
using LuiHardware.SyringePump;
using System;
using System.Collections.Generic;

namespace LuiHardware
{
    public class Commander
    {
        public ICamera Camera { get; set; }
        public IBeamFlags BeamFlag { get; set; }
        public IDigtalDelayGenerator DDG { get; set; }
        public ISyringePump Pump { get; set; }
        public List<Double> Delays { get; set; }

        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Commander(ICamera camera = null, IBeamFlags beamFlags = null, IDigtalDelayGenerator ddg = null, ISyringePump pump = null)
        {
            // Set dummies instead of null values to save a *ton* of null checks elsewhere.
            //Camera = camera != null ? camera : new DummyCamera();
            BeamFlag = beamFlags != null ? beamFlags : new DummyBeamFlags();
            DDG = ddg != null ? ddg : new DummyDDG();
            Pump = pump != null ? pump : new DummyPump();
        }

        public void SetDelays(string file)
        {
            // read file
            Delays = new List<double>();
        }

        public int[] Collect(int n)
        {
            for (int i = 0; i < n; i++)
            {
                //Camera.CountsFvb();
            }
            return null;
        }

        public int[] Dark()
        {
            BeamFlag.CloseLaserAndProbe();
            return Camera.Acquire();
        }

        public uint Dark(int[] DataBuffer)
        {
            BeamFlag.CloseLaserAndProbe();
            return Camera.Acquire(DataBuffer);
        }

        public int[] Probe()
        {
            BeamFlag.CloseLaserAndProbe();
            BeamFlag.OpenProbe();
            int[] data = Camera.Acquire();
            BeamFlag.CloseLaserAndProbe();
            return data;
        }

        public uint Probe(int[] DataBuffer)
        {
            BeamFlag.CloseLaserAndProbe();
            BeamFlag.OpenProbe();
            uint ret = Camera.Acquire(DataBuffer);
            BeamFlag.CloseLaserAndProbe();
            return ret;
        }

        public int[] Transient()
        {
            BeamFlag.OpenLaserAndProbe();
            int[] data = Camera.Acquire();
            BeamFlag.CloseLaserAndProbe();
            return data;
        }

        public uint Transient(int[] DataBuffer)
        {
            BeamFlag.OpenLaserAndProbe();
            uint ret = Camera.Acquire(DataBuffer);
            BeamFlag.CloseLaserAndProbe();
            return ret;
        }

    }
}
