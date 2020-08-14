using LuiHardware.Object;
using System;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace LuiHardware.Gpib
{
    /// <summary>
    /// Provide GPIB using Prologix USB GPIB controller.
    /// </summary>
    public class PrologixGpibProvider : AbstractGpibProvider
    {
        #region Constants
        private const string PrologixAddress = "++addr";
        private const string PrologixIFC = "++ifc";
        private const string PrologixMode = "++mode 1";
        private const string PrologixEOI = "++eoi 1";
        private const string PrologixEOS = "++eos 0"; // 0 = CRLF termination
        private const string PrologixRead = "++read";
        #endregion

        SerialPort _port;

        public string PortName
        {
            get
            {
                return _port.PortName;
            }
        }

        private int Timeout { get; set; }
        private const int DefaultTimeout = 500;

        public PrologixGpibProvider(string PortName)
        {
            Init(PortName, DefaultTimeout);
        }

        public PrologixGpibProvider(LuiObjectParameters p) : this(p as GpibProviderParameters) { }

        public PrologixGpibProvider(GpibProviderParameters p)
        {
            if (p == null || p.PortName == null)
                throw new ArgumentException("PortName must be defined.");
            Init(p.PortName, p.Timeout);
        }

        private void Init(string PortName, int Timeout)
        {

            #region Serial port configuration

            _port = new SerialPort(PortName)
            {
                BaudRate = 9600, //1200 ???
                Parity = Parity.None,
                DataBits = 8,
                StopBits = StopBits.One,
                Handshake = Handshake.RequestToSend,
                RtsEnable = true,
                DtrEnable = true,
                Encoding = Encoding.ASCII,
                DiscardNull = false,
                ParityReplace = 0,
                ReadTimeout = Timeout,
                WriteTimeout = Timeout
            };

            #endregion

            // set up prologix controller
            SendTX(PrologixIFC); // Make Prologix Controller-In-Charge
            SendTX(PrologixMode); // Use CONTROLLER mode
            SendTX(PrologixEOI); // Assert EOI after transmit GPIB data.
            SendTX(PrologixEOS); // Use CRLF as GPIB terminator.
            SendTX("++auto 1"); // Enable read after write.
            SendTX("++read_tmo_ms " + Timeout); // Set readout for manual 
            SendTX("++ver");
        }

        void Open()
        {
            _port.Open();
        }

        void Close()
        {
            if (_port.IsOpen) _port.Close();
            Thread.Sleep(Constants.SerialPortCloseDelay); // Prevents subsequent call from interfering with close.
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_port.IsOpen) _port.Close();
                _port.Dispose();
            }
        }

        private void SendTX(string command)
        {
            string TX;
            if (command[0] == (char)43) // send to prologix 
            {
                TX = command + "\r\n";
            }
            else
            {
                TX = EscapeString(command) + "\r\n"; // send to instrument
            }
            try
            {
                if (!_port.IsOpen) _port.Open();
                _port.Write(TX);
            }
            catch (IOException ex)
            {
                Close();
            }
        }

        override public void LoggedWrite(byte address, string command)
        {
            Log.Debug("GPIB Command: " + command);
            try
            {
                if (!_port.IsOpen) _port.Open();
                SendTX(command);
            }
            catch (IOException ex)
            {
                Log.Error(ex);
            }
        }

        override public string LoggedQuery(byte address, string command)
        {
            Log.Debug("GPIB Command: " + command);
            string buffer = null;
            try
            {
                if (!_port.IsOpen) _port.Open();
                SendTX(command);
                // buffer = ReadWithTimeout();
                //buffer = buffer.TrimEnd("\r\n".ToCharArray());
            }
            catch (IOException ex)
            {
                Log.Error(ex);
            }
            Log.Debug("GPIB Response: " + buffer);
            return buffer;
        }

        /// <summary>
        /// This function escapes the GPIB command string with ascii null characters for use with Prologix controller.
        /// CR (13), LF (10), ESC (27), + (43) characters will also be escaped.
        /// </summary>
        /// <param name="s">GPIB command</param>
        /// <returns>Null-escaped string</returns>
        string EscapeString(string s)
        {
            StringBuilder builder = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == (char)10 || s[i] == (char)13 || s[i] == (char)27 || s[i] == (char)43)
                {
                    builder.Append((char)27); // escape
                }
                builder.Append(s[i]);
                builder.Append('\0'); // Hack/workaround for every-other-character problem.
            }
            return builder.ToString();
        }

        public override void Update(GpibProviderParameters p)
        {
            Timeout = p.Timeout;
        }
    }
}