using ATMCD32CS;
using LuiHardware.Object;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Andor Camera with temperature control
/// </summary>
namespace LuiHardware.Camera
{
    public class AndorTempControlled : AndorCamera
    {
        public int currentTemp;
        public const int shutdownTemp = 5;

        public const uint Temp_Stabilized = AndorSDK.DRV_TEMP_STABILIZED;
        public const uint Temp_NOT_Stabilized = AndorSDK.DRV_TEMP_NOT_STABILIZED;

        public int TempMin { get; }
        public int TempMax { get; }

        public AndorTempControlled() { }

        public AndorTempControlled(LuiObjectParameters p) : this(p as CameraParameters) { }

        public AndorTempControlled(CameraParameters p) : base(p)
        {
            if (numCameras == 1)
            {
                int _TempMin = 0, _TempMax = 0;
                sdk.GetTemperatureRange(ref _TempMin, ref _TempMax);
                TempMin = _TempMin; TempMax = _TempMax;
                sdk.CoolerON();
                StabilizeTemperature(p.Temperature);
            }
        }
        public override void Update(CameraParameters p)
        {
            base.Update(p);
            StabilizeTemperature(p.Temperature);
        }

        public virtual int Temperature
        {
            get
            {
                int currentTemp = 0;
                sdk.GetTemperature(ref currentTemp);
                return currentTemp;
            }
        }

        public virtual uint TemperatureStatus
        {
            get
            {
                int currentTemperature = 0;
                return sdk.GetTemperature(ref currentTemperature);
            }
        }

        public virtual void StabilizeTemperature(int targetTemperature, CancellationToken? token = null)
        {

            if (targetTemperature < TempMin || targetTemperature > TempMax)
            {
                throw new ArgumentException("Temperature out of range.");
            }
            sdk.SetTemperature(targetTemperature);
            float currentTemperature = 0;
            sdk.GetTemperatureF(ref currentTemperature);
            while (Math.Abs(currentTemperature - targetTemperature) > 3F)
            {
                if (token.HasValue && token.Value.IsCancellationRequested) break;
                sdk.GetTemperatureF(ref currentTemperature);
            }
        }

        public virtual void StabilizeTemperature(CancellationToken? token = null)
        {
            int currentTemperature = 0;
            uint status = AndorSDK.DRV_TEMP_NOT_STABILIZED;
            while (status != AndorSDK.DRV_TEMP_STABILIZED)
            {
                if (token.HasValue && token.Value.IsCancellationRequested) break;
                status = sdk.GetTemperature(ref currentTemperature);
            }
        }

        public async Task StabilizeTemperatureAsync(int targetTemperature)
        {
            await Task.Run(() => StabilizeTemperature(targetTemperature));
        }

        public async Task StabilizeTemperatureAsync(int targetTemperature, CancellationToken token)
        {
            await Task.Run(() => StabilizeTemperature(targetTemperature, token));
        }

        public async Task StabilizeTemperatureAsync()
        {
            await Task.Run(() => StabilizeTemperature());
        }

        public async Task StabilizeTemperatureAsync(CancellationToken token)
        {
            await Task.Run(() => StabilizeTemperature(token));
        }

        public bool StabilizeUntil(Func<bool> BreakoutCondition)
        {
            return StabilizeUntil(BreakoutCondition, 200);
        }

        public virtual bool StabilizeUntil(Func<bool> BreakoutCondition, int PollDelayMs)
        {
            int currentTemperature = 0;
            uint status = AndorSDK.DRV_TEMP_NOT_STABILIZED;
            while (status != AndorSDK.DRV_TEMP_STABILIZED)
            {
                status = sdk.GetTemperature(ref currentTemperature);
                if (BreakoutCondition()) return true;
                Thread.Sleep(PollDelayMs);
            }
            return false;
        }

        public virtual void WarmToTemperature(int thresholdTemperature)
        {
            float currentTemperature = 0;
            sdk.GetTemperatureF(ref currentTemperature);
            while (currentTemperature < (thresholdTemperature - 3F))
            {
                sdk.GetTemperatureF(ref currentTemperature);
            }
        }

        public override void Close()
        {
            if (sdk != null)
            {
                sdk.CoolerOFF();
                WarmToTemperature(shutdownTemp);
                sdk.ShutDown();
            }
        }
    }
}
