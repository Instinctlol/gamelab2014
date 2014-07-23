using System;
using System.Collections.Generic;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Defines the methods and properties, which a Pointer-Device (e.g. mouse) must implement.
    /// </summary>
    public interface IPointerGestureDevice
    {
        /// <summary>
        /// Gets the pointer gesture state.
        /// </summary>
        /// <value>The pointer gesture state.</value>
        PointerGestureState PointerGestureState
        {
            get;
        }
    }
}
