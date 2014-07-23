using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Drawing;

namespace GestureLib
{
    /// <summary>
    /// Defines the button action of the mouse
    /// </summary>
    public enum MouseButtonState
    {
        /// <summary>
        /// Press left mouse button
        /// </summary>
        LeftButtonDown,
        /// <summary>
        /// Press right mouse button
        /// </summary>
        RightButtonDown,
        /// <summary>
        /// Stop pressing left mouse button
        /// </summary>
        LeftButtonUp,
        /// <summary>
        /// Stop pressing right mouse button
        /// </summary>
        RightButtonUp,
    }

    /// <summary>
    /// Provides functionality for moving the mouse cursor and simulating mouse clicks
    /// </summary>
    public static class MouseApi
    {
        /// <summary>
        /// Gets or sets the current mouse position.
        /// </summary>
        /// <value>The mouse position.</value>
        public static Point MousePosition
        {
            get
            {
                Point point = new Point();
                NativeMethods.GetCursorPos(ref point);

                return point;
            }

            set
            {
                NativeMethods.SetCursorPos(value.X, value.Y);
            }
        }

        /// <summary>
        /// Sets the state of the mouse button.
        /// </summary>
        /// <param name="mouseButtonState">State of the mouse button.</param>
        public static void SetButtonState(MouseButtonState mouseButtonState)
        {
            NativeMethods.MouseEventType? mouseEventType = null;

            switch (mouseButtonState)
            {
                case MouseButtonState.LeftButtonDown:
                    mouseEventType = NativeMethods.MouseEventType.MOUSEEVENTF_LEFTDOWN;
                    break;
                case MouseButtonState.LeftButtonUp:
                    mouseEventType = NativeMethods.MouseEventType.MOUSEEVENTF_LEFTUP;
                    break;
                case MouseButtonState.RightButtonDown:
                    mouseEventType = NativeMethods.MouseEventType.MOUSEEVENTF_RIGHTDOWN;
                    break;
                case MouseButtonState.RightButtonUp:
                    mouseEventType = NativeMethods.MouseEventType.MOUSEEVENTF_RIGHTUP;
                    break;
                default:
                    break;
            }
            if (mouseEventType.HasValue)
            {
                NativeMethods.mouse_event(mouseEventType.Value, 0, 0, 0, IntPtr.Zero);
            }
        }
    }
}
