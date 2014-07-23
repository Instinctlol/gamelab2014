using System;
using System.Collections.Generic;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Defines the methods and properties, which a Acceleration-Device (e.g. Wii Remote) must implement.
    /// </summary>
    public interface IAccelerationGestureDevice
    {
        /// <summary>
        /// Gets the acceleration gesture state.
        /// </summary>
        /// <value>The acceleration gesture state.</value>
        AccelerationGestureState AccelerationGestureState
        {
            get;
        }
    }
}
