using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GestureLib
{
    /// <summary>
    /// Defines the entry point for the GestureLib library
    /// </summary>
    public class GestureLib : IDisposable
    {
        private Recording _recording;
        private AbstractGestureDevice _gestureDevice;
        private AbstractConfigurationManager _configurationManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="GestureLib"/> class.
        /// </summary>
        public GestureLib()
        {
            AvailableGestureActions = new GestureActionCollection() { AutonamingEnabled = true };
            AvailableGestureAlgorithms = new GestureAlgorithmCollection() { AutonamingEnabled = true };
            TrainedGestures = new TrainedGestureCollection();
        }

        /// <summary>
        /// Gets or sets the gesture device, which is used for recording the gestures.
        /// </summary>
        /// <value>The gesture device.</value>
        public AbstractGestureDevice GestureDevice 
        {
            get { return _gestureDevice; }
            set
            {
                if (_gestureDevice != null)
                {
                    _gestureDevice.RecordingStart -= new EventHandler(GestureDevice_RecordingStart);
                    _gestureDevice.RecordingFinish -= new EventHandler(GestureDevice_RecordingFinish);

                    if (_gestureDevice.ImplementsInterface(typeof(IMouseEmulationDevice)))
                    {
                        IMouseEmulationDevice pointerGestureDevice = (IMouseEmulationDevice)_gestureDevice;
                        pointerGestureDevice.MouseEmulationStateChanged -= new EventHandler<MouseEmulationStateEventArgs>(GestureDevice_MouseEmulationStateChanged);
                    }
                }
                
                _gestureDevice = value;

                _gestureDevice.RecordingStart += new EventHandler(GestureDevice_RecordingStart);
                _gestureDevice.RecordingFinish += new EventHandler(GestureDevice_RecordingFinish);

                if (_gestureDevice.ImplementsInterface(typeof(IMouseEmulationDevice)))
                {
                    IMouseEmulationDevice pointerGestureDevice = (IMouseEmulationDevice)_gestureDevice;
                    pointerGestureDevice.MouseEmulationStateChanged += new EventHandler<MouseEmulationStateEventArgs>(GestureDevice_MouseEmulationStateChanged);
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the configuration manager, which is used for storing TrainedGestures collection.
        /// </summary>
        /// <value>The configuration manager.</value>
        public AbstractConfigurationManager ConfigurationManager
        {
            get { return _configurationManager; }
            set
            {
                _configurationManager = value;
                _configurationManager.InternalGestureLib = this;
            }
        }

        /// <summary>
        /// Holds the TrainedGestures.
        /// </summary>
        /// <value>The TrainedGestures.</value>
        public TrainedGestureCollection TrainedGestures { get; private set; }
        
        /// <summary>
        /// Holds the available gesture actions, which can be assigned in a TrainedGesture.
        /// </summary>
        /// <value>The available gesture actions.</value>
        public GestureActionCollection AvailableGestureActions { get; private set; }
        
        /// <summary>
        /// Holds the available gesture algorithms, which will analyze the recorded gesture states.
        /// </summary>
        /// <value>The available gesture algorithms.</value>
        public GestureAlgorithmCollection AvailableGestureAlgorithms { get; private set;  }

        /// <summary>
        /// Gets a reference to the recorder and analyzer for the gesture states.
        /// </summary>
        /// <value>The recording.</value>
        public Recording Recording 
        {
            get
            { 
                if(_recording == null)
                {
                    _recording = new Recording(this);
                }

                return _recording;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the mouse will be impersonated by the GestureDevice.
        /// </summary>
        /// <value><c>true</c> if the mouse is impersonated otherwise, <c>false</c>.</value>
        public bool ImpersonatingMouse { get; set; }

        /// <summary>
        /// Gets or sets the screen bounds, which define the range where the impersonated mouse cursor will be projected.
        /// </summary>
        /// <value>The screen bounds.</value>
        public Rectangle ScreenBounds { get; set; }

        private void GestureDevice_RecordingStart(object sender, EventArgs e)
        {
            Recording.StartRecording();
        }
        private void GestureDevice_RecordingFinish(object sender, EventArgs e)
        {
            Recording.EndRecording();
        }

        private void GestureDevice_MouseEmulationStateChanged(object sender, MouseEmulationStateEventArgs e)
        {
            if (ImpersonatingMouse)
            {
                if (e.MousePosition.HasValue)
                {
                    PointF pointerLocation = new PointF(e.MousePosition.Value.X, e.MousePosition.Value.Y);
                    pointerLocation.X *= (float)ScreenBounds.Width + (float)ScreenBounds.Left;
                    pointerLocation.Y *= (float)ScreenBounds.Height + (float)ScreenBounds.Top;

                    MouseApi.MousePosition = new System.Drawing.Point((int)pointerLocation.X, (int)pointerLocation.Y);
                }

                if (e.MouseButtonState.HasValue)
                {
                    MouseApi.SetButtonState(e.MouseButtonState.Value);
                }
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _recording.Dispose();
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
