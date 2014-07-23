using System;
using System.Collections.Generic;
using System.Text;
using WiimoteLib;
using System.Windows.Threading;
using System.Drawing;
using System.Timers;

namespace GestureLib
{
    public class WiiAirMouseGestureDevice : AbstractGestureDevice, IPointerGestureDevice, IAccelerationGestureDevice, IMouseEmulationDevice
    {
        public event EventHandler<MouseEmulationStateEventArgs> MouseEmulationStateChanged;
        private delegate void WiimoteChangedDelegate(WiimoteChangedEventArgs args);

        private Wiimote _wiimote;
        private bool _isLeftEmulationButtonPressed;
        private bool _isRightEmulationButtonPressed;

        private Timer _motionTimer;

        private float _dWiiMotePosX;    // Current WiiMote-X value
        private float _dWiiMotePosY;    // Current WiiMote-Y value

        private System.Drawing.Size _screenSize;
        
        public WiiAirMouseGestureDevice(Size screenSize)
        {
            _screenSize = screenSize;

            _dWiiMotePosX = MouseApi.MousePosition.X / screenSize.Width;
            _dWiiMotePosY = MouseApi.MousePosition.Y / screenSize.Height;
        }

        public bool IsActionButtonPressed { get; private set; }
        public Dispatcher Dispatcher { get; set; }

        //public Wiimote Wiimote
        //{
        //    get { return _wiimote; }
        //    set
        //    {
        //        if (_wiimote != null)
        //        {
        //            _wiimote.WiimoteChanged -=
        //            _wiimote.WiimoteChanged -= Wiimote_WiimoteChanged;
        //            _motionTimer.Stop();
        //            _motionTimer.Elapsed -= new ElapsedEventHandler(MotionTimer_Elapsed);
        //        }

        //        _wiimote = value;
        //        _wiimote.WiimoteChanged += new WiimoteChangedEventHandler(Wiimote_WiimoteChanged);
        //        _motionTimer = new Timer(10.0);
        //        _motionTimer.Elapsed += new ElapsedEventHandler(MotionTimer_Elapsed);
        //        _motionTimer.Start();
        //    }
        //}

        private void MotionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Some information: the WiiMote utilize the ADXL330 (3-axis, +/- 3g MEMS Accelerometer)
            // from Analog Devices. It is a complete 3-axis accelerometer with signal conditioned
            // voltage outputs, measuring position, motion, tilt, shock and vibration.
            // Operating voltage: 1.8 to 3.6V
            //
            // The Motion Control is currently based on a Singular Signed Quadratic form with one
            // variable, in its general form: F(x) = ax^2. However, since we have to deal with the
            // WiiMote which delivers positive and negative values, the formula has to be adapted,
            // otherwise the negative values will be lost due to the squaring.
            // Ultimately the working implentation form is:
            // sign = sqrt(x^2) / x     (1)  (the code will use the built-in Math.Sign function)
            // f(x) = sign * x^2        (2)
            // putting (1) and (2) together gives the working algorithm implemented in this version:
            // f(x) = x * sqrt(x^2)

            //---------- POSITION_X:------------------------------------------
            float dSamplePeriod = 0.01F;
            float dWiiOffsetX = 0.03846154F;
            float dWiiOffsetY = -0.04F;
            int nSpeedGain = 4;

            if (AccelerationGestureState != null)
            {
                float dWiiMoteSetSpeedX = AccelerationGestureState.X - dWiiOffsetX;   // CURRENT position X from WiiMote

                // Using the Signed Square function:
                int nSign = Math.Sign(dWiiMoteSetSpeedX);   // Mind the Wiimote directions ...
                float dWiiMoteSSXQuadratic = (float)Math.Pow((double)dWiiMoteSetSpeedX, 2.0) * nSign;

                _dWiiMotePosX = (_dWiiMotePosX + (dWiiMoteSSXQuadratic * dSamplePeriod) * nSpeedGain);   // INTEGRATOR

                // Do some limitations ...
                if (_dWiiMotePosX > 1) _dWiiMotePosX = 1;
                if (_dWiiMotePosX < 0) _dWiiMotePosX = 0;

                //---------- POSITION_Y:------------------------------------------
                float dWiiMoteSetSpeedY = AccelerationGestureState.Y - dWiiOffsetY;   // CURRENT position Y from WiiMote
                //lblVy.Text = dWiiMoteSetSpeedY.ToString();  // Just for debug visualisation

                // Using the Signed Square function:
                nSign = Math.Sign(dWiiMoteSetSpeedY);   // Mind the Wiimote directions ...
                float dWiiMoteSSYQuadratic = (float)Math.Pow((double)dWiiMoteSetSpeedY, 2.0) * nSign;

                _dWiiMotePosY = (_dWiiMotePosY + (dWiiMoteSSYQuadratic * dSamplePeriod) * nSpeedGain);   // INTEGRATOR

                // Do some limitations ...
                if (_dWiiMotePosY > 1) _dWiiMotePosY = 1;
                if (_dWiiMotePosY < 0) _dWiiMotePosY = 0;

                //----------------- MOVING THE CURSOR: ----------------------------
                //OnMouseEmulationStateChanged(
                //    new MouseEmulationStateEventArgs(
                //        new PointF(_dWiiMotePosX, _dWiiMotePosY),
                //        null));
            }
        }

        private void Wiimote_WiimoteChanged(object sender, WiimoteChangedEventArgs args)
        {
            WiimoteChangedDelegate dlg = new WiimoteChangedDelegate(WiimoteChanged);
            Dispatcher.Invoke(DispatcherPriority.Normal, dlg, args);
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
            if (args.WiimoteState.IRState.IRSensors[0].Found)
            {
                int count = 0;
                float avgX = 0.0F;
                float avgY = 0.0F;

                if (args.WiimoteState.IRState.IRSensors[0].Found)
                {
                    avgX += args.WiimoteState.IRState.IRSensors[0].RawPosition.X;
                    avgY += args.WiimoteState.IRState.IRSensors[0].RawPosition.Y;
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
                args.WiimoteState.AccelState.RawValues.X,//.AccelState.X,
                args.WiimoteState.AccelState.RawValues.Y,
                args.WiimoteState.AccelState.RawValues.Z);
            #endregion

            OnGestureDeviceParametersChanged();

            #region Mouse Emulation

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
                    null,
                    mouseButtonState));
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
