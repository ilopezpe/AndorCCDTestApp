using log4net;
using LuiHardware.Camera;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace AndorCCDTestApp
{
    public partial class AndorCCDTestApp : Form
    {
        public bool cameraConnected = false;
        public int MCPGain;

        public List<int[]> ListOfArrays { get; set; }

        public enum Dialog
        {
            INITIALIZE, PROGRESS, TEMPERATURE
        }

        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler TaskStarted;
        public event EventHandler TaskFinished;
        private readonly ManualResetEvent Paused;

        protected BackgroundWorker worker;
        CancellationTokenSource TemperatureCts = null;

        readonly ICamera andor;
        private P CameraAs<P>() where P : class => andor as P;

        #region Object definitions
        struct WorkArgs
        {
            public string ReadMode { get; set; }
            public int Accumulations { get; set; }
            public int TrackCenter { get; set; }
            public int TrackHeight { get; set; }
            public WorkArgs(string ReadMode, int Accumulations, int TrackCenter, int TrackHeight) : this()
            {
                this.ReadMode = ReadMode;
                this.Accumulations = Accumulations;
                this.TrackCenter = TrackCenter;
                this.TrackHeight = TrackHeight;
            }
        }

        struct ProgressObject
        {
            public ProgressObject(double[] Data, Dialog Status)
            {
                this.Data = Data;
                this.Status = Status;
            }
            public readonly double[] Data;
            public readonly Dialog Status;
        }
        #endregion

        public AndorCCDTestApp()
        {
            Paused = new ManualResetEvent(true);

            // Initialize parameters here
            var cp = new CameraParameters(typeof(AndorTempControlled))
            {
                Name = "Camera",
                Dir = ".",
                InitialGain = 10,
                Temperature = 20,
                ReadMode = AndorCamera.ReadModeFVB
            };
            andor = new AndorTempControlled(cp);

            InitializeComponent();
        }

        private void AndorCCDTestApp_Load(object sender, EventArgs e)
        {
            // Initialize Window
            buttonAbort.Enabled = false;
            buttonPause.Enabled = false;
            buttonSave.Enabled = false;

            LogTextBox.Text = "Initializing...\r\n";

            if (andor.numCameras == 1)
            {
                cameraConnected = true;
                groupBoxReadMode.Enabled = cameraConnected;
                groupBoxSettings.Enabled = cameraConnected;
                groupBoxTemp.Enabled = cameraConnected;
                LogTextBox.AppendText("ANDOR: Camera connected. \r\n");
                LogTextBox.AppendText("ANDOR: SN " + andor.Serial.ToString() + "\r\n");
                cameraConnected = true;
                InitWindow();
                LogTextBox.Text += "Ready.\r\n";
            }
            if (andor.numCameras == 0)
            {
                cameraConnected = false;
                groupBoxReadMode.Enabled = cameraConnected;
                groupBoxSettings.Enabled = cameraConnected;
                groupBoxTemp.Enabled = cameraConnected;
                LogTextBox.Text += "ANDOR: NO camera connected. \r\n";
            }
        }

        private void InitWindow()
        {
            UpdateTemperatureLabel();

            numericTemp.Minimum = CameraAs<AndorTempControlled>().TempMin;
            numericTemp.Maximum = 50;
            numericTemp.Value = Properties.Settings.Default.Temperature;

            numericGain.Minimum = 0;
            numericGain.Maximum = andor.MCPGainMax;
            numericGain.Value = andor.Gain;

            numericAccumulations.Value = Properties.Settings.Default.Accumulations;

            numericYcenter.Minimum = 1;
            numericYcenter.Maximum = andor.YDim;
            numericYheight.Minimum = 1;
            numericYheight.Maximum = andor.YDim;

            if (andor.YDim % 2 == 0)
            {
                numericYcenter.Value = andor.YDim / 2;
            }
            else
            {
                numericYcenter.Value = (andor.YDim + 1) / 2;
            }
            numericYheight.Value = andor.YDim;

            comboBoxReadMode.Items.AddRange(new object[] { "FVB", "Single-Track" });
            comboBoxReadMode.SelectedIndex = 0; // default FVB

            comboBoxHSSpeed.Items.AddRange(new object[] { andor.HSSpeedMin + " MHz", (int)andor.HSSpeedMax + " MHz" });
            comboBoxHSSpeed.SelectedIndex = 0;

            UpdateChart();

            numericGain.Enabled = cameraConnected;
            numericAccumulations.Enabled = cameraConnected;
        }

        private void UpdateTemperatureLabel()
        {
            var cparams = CameraAs<AndorTempControlled>();
            if (cparams != null)
            {
                labelTemperature.Text = cparams.Temperature.ToString();
            }
        }
        public void UpdateChart()
        {
            chart1.Series.Clear();
            var series1 = new Series
            {
                Name = "Series1",
                Color = Color.Blue,
                IsVisibleInLegend = false,
                IsXValueIndexed = true,
                ChartType = SeriesChartType.Line,
            };

            chart1.Series.Add(series1);

            chart1.ChartAreas[0].AxisY.Title = "Counts";
            chart1.ChartAreas[0].AxisX.Title = "Pixel";
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.Maximum = andor.XDim;

            chart1.Invalidate();
        }

        private void DoTempCheck(Func<bool> Breakout) // need to develop further for async 
        {
            if (andor is AndorTempControlled)
            {
                AndorTempControlled camct = CameraAs<AndorTempControlled>();
                if (camct.TemperatureStatus != AndorTempControlled.Temp_Stabilized)
                {
                    bool equil = (bool)Invoke(new Func<bool>(TemperatureStabilizedDialog));
                    if (equil)
                    {
                        if (camct.StabilizeUntil(Breakout)) return;
                    }
                }
            }
        }

        private bool TemperatureStabilizedDialog()
        {
            DialogResult result = MessageBox.Show("Camera temperature has not stabilized. Continue?", "Error", MessageBoxButtons.YesNo);
            if (result == DialogResult.No)
            {
                worker.CancelAsync();
            }

            return false;
        }

        private void comboBoxReadMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxReadMode.SelectedIndex == 0)
            {
                andor.ReadMode = AndorCamera.ReadModeFVB;
                LogTextBox.AppendText("ANDOR: ReadMode FVB\r\n");
                numericYcenter.Enabled = false;
                numericYheight.Enabled = false;
            }
            else
            {
                andor.ReadMode = AndorCamera.ReadModeSingleTrack;
                LogTextBox.AppendText("ANDOR: ReadMode Single-Track\r\n");
                numericYcenter.Enabled = true;
                numericYheight.Enabled = true;
            }
        }

        private async void buttonSetTemp_Click(object sender, EventArgs e)
        {
            OnTaskStarted(e);
            LogTextBox.AppendText("ANDOR: Temp set " + numericTemp.Value.ToString() + " deg\r\n");
            await UpdateTemperature();
            OnTaskFinished(e);
        }

        private async Task UpdateTemperature()
        {
            AndorTempControlled camct = CameraAs<AndorTempControlled>();
            if (camct != null)
            {
                if (TemperatureCts != null) TemperatureCts.Cancel();
                TemperatureCts = new CancellationTokenSource();

                if (numericTemp.Value > camct.Temperature) { labelTemperature.Text = "Warming"; }
                if (numericTemp.Value < camct.Temperature) { labelTemperature.Text = "Cooling"; }
                labelTemperature.BackColor = Color.Red;

                await camct.StabilizeTemperatureAsync((int)numericTemp.Value, TemperatureCts.Token); // Wait for 3 deg threshold

                LogTextBox.AppendText("ANDOR: Temp stabilizing...\r\n");
                labelTemperature.BackColor = Color.Gold;
                if (numericTemp.Value > camct.Temperature) { labelTemperature.Text = "Warming"; }
                if (numericTemp.Value < camct.Temperature) { labelTemperature.Text = "Cooling"; }

                await camct.StabilizeTemperatureAsync(TemperatureCts.Token); // Wait for driver signal
                LogTextBox.AppendText("Ready.\r\n");
                labelTemperature.BackColor = Color.RoyalBlue;
                UpdateTemperatureLabel();
                TemperatureCts = null;
            }
        }

        protected bool PauseCancelProgress(DoWorkEventArgs e, int percentProgress, object progress)
        {
            if (CancelCheck(e)) return true; // If cancelling, set e.Cancel and return true.
            worker.ReportProgress(percentProgress, progress);
            if (WillPause())
            {
                // Going to pause.
                WaitForResume();
                worker.ReportProgress(percentProgress, progress);
            }
            return false;
        }

        protected bool CancelCheck(DoWorkEventArgs e)
        {
            if (worker.CancellationPending)
            {
                e.Cancel = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// If Paused is not set, Waits until Paused is set. Returns true if waiting occurred.
        /// </summary>
        /// <returns></returns>
        protected bool WaitForResume()
        {
            return !Paused.WaitOne(Timeout.Infinite);
        }

        protected bool WaitForResume(int timeout)
        {
            return !Paused.WaitOne(timeout);
        }

        protected bool WillPause()
        {
            return !Paused.WaitOne(0);
        }

        private void buttonShutdown_Click(object sender, EventArgs e)
        {
           Dispose();
        }

        private void buttonSaveSettings_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.Accumulations = (int)numericAccumulations.Value;
            Properties.Settings.Default.Temperature = (int)numericTemp.Value;
            Properties.Settings.Default.Save();
            LogTextBox.AppendText("SETTINGS: saved\r\n");
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            FileIO.SaveData(ListOfArrays);
            LogTextBox.AppendText("FILE: saved\r\n");
        }

        private void SetUpWorker()
        {
            worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(DoWork);
            worker.ProgressChanged += new ProgressChangedEventHandler(WorkProgress);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(WorkComplete);
            worker.WorkerSupportsCancellation = true; // update
            worker.WorkerReportsProgress = true; // update
        }

        private void ButtonAcquire_Click(object sender, EventArgs e)
        {
            UpdateChart();
            SetUpWorker();

            var args = new WorkArgs()
            {
                ReadMode = (string)comboBoxReadMode.SelectedItem,
                Accumulations = (int)numericAccumulations.Value,
                TrackCenter = (int)numericYcenter.Value,
                TrackHeight = (int)numericYheight.Value
            };
            worker.RunWorkerAsync(args);
            OnTaskStarted(EventArgs.Empty);
        }

        private void buttonPause_Click(object sender, EventArgs e)
        {
            if (Paused.WaitOne(0)) // True if set (running/resumed).
            {
                Paused.Reset(); // Signal pause.
                buttonPause.Text = "Resume";
            }
            else
            {
                Paused.Set(); // Signal resume.
                buttonPause.Text = "Pause";
            }
        }

        private void buttonAbort_Click(object sender, EventArgs e)
        {
            worker.CancelAsync();
        }

        /// <summary>
        /// Readout of pixel counts based on camera settings
        /// Updates graph with output
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            DoTempCheck(() => PauseCancelProgress(e, 0, new ProgressObject(null, Dialog.TEMPERATURE)));

            if (PauseCancelProgress(e, 0, new ProgressObject(null, Dialog.INITIALIZE)))
            {
                return;
            }

            // set up acquisition
            var args = (WorkArgs)e.Argument; // pass number of accumulations and Accumulations

            // create pixel sequence
            int[] pixel = new int[andor.XDim];
            for (int i = 0; i < andor.XDim; i++)
            {
                pixel[i] = i + 1;
            }
            List<int[]> DataArray = new List<int[]>();

            // initialize arrays 
            int[] DataBuffer = new int[andor.XDim];
            int[] DataSum = new int[andor.XDim];

            for (int i = 0; i < args.Accumulations; i++)
            {
                Array.Clear(DataBuffer, 0, DataBuffer.Length);

                if (args.ReadMode == "FVB")
                {
                    andor.Acquire(DataBuffer);
                }
                else
                {
                    andor.Acquire(DataBuffer, args.TrackCenter, args.TrackHeight);
                }
                MathOps.MatrixSum(DataSum, DataBuffer); // what is column sum????
                DataArray.Add(DataSum);
                if (PauseCancelProgress(e, i, new ProgressObject(null, Dialog.PROGRESS))) return;
            }
            e.Result = DataArray;
        }
        private void WorkProgress(object sender, ProgressChangedEventArgs e)
        {
            var progress = (ProgressObject)e.UserState;
            var progressValue = (e.ProgressPercentage + 1).ToString();
            switch (progress.Status)
            {
                case Dialog.INITIALIZE:
                    textBoxProgressLabel.Text = "Initializing";
                    break;
                case Dialog.PROGRESS:
                    textBoxProgressLabel.Text = "Acquiring";
                    textBoxProgress.Text = progressValue + "/" + numericAccumulations.Value.ToString();
                    break;
                case Dialog.TEMPERATURE:
                    textBoxProgressLabel.Text = "Check Temp";
                    break;
            }
        }

        private void WorkComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                // Handle the exception thrown in the worker thread.
                MessageBox.Show(e.Error.ToString());
            }
            else if (e.Cancelled)
            {
                textBoxProgressLabel.Text = "Aborted";
            }
            else
            {
                textBoxProgressLabel.Text = "Success";
                ListOfArrays = (List<int[]>)e.Result;
                chart1.Series[0].Points.DataBindY(ListOfArrays[0]);
            }
            OnTaskFinished(EventArgs.Empty);
        }

        public bool IsBusy
        {
            get
            {
                return worker != null ? worker.IsBusy : false;
            }
        }

        public void OnTaskStarted(EventArgs args)
        {
            groupBoxReadMode.Enabled = false;
            groupBoxSettings.Enabled = false;
            groupBoxTemp.Enabled = false;
            buttonAbort.Enabled = true;
            buttonPause.Enabled = true;
            buttonSave.Enabled = false;

            Paused.Set(); // set running/resumed
            TaskStarted?.Invoke(this, args);
        }

        public void OnTaskFinished(EventArgs args)
        {
            groupBoxReadMode.Enabled = true;
            groupBoxSettings.Enabled = true;
            groupBoxTemp.Enabled = true;
            buttonAbort.Enabled = false;
            buttonPause.Enabled = false;
            buttonSave.Enabled = true;
            TaskFinished?.Invoke(this, args);
        }
    }
}
