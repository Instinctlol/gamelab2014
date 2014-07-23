using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Describes the methods and properties, which must be implemented, when a gesture device should be used as a mouse replacement
    /// </summary>
    public interface IMouseEmulationDevice
    {
        /// <summary>
        /// Occurs when mouse state changed.
        /// </summary>
        event EventHandler<MouseEmulationStateEventArgs> MouseEmulationStateChanged;
    }
}
