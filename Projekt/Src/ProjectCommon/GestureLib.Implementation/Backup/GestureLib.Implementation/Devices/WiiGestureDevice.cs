using System;
using System.Collections.Generic;
using System.Text;
using WiimoteLib;
using System.Windows.Threading;
using System.Drawing;

namespace GestureLib
{
    public class WiiGestureDevice : AbstractGestureDevice, IPointerGestureDevice, IAccelerationGestureDevice, IMouseEmulationDevice
    {
        public event EventHandler<MouseEmulationStateEventArgs> MouseEmulationStateChanged;
        private delegate void WiimoteChangedDelegate(WiimoteChangedEventArgs args);

        private Wiimote _wiimote;
        private bool _isLeftEmulationButtonPressed;
        private bool _isRightEmulationButtonPressed;

        public bool IsActionButtonPressed { get; private set; }
        public Dispatcher Dispatcher { get; set; }
        
        public Wiimote Wiimote 
        {
            get { return _wiimote; }
            set 
            {
                if (_wiimote != null)
                {
                    _wiimote.WiimoteChanged -= new WiimoteChangedEventHandler(Wiimote_WiimoteChanged);
                }

                _wiimote = value;
                _wiimote.WiimoteChanged += new WiimoteChangedEventHandler(Wiimote_WiimoteChanged);
            }
        }

        private void Wiimote_WiimoteChanged(object sender, WiimoteChangedEventArgs args)
        {
            WiimoteChangedDelegate dlg = new WiimoteChangedDelegate(WiimoteChanged);

            if (Dispatcher != null)
            {
                Dispatcher.Invoke(DispatcherPriority.Normal, dlg, args);
            }
            else
            {
                dlg.Invoke(args);
            }
        }

        private void WiimoteChanged(WiimoteChangedEventArgs args)
        {
            #region Recording Button
            if (args.WiimoteState.ButtonState.B)
            {
                if (!IsActionButtonPressed)
                {
                    IsActionButtonPressed = true;
                    OnRecordingStart();
                }
            }
            else
            {
                if (IsActionButtonPressed)
                {
                    IsActionButtonPressed = false;
                    OnRecordingFinish();
                }
            }
            #endregion

            #region Pointer State (Infrared)
            if (args.WiimoteState.IRState.Found1)
            {
                int count = 0;
                float avgX = 0.0F;
                float avgY = 0.0F;

                if (args.WiimoteState.IRState.Found1)
                {
                    avgX += args.WiimoteState.IRState.X1;
                    avgY += args.WiimoteState.IRState.Y1;
                    count++;

                    /*if (args.WiimoteState.IRState.Found2)
                    {
                        avgX += args.WiimoteState.IRState.X2;
                        avgY += args.WiimoteState.IRState.Y2;
                        count++;

                        if (args.WiimoteState.IRState.Found3)
                        {
                            avgX += args.WiimoteState.IRState.X3;
                            avgY += args.WiimoteState.IRState.Y3;
                            count++;

                            if (args.WiimoteState.IRState.Found4)
                            {
                                avgX += args.WiimoteState.IRState.X4;
                                avgY += args.WiimoteState.IRState.Y4;
                                count++;
                            }
                        }
                    }*/

                    if (count > 0)
                    {

                        avgX /= (float)count;
                        avgY /= (float)count;
                    }
                }
            
                PointerGestureState = new PointerGestureState(
                    1 - avgX,
                    avgY);
            }
            #endregion

            #region Acceleration State
            AccelerationGestureState = new AccelerationGestureState(
                args.WiimoteState.AccelState.X,
                args.WiimoteState.AccelState.Y,
                args.WiimoteState.AccelState.Z);
            #endregion

            OnGestureDeviceParametersChanged();

            #region Mouse Emulation

            if (PointerGestureState != null)
            {
                MouseButtonState? mouseButtonState = null;
                
                #region Left Mouse Button
                if (args.WiimoteState.ButtonState.Minus)
                {
                    if (!_isLeftEmulationButtonPressed)
                    {
                        _isLeftEmulationButtonPressed = true;
                        mouseButtonState = MouseButtonState.LeftButtonDown;
                    }
                }
                else
                {
                    if (_isLeftEmulationButtonPressed)
                    {
                        _isLeftEmulationButtonPressed = false;
                        mouseButtonState = MouseButtonState.LeftButtonUp;
                    }
                }
                #endregion

                #region Right Mouse Button
                if (args.WiimoteState.ButtonState.Plus)
                {
                    if (!_isRightEmulationButtonPressed)
                    {
                        _isRightEmulationButtonPressed = true;
                        mouseButtonState = MouseButtonState.RightButtonDown;
                    }
                }
                else
                {
                    if (_isRightEmulationButtonPressed)
                    {
                        _isRightEmulationButtonPressed = false;
                        mouseButtonState = MouseButtonState.RightButtonUp;
                    }
                }
                #endregion

                OnMouseEmulationStateChanged(
                    new MouseEmulationStateEventArgs(
                        new PointF(PointerGestureState.X, PointerGestureState.Y),
                        mouseButtonState));
            }
            #endregion
        }

        protected void OnMouseEmulationStateChanged(MouseEmulationStateEventArgs e)
        {
            EventHandler<MouseEmulationStateEventArgs> handler = MouseEmulationStateChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        #region IPointerGestureDevice Members
        
        public PointerGestureState PointerGestureState { get; private set; }

        #endregion

        #region IAccelerationGestureDevice Members

        public AccelerationGestureState AccelerationGestureState { get; private set; }

        #endregion
    }
}
