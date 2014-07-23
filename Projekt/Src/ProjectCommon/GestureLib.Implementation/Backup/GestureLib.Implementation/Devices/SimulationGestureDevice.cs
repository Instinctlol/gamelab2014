using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;

namespace GestureLib
{
    public class SimulationGestureDevice : AbstractGestureDevice, IPointerGestureDevice, IAccelerationGestureDevice, IMouseEmulationDevice, IDisposable
    {
        public event EventHandler<MouseEmulationStateEventArgs> MouseEmulationStateChanged;

        private Timer _eventRaiseTimer;
        private int _currentGestureIndex;
        private List<string[]> _gestures;

        public int Speed { get; set; }
        public string SourceFileName { get; set; }
        public bool IsRunning { get; private set; }

        public void StartGestureDevice()
        {
            if (!IsRunning)
            {
                OnRecordingStart();

                StreamReader reader = new StreamReader(SourceFileName);

                _gestures = new List<string[]>(); 

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();

                    string[] splitLine = line.Split(';');
                    _gestures.Add(splitLine);
                }

                reader.Dispose();
                reader.Close();

                _eventRaiseTimer = new Timer(new TimerCallback(EventRaiseTimer_Raised), null, 0, Speed);
            }

            IsRunning = true;
        }

        private void EventRaiseTimer_Raised(object e)
        {
            if (_currentGestureIndex < _gestures.Count)
            {
                System.Globalization.NumberFormatInfo numberFormatInfo = new System.Globalization.NumberFormatInfo();
                numberFormatInfo.NumberDecimalSeparator = ".";

                string[] splitLine = _gestures[_currentGestureIndex];

                PointerGestureState = new PointerGestureState(float.Parse(splitLine[0], numberFormatInfo), float.Parse(splitLine[1], numberFormatInfo));
                AccelerationGestureState = new AccelerationGestureState(float.Parse(splitLine[2], numberFormatInfo), float.Parse(splitLine[3], numberFormatInfo), float.Parse(splitLine[4], numberFormatInfo));

                OnGestureDeviceParametersChanged();

                EventHandler<MouseEmulationStateEventArgs> handler = MouseEmulationStateChanged;
                if (handler != null)
                {
                    handler(this, 
                        new MouseEmulationStateEventArgs(
                            new PointF(PointerGestureState.X, PointerGestureState.Y), 
                            null));
                }

                _currentGestureIndex++;
            }
            else
            {
                _currentGestureIndex = 0;
                _eventRaiseTimer.Dispose();
                _eventRaiseTimer = null;

                IsRunning = false;

                OnRecordingFinish();
            }
        }

        #region IPointerGestureDevice Members

        public PointerGestureState PointerGestureState { get; private set; }

        #endregion

        #region IAccelerationGestureDevice Members

        public AccelerationGestureState AccelerationGestureState { get; private set; }

        #endregion

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventRaiseTimer.Dispose();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
