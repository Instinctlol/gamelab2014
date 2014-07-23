using System;
using System.Collections.Generic;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Describes one state of an acceleration gesture.
    /// </summary>
    public class AccelerationGestureState : IGestureState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccelerationGestureState"/> class.
        /// </summary>
        /// <param name="x">The X-acceleration value.</param>
        /// <param name="y">The Y-acceleration value.</param>
        /// <param name="z">The Z-acceleration value.</param>
        public AccelerationGestureState(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Gets the X-acceleration value.
        /// </summary>
        /// <value>The X-acceleration value.</value>
        public float X { get; private set; }
        
        /// <summary>
        /// Gets the Y-acceleration value.
        /// </summary>
        /// <value>The Y-acceleration value.</value>
        public float Y { get; private set; }
        
        /// <summary>
        /// Gets the Z-acceleration value.
        /// </summary>
        /// <value>The Z-acceleration value.</value>
        public float Z { get; private set; }
    }
}
