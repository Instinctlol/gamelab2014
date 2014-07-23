using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GestureLib
{
    /// <summary>
    /// Describes one state of an pointer gesture.
    /// </summary>
    public class PointerGestureState : IGestureState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PointerGestureState"/> class.
        /// </summary>
        /// <param name="x">The X-pointer value.</param>
        /// <param name="y">The Y-pointer value.</param>
        public PointerGestureState(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Gets the Y-pointer value.
        /// </summary>
        /// <value>The Y-pointer value.</value>
        public float X { get; private set; }

        /// <summary>
        /// Gets the Y-pointer value.
        /// </summary>
        /// <value>The Y-pointer value.</value>
        public float Y { get; private set; }
    }
}
