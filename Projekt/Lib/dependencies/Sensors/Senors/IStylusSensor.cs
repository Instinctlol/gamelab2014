using System;
using System.Collections.Generic;
using System.Text;

namespace Sensors
{
    public enum StylusState
    {
        StylusOut = 1,
        StylusIn = 0,
    }

    public delegate void StyleEventHandler(IStylusSensor sensor);

    public interface IStylusSensor
    {
        StylusState StylusState
        {
            get;
        }

        event StyleEventHandler StylusStateChanged;
    }
}
