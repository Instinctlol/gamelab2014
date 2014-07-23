using System;
using System.Collections.Generic;
using System.Text;

namespace Sensors
{
    public enum Brightness
    {
        Dark,
        Dim,
        Normal,
        Bright
    }

    public delegate void BrightnessChangedHandler(ILightSensor sender);

    public interface ILightSensor : IDisposable
    {
        double GetLumens();
        Brightness Brightness
        {
            get;
        }
        event BrightnessChangedHandler BrightnessChanged;
    }
}
