using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Linq;
using System.Collections.ObjectModel;

namespace GestureLib
{
    /// <summary>
    /// Provides functionality for setting up everything, which corresponds to the gesture recording or analyzing process
    /// </summary>
    public class Recording : IDisposable
    {
        /// <summary>
        /// Occurs when the recording process finished.
        /// </summary>
        public event EventHandler RecordingFinished;

        /// <summary>
        /// Occurs when the recording process started.
        /// </summary>
        public event EventHandler RecordingStarted;

        private int _forceAddingCount;
        private int _eventCount;
        private OverlayForm _overlayForm;

        internal Recording(GestureLib gestureLib)
        {
            GestureLib = gestureLib;
            EventFilterNumber = 1;
        }

        internal GestureLib GestureLib { get; set; }

        /// <summary>
        /// Gets a value, which is indicating the recording process.
        /// </summary>
        /// <value><c>true</c> if the recording process is currently running; otherwise, <c>false</c>.</value>
        public bool RecordingEnabled { get; private set; }

        /// <summary>
        /// Gets or sets the event filter number.
        /// </summary>
        /// <value>The number of events, which will be used for analyzing the recorded data.
        /// (e.g. if you enter 2, then only every second GestureState is used for analyzing, if you enter 3, only every third will is used, and so on...)
        /// </value>
        public int EventFilterNumber { get; set; }

        /// <summary>
        /// Gets the recorded acceleration gesture states.
        /// </summary>
        /// <value>The recorded acceleration gesture states.</value>
        public GestureStateCollection<AccelerationGestureState> RecordedAccelerationGestureStates { get; private set; }

        /// <summary>
        /// Gets or sets the recorded pointer gesture states.
        /// </summary>
        /// <value>The recorded pointer gesture states.</value>
        public GestureStateCollection<PointerGestureState> RecordedPointerGestureStates { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether an overlay will be shown, which displays the current pointer location, when the recording process begins.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if, the overlay form should be shown; otherwise, <c>false</c>.
        /// </value>
        public bool ShowOverlayWhenBeginRecording { get; set; }

        /// <summary>
        /// Starts the recording process.
        /// </summary>
        public void StartRecording()
        {
            //Stop recording, if its still running
            if (RecordingEnabled)
            {
                EndRecording();
            }

            if (ShowOverlayWhenBeginRecording)
            {
                _overlayForm = new OverlayForm();
                _overlayForm.Show();
            }

            //Reset all recorded gesture states
            RecordedAccelerationGestureStates = new GestureStateCollection<AccelerationGestureState>();
            RecordedPointerGestureStates = new GestureStateCollection<PointerGestureState>();

            //Register the event, which checkes for new gesture states
            GestureLib.GestureDevice.GestureDeviceParametersChanged += new EventHandler(GestureDevice_GestureDeviceParametersChanged);
            RecordingEnabled = true;

            OnRecordingStarted();
        }

        /// <summary>
        /// Ends the recording process.
        /// </summary>
        public void EndRecording()
        {
            //Stops recording, if it is running
            if (RecordingEnabled)
            {
                GestureLib.GestureDevice.GestureDeviceParametersChanged -= new EventHandler(GestureDevice_GestureDeviceParametersChanged);
                RecordingEnabled = false;
                _forceAddingCount = 0;

                if (_overlayForm != null)
                {
                    _overlayForm.Close();
                    _overlayForm.Dispose();
                    _overlayForm = null;
                }
            }

            OnRecordingFinished();
        }

        /// <summary>
        /// Called when the recording process started.
        /// </summary>
        protected void OnRecordingStarted()
        {
            EventHandler handler = RecordingStarted;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called when the recording process finished.
        /// </summary>
        protected void OnRecordingFinished()
        {
            EventHandler handler = RecordingFinished;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Recognizes the recording of acceleration gesture states.
        /// </summary>
        /// <param name="recordedAccelerationGestureStates">The recorded acceleration gesture states, which should be analyzed.</param>
        /// <returns></returns>
        public GestureAlgorithmCollection RecognizeRecording(GestureStateCollection<AccelerationGestureState> recordedAccelerationGestureStates)
        {
            GestureAlgorithmCollection gestureAlgorithmCollection = new GestureAlgorithmCollection();

            if (RecordedAccelerationGestureStates.Count > 0)
            {
                float highestResult = 0.0F;

                foreach (IAccelerationGestureAlgorithm algorithm in GestureLib.AvailableGestureAlgorithms.OfType<IAccelerationGestureAlgorithm>())
                {
                    float result = algorithm.CalculateMatching(recordedAccelerationGestureStates);

                    if (result >= 0.9)
                    {
                        if (highestResult < result)
                        {
                            highestResult = result;
                            gestureAlgorithmCollection.Clear();
                        }

                        gestureAlgorithmCollection.Add(algorithm);
                    }
                }
            }

            return gestureAlgorithmCollection;
        }

        /// <summary>
        /// Recognizes the recording of pointer gesture states.
        /// </summary>
        /// <param name="recordedPointerGestureStates">The recorded pointer gesture states, which should be analyzed.</param>
        /// <returns></returns>
        public PointTendenceAnalyzer RecognizeRecording(GestureStateCollection<PointerGestureState> recordedPointerGestureStates)
        {
            GestureAlgorithmCollection gestureAlgorithmCollection = new GestureAlgorithmCollection();
            PointTendenceAnalyzer pointTendenceAnalyzer = new PointTendenceAnalyzer(recordedPointerGestureStates);

            if (pointTendenceAnalyzer.CornerMarks != null)
            {
                for (int i = 1; i < pointTendenceAnalyzer.CornerMarks.Count; i++)
                {
                    float mostMatchingResult = 0.0F;
                    IGestureAlgorithm mostMatchingGestureAlgorithm = null;

                    foreach (IPointerGestureAlgorithm algorithm in GestureLib.AvailableGestureAlgorithms.OfType<IPointerGestureAlgorithm>())
                    {
                        float result = algorithm.CalculateMatching(pointTendenceAnalyzer.CornerMarks[i - 1], pointTendenceAnalyzer.CornerMarks[i]);

                        if (mostMatchingResult < result)
                        {
                            mostMatchingResult = result;
                            mostMatchingGestureAlgorithm = algorithm;
                        }
                    }

                    if (mostMatchingGestureAlgorithm != null)
                    {
                        gestureAlgorithmCollection.Add(mostMatchingGestureAlgorithm);
                    }
                }

                pointTendenceAnalyzer.MatchedGestureAlgorithms = gestureAlgorithmCollection;
            }

            return pointTendenceAnalyzer;
        }

        private void GestureDevice_GestureDeviceParametersChanged(object sender, EventArgs e)
        {
            _eventCount++;

            if (_eventCount % EventFilterNumber == 0)
            {
                //Record the gesture states separatly (1 collection for PointerDevices, 1 for AccelerationDevices)
                if (GestureLib.GestureDevice.ImplementsInterface(typeof(IPointerGestureDevice)))
                {
                    bool denyAdding = false;

                    PointerGestureState newPointerGestureState = ((IPointerGestureDevice)GestureLib.GestureDevice).PointerGestureState;

                    if (newPointerGestureState != null)
                    {
                        if (RecordedPointerGestureStates.Count > 0)
                        {
                            PointerGestureState lastPointerGestureState =
                                RecordedPointerGestureStates[RecordedPointerGestureStates.Count - 1];

                            float diffX = Math.Abs(newPointerGestureState.X - lastPointerGestureState.X);
                            float diffY = Math.Abs(newPointerGestureState.Y - lastPointerGestureState.Y);
                            bool isNearLastValue = diffX < 0.1 && diffY < 0.1;
                            //bool isNearLastValue = true;

                            if ((lastPointerGestureState.X == newPointerGestureState.X &&
                                lastPointerGestureState.Y == newPointerGestureState.Y) ||
                                !isNearLastValue)
                            {
                                _forceAddingCount++;
                                denyAdding = true;
                            }
                        }

                        if (!denyAdding || _forceAddingCount >= 15)
                        {
                            if (_overlayForm != null)
                            {
                                _overlayForm.SetCurrentPoint(
                                    new PointF(newPointerGestureState.X, newPointerGestureState.Y));
                            }

                            _forceAddingCount = 0;
                            RecordedPointerGestureStates.Add(newPointerGestureState);
                        }
                    }
                }

                if (GestureLib.GestureDevice.ImplementsInterface(typeof(IAccelerationGestureDevice)))
                {
                    RecordedAccelerationGestureStates.Add(((IAccelerationGestureDevice)GestureLib.GestureDevice).AccelerationGestureState);
                }

                _eventCount = 0;
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
                if (_overlayForm != null && !_overlayForm.IsDisposed)
                {
                    _overlayForm.Close();
                    _overlayForm.Dispose();
                }
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
