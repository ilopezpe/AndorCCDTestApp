using LuiHardware.Object;
using System;
using System.IO.Ports;
using System.Threading;

namespace LuiHardware.Shutter
{
    /// <summary>
    /// Class representing BeamFlags operated by numato usbgpio16 controller.
    /// </summary>
    public class BeamFlags : AbstractBeamFlags
    {
        #region Constants 
        //use masks to send commands simultaneously
        public const string gpioOutputs = "00ff";
        public const string gpioMask = "c000";

        //probe light shutter
        //reverse logic, i.e. high = close shutters. low = open shutters.
        public const string OpenProbeCommand = "gpio clear E\r";
        public const string CloseProbeCommand = "gpio set E\r";

        //laser shutter
        public const string OpenLaserCommand = "gpio clear F\r";
        public const string CloseLaserCommand = "gpio set F\r";

        // the following allows switching both io14 and io15 simultaneously.
        public const string OpenLaserAndProbeCommand = "gpio writeall 0000\r";
        public const string CloseLaserAndProbeCommand = "gpio writeall ffff\r";
        #endregion

        // Approximate time in ms for solenoid to switch.
        public const int DefaultDelay = 300;

        public int Delay { get; set; } // Time in miliseconds to sleep between commands.

        public string PortName
        {
            get
            {
                return _port.PortName;
            }
        }

        private SerialPort _port;

        public BeamFlags(LuiObjectParameters p) : this(p as BeamFlagsParameters) { }

        public BeamFlags(BeamFlagsParameters p)
        {
            if (p == null || p.PortName == null)
                throw new ArgumentException("PortName must be defined.");
            Init(p.PortName);
        }

        public BeamFlags(String portName)
        {
            Init(portName);
        }

        private void Init(string portName)
        {
            Delay = DefaultDelay;
            _port = new SerialPort(portName)
            {
                BaudRate = 9600
            };
            if (!_port.IsOpen)
                _port.Open();

            _port.DiscardInBuffer();
            _port.Write("gpio iodir" + gpioOutputs + "\r");
            Thread.Sleep(10);
            _port.Write("gpio iomask " + gpioMask + "\r");
            Thread.Sleep(10);
            _port.DiscardOutBuffer();

            CloseLaserAndProbe();
        }

        public override void OpenLaser()
        {
            OpenLaser(true);
        }

        /// <summary>
        /// Opens the laser flag, optionally sleeping to ensure the flag has opened completely.
        /// LaserState is updated only after sleeping in case of monitoring by another thread.
        /// </summary>
        /// <param name="wait"></param>
        private void OpenLaser(bool wait)
        {
            _port.DiscardInBuffer();
            _port.Write(OpenLaserCommand);
            if (wait) Thread.Sleep(Delay);
            LaserState = BeamFlagPosition.Opened;
            _port.DiscardOutBuffer();
        }

        public override void OpenProbe()
        {
            OpenProbe(true);
        }

        private void OpenProbe(bool wait)
        {
            _port.DiscardInBuffer();
            _port.Write(OpenProbeCommand);
            if (wait) Thread.Sleep(Delay);
            ProbeState = BeamFlagPosition.Opened;
            _port.DiscardOutBuffer();
        }

        public override void OpenLaserAndProbe()
        {
            OpenLaserAndProbe(true);
        }

        private void OpenLaserAndProbe(bool wait)
        {
            _port.DiscardInBuffer();
            _port.Write(OpenLaserAndProbeCommand);
            if (wait) Thread.Sleep(Delay);
            LaserState = BeamFlagPosition.Opened;
            ProbeState = BeamFlagPosition.Opened;
            _port.DiscardOutBuffer();
        }

        public override void CloseLaser()
        {
            CloseLaser(true);
        }

        private void CloseLaser(bool wait)
        {
            _port.DiscardInBuffer();
            _port.Write(CloseLaserCommand);
            if (wait) Thread.Sleep(Delay);
            LaserState = BeamFlagPosition.Closed;
            _port.DiscardOutBuffer();
        }

        public override void CloseProbe()
        {
            CloseProbe(true);
        }

        private void CloseProbe(bool wait)
        {
            _port.DiscardInBuffer();
            _port.Write(CloseProbeCommand);
            if (wait) Thread.Sleep(Delay);
            ProbeState = BeamFlagPosition.Closed;
            _port.DiscardOutBuffer();
        }

        public override void CloseLaserAndProbe()
        {
            CloseLaserAndProbe(true);
        }

        private void CloseLaserAndProbe(bool wait)
        {
            _port.DiscardInBuffer();
            _port.Write(CloseLaserAndProbeCommand);
            if (wait) Thread.Sleep(Delay);
            LaserState = BeamFlagPosition.Closed;
            ProbeState = BeamFlagPosition.Closed;
            _port.DiscardOutBuffer();
        }

        private void EnsurePortDisposed()
        {
            if (_port != null)
            {
                if (_port.IsOpen) _port.Close();
                _port.Dispose();
            }
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        CloseLaserAndProbe();
        //        EnsurePortDisposed();
        //    }
        //}

        //public override void Update(BeamFlagsParameters p)
        //{
        //    Delay = p.Delay;
        //}
    }
}
