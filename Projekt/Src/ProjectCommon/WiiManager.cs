using System;
using System.Collections.Generic;
using System.Text;
using WiimoteLib;
using ProjectCommon;
using Engine;
using Engine.MathEx;


namespace ProjectCommon
{
    public class WiiManager
    {

        static WiiManager instanceWM;

        WiimoteCollection wmc = null;
        bool wiiMoteInitialized = false;
        public Wiimote wm = new Wiimote();
        public WiimoteState lastState = null;
        bool useWiiMote = true; // 
        

        private delegate void UpdateWiimoteStateDelegate(WiimoteChangedEventArgs args);
        private delegate void UpdateExtensionChangedDelegate(WiimoteExtensionChangedEventArgs args);

        public static WiiManager InstanceWM
        {

            get { return instanceWM; }

        }

        public static void InitMote()
        {
           instanceWM = new WiiManager();

            if (instanceWM.useWiiMote)
            {
                try
                {
                    InstanceWM.wm = new Wiimote();
                    InstanceWM.lastState = new WiimoteState();

                    InstanceWM.wm.Connect();
                    InstanceWM.wiiMoteInitialized = true;
                    InstanceWM.wm.SetLEDs(true, true, false, false);
                    InstanceWM.lastState = InstanceWM.wm.WiimoteState;

                    InstanceWM.lastState.ButtonState.A = false;
                    InstanceWM.lastState.ButtonState.Up = false;
                    InstanceWM.lastState.ButtonState.Down = false;
                    InstanceWM.lastState.ButtonState.Left = false;
                    InstanceWM.lastState.ButtonState.Right = false;
                }
                catch (Exception  /*ex*/)
                {
                    InstanceWM.wiiMoteInitialized = false;
                }
            }
            else
            {
                InstanceWM.wiiMoteInitialized = false;
            }

            
        }

        // WiiMotes enkoppeln
        public static void closeMotes()
        {
            if(instanceWM.wiiMoteInitialized)
            {
                instanceWM.wm.SetLEDs(false, false, false, false);
                instanceWM.wm.Disconnect();
            }

        }


        // Ereignisse 

        //private void UpdateWiimoteChanged(WiimoteChangedEventArgs args)
        public static void UpdateWiimoteChanged()
        {
            //WiimoteState ws = args.WiimoteState;






            if (InstanceWM.wiiMoteInitialized)
            {
                WiimoteState wiiState = InstanceWM.wm.WiimoteState;
                const float nunchuckThreshold = 0.1f;
                const float nunchuckScale = 0.01f;

                float nunchuckX = wiiState.NunchukState.Joystick.X; // [-0.5, 0.5], left negative, right positive
                float nunchuckY = wiiState.NunchukState.Joystick.Y; // [-0.5, 0.5], bottom negative, top positive

                bool buttonUpPressed = wiiState.ButtonState.Up;
                bool buttonDownPressed = wiiState.ButtonState.Down;
                bool buttonLeftPressed = wiiState.ButtonState.Left;
                bool buttonRightPressed = wiiState.ButtonState.Right;

                bool buttonUpPressedOld = InstanceWM.lastState.ButtonState.Up;
                bool buttonDownPressedOld = InstanceWM.lastState.ButtonState.Down;
                bool buttonLeftPressedOld = InstanceWM.lastState.ButtonState.Left;
                bool buttonRightPressedOld = InstanceWM.lastState.ButtonState.Right;

                bool buttonAPressed = wiiState.ButtonState.A;
                bool buttonAPressedOld = InstanceWM.lastState.ButtonState.A;

                if (buttonUpPressed /*&& !buttonUpPressedOld*/)
                {
                    GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.Up));
                }
                else /*if (!buttonUpPressed && buttonUpPressedOld)*/
                {
                    GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.Up));
                }

                if (buttonDownPressed)
                {
                    GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.Down));
                }
                else
                {
                    GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.Down));
                }

                if (buttonLeftPressed)
                {
                    GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.Left));
                }
                else
                {
                    GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.Left));
                }

                if (buttonRightPressed)
                {
                    GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.Right));
                }
                else
                {
                    GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.Right));
                }

                if (buttonAPressed)
                {
                    GameControlsManager.Instance.DoMouseDown(EMouseButtons.Left);
                }
                else
                {
                    GameControlsManager.Instance.DoMouseUp(EMouseButtons.Left);
                }

                if (Math.Abs(nunchuckX) > nunchuckThreshold)
                {
                    GameControlsManager.Instance.DoMouseMoveRelative(new Vec2(nunchuckScale * nunchuckX, 0.0f));
                }

                if (Math.Abs(nunchuckY) > nunchuckThreshold)
                {
                    GameControlsManager.Instance.DoMouseMoveRelative(new Vec2(0.0f, nunchuckScale * -1.0f * nunchuckY));
                }

                InstanceWM.lastState = wiiState;
            }

        }


    }
}
