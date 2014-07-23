using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GestureLib
{
    /// <summary>
    /// Describes the event arguments of a mouse event
    /// </summary>
    public class MouseEmulationStateEventArgs : EventArgs
    {
        private MouseButtonState? _mouseButtonState;
        private PointF? _mousePosition;

        /// <summary>
        /// Initializes a new instance of the <see cref="MouseEmulationStateEventArgs"/> class.
        /// </summary>
        /// <param name="mousePosition">The new mouse position.</param>
        /// <param name="mouseButtonState">The new state of the mouse button.</param>
        public MouseEmulationStateEventArgs(PointF? mousePosition, MouseButtonState? mouseButtonState)
        {
            _mousePosition = mousePosition;
            _mouseButtonState = mouseButtonState;
        }

        /// <summary>
        /// Gets the mouse position.
        /// </summary>
        /// <value>The mouse position.</value>
        public PointF? MousePosition
        {
            get { return _mousePosition; }
        }

        /// <summary>
        /// Gets the state of the mouse button.
        /// </summary>
        /// <value>The state of the mouse button.</value>
        public MouseButtonState? MouseButtonState
        {
            get { return _mouseButtonState; }
        }
    }
}
