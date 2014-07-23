using System;
using System.Collections.Generic;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Base class for a new gesture device (e.g. mouse or Wii Remote)
    /// </summary>
    public abstract class AbstractGestureDevice
    {
        /// <summary>
        /// Occurs when the gesture state changed.
        /// </summary>
        public event EventHandler GestureDeviceParametersChanged;

        /// <summary>
        /// Occurs when the recording process of the gesture started.
        /// </summary>
        public event EventHandler RecordingStart;

        /// <summary>
        /// Occurs when the recording process of the gesture is finished.
        /// </summary>
        public event EventHandler RecordingFinish;

        /// <summary>
        /// Called when the gesture state changed.
        /// </summary>
        protected virtual void OnGestureDeviceParametersChanged()
        {
            EventHandler handler = GestureDeviceParametersChanged;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when the recording process of the gesture started.
        /// </summary>
        protected virtual void OnRecordingStart()
        {
            EventHandler handler = RecordingStart;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when the recording process of the gesture is finished.
        /// </summary>
        protected virtual void OnRecordingFinish()
        {
            EventHandler handler = RecordingFinish;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
