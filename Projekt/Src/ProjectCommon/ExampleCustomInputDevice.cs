// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Engine.MathEx;
using WiimoteLib;
using ProjectCommon;

namespace ProjectCommon
{
    //For enabling this example device you need uncomment "ExampleCustomInputDevice.InitDevice();"
    //in the GameEngineApp.cs. After it you will see this device in the Game Options window.

    public class ExampleCustomDeviceSpecialEvent : InputEvent
    {
        public ExampleCustomDeviceSpecialEvent(InputDevice device)
            : base(device)
        {
        }
    }

    public class ExampleCustomInputDevice : JoystickInputDevice
    {
        public static bool _globalInventoryTrigger = false;
        public static bool _globalInventoryLeft = false;
        public static bool _globalInventoryRight = false;
        public static bool _globalWeaponStatusInfo = false;


        public ExampleCustomInputDevice(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Initialize the device
        /// </summary>
        /// <returns>Returns true if initializng was successfully</returns>
        internal bool Init()
        {

            if (WiiManager.InstanceWM == null)
                WiiManager.InitMote();

            //buttons
            Button[] buttons = new Button[13];
            buttons[0] = new Button(JoystickButtons.Button1, 0); //UP
            buttons[1] = new Button(JoystickButtons.Button2, 1); //DOWN
            buttons[2] = new Button(JoystickButtons.Button3, 2); //RIGHT
            buttons[3] = new Button(JoystickButtons.Button4, 3); //LEFT
            buttons[4] = new Button(JoystickButtons.Button5, 4); //A
            buttons[5] = new Button(JoystickButtons.Button6, 5); //B
            buttons[6] = new Button(JoystickButtons.Button7, 6); //+
            buttons[7] = new Button(JoystickButtons.Button8, 7); //-
            buttons[8] = new Button(JoystickButtons.Button9, 8); //1
            buttons[9] = new Button(JoystickButtons.Button10, 9); //2
            buttons[10] = new Button(JoystickButtons.Button11, 10); //HOME
            buttons[11] = new Button(JoystickButtons.Button12, 11); //C
            buttons[12] = new Button(JoystickButtons.Button13, 12); //Z


            //axes
            Axis[] axes = new Axis[2];
            axes[0] = new JoystickInputDevice.Axis(JoystickAxes.X, new Range(-1, 1), false);
            axes[1] = new JoystickInputDevice.Axis(JoystickAxes.Y, new Range(-1, 1), false);

            //povs
            POV[] povs = new POV[0];
            //povs[ 0 ] = new JoystickInputDevice.POV( JoystickPOVs.POV1 );

            //sliders
            Slider[] sliders = new Slider[0];
            //sliders[ 0 ] = new Slider( JoystickSliders.Slider1 );

            //forceFeedbackController
            ForceFeedbackController forceFeedbackController = null;

            //initialize data
            InitDeviceData(buttons, axes, povs, sliders, forceFeedbackController);

            return true;
        }

        /// <summary>
        /// Shutdown the device
        /// </summary>
        protected override void OnShutdown()
        {

            if (WiiManager.InstanceWM != null)
                WiiManager.closeMotes();
        }

        /// <summary>
        /// Update the device state. Calling at each tick.
        /// </summary>
        protected override void OnUpdateState()
        {
            WiiManager.InstanceWM.wm.SetReportType(InputReport.ButtonsAccel, false);// gestenerkennung
            WiiManager.InstanceWM.wm.SetReportType(InputReport.ButtonsExtension, true); // steuerung
            

            WiimoteState wiiState = WiiManager.InstanceWM.wm.WiimoteState;

            //WiiManager.UpdateWiimoteChanged();

            const float nunchuckThreshold = 1.0f;
            const float nunchuckScale = 2.0f;

            float nunchuckX = wiiState.NunchukState.Joystick.X; // [-0.5, 0.5], left negative, right positive
            float nunchuckY = wiiState.NunchukState.Joystick.Y; // [-0.5, 0.5], bottom negative, top positive

            bool buttonUpPressed = wiiState.ButtonState.Up;
            bool buttonDownPressed = wiiState.ButtonState.Down;
            bool buttonLeftPressed = wiiState.ButtonState.Left;
            bool buttonRightPressed = wiiState.ButtonState.Right;
            bool buttonAPressed = wiiState.ButtonState.A;
            bool buttonBPressed = wiiState.ButtonState.B;
            bool buttonPlusPressed = wiiState.ButtonState.Plus;
            bool buttonMinusPressed = wiiState.ButtonState.Minus;
            bool buttonHomePressed = wiiState.ButtonState.Home;
            bool buttonOnePressed = wiiState.ButtonState.One;
            bool buttonTwoPressed = wiiState.ButtonState.Two;
            bool buttonCPressed = wiiState.NunchukState.C;
            bool buttoZAPressed = wiiState.NunchukState.Z;
            


            # region BTNs

            //button1 UP
            {
                bool pressed = wiiState.ButtonState.Up;
                if (Buttons[0].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[0]));
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[0]));
                    }
                    Buttons[0].Pressed = pressed;
                }
            }

            //button2 DOWN
            {
                bool pressed = wiiState.ButtonState.Down;
                if (Buttons[1].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[1]));
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[1]));
                    }
                    Buttons[1].Pressed = pressed;
                }
            }



            //button3 RIGHT
            {
                bool pressed = wiiState.ButtonState.Right;
                if (Buttons[2].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[2]));
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[2]));
                    }
                    Buttons[2].Pressed = pressed;
                }
            }

            //button4 LEFT
            {
                bool pressed = wiiState.ButtonState.Left;
                if (Buttons[3].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[3]));
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[3]));
                    }
                    Buttons[3].Pressed = pressed;
                }
            }

            //button5 A
            {
                bool pressed = wiiState.ButtonState.A;
                if (Buttons[4].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[4]));
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[4]));
                    }
                    Buttons[4].Pressed = pressed;
                }
            }

            //button6 A
            {
                bool pressed = wiiState.ButtonState.B;
                if (Buttons[5].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[5]));
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[5]));
                    }
                    Buttons[5].Pressed = pressed;
                }
            }

            //button7 PLUS
            {
                bool pressed = wiiState.ButtonState.Plus||_globalInventoryRight;
                if (Buttons[6].Pressed != pressed)
                {
                    if (pressed && !WiiManager.openInventory)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[6]));
                        _globalInventoryRight = false;
                    }
                    else if (!pressed && !WiiManager.openInventory)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[6]));
                    }
                    Buttons[6].Pressed = pressed;
                }
            }

            //button8 MINUS
            {
                bool pressed = wiiState.ButtonState.Minus|| _globalInventoryLeft;
                if (Buttons[7].Pressed != pressed)
                {
                    if (pressed && !WiiManager.openInventory)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[7]));
                        _globalInventoryLeft = false;
                    }
                    else if (!pressed && !WiiManager.openInventory)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[7]));
                    }
                    Buttons[7].Pressed = pressed;
                }
            }


            //button9 A
            {
                bool pressed = wiiState.ButtonState.One;
                if (Buttons[8].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[8]));
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[8]));
                    }
                    Buttons[8].Pressed = pressed;
                }
            }

            //button10 TWO
            {
                bool pressed = wiiState.ButtonState.Two || _globalWeaponStatusInfo;
                if (Buttons[9].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[9]));
                        _globalWeaponStatusInfo = false;
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[9]));
                    }
                    Buttons[9].Pressed = pressed;
                }
            }

            //button11 HOME
            {
                bool pressed = wiiState.ButtonState.Home||_globalInventoryTrigger;
                if (Buttons[10].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[10]));
                        _globalInventoryTrigger = false;
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[10]));
                    }
                    Buttons[10].Pressed = pressed;
                }
            }

            //button12 C
            {
                bool pressed = wiiState.NunchukState.C;
                if (Buttons[11].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[11]));
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[11]));
                    }
                    Buttons[11].Pressed = pressed;
                }
            }

            //button13 Z
            {
                bool pressed = wiiState.NunchukState.Z;
                if (Buttons[12].Pressed != pressed)
                {
                    if (pressed)
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonDownEvent(this, Buttons[12]));
                    }
                    else
                    {
                        InputDeviceManager.Instance.SendEvent(
                            new JoystickButtonUpEvent(this, Buttons[12]));
                    }
                    Buttons[12].Pressed = pressed;
                }
            }
            # endregion

            //axis X
            {
                float value = nunchuckX * nunchuckScale; ; //MathFunctions.Sin(EngineApp.Instance.Time * 2.0f);

                Axes[0].Value = value;

                InputDeviceManager.Instance.SendEvent(
                    new JoystickAxisChangedEvent(this, Axes[0]));
            }

            //axis Y
            {
                float value = nunchuckY; //* nunchuckScale; //MathFunctions.Sin(EngineApp.Instance.Time * 2.0f);

                Axes[1].Value = value;

                InputDeviceManager.Instance.SendEvent(
                    new JoystickAxisChangedEvent(this, Axes[1]));
            }


            //    //custom event example
            //    {
            //        //this event will be caused in the EngineApp.OnCustomInputDeviceEvent()
            //        //and in the all gui controls EControl.OnCustomInputDeviceEvent().
            //        ExampleCustomInputDeviceSpecialEvent customEvent =
            //            new ExampleCustomInputDeviceSpecialEvent( this );
            //        InputDeviceManager.Instance.SendEvent( customEvent );
            //    }
        }

            /// <summary>
            /// Initialize the device and register them in the InputDeviceManager
            /// </summary>
            public static void InitDevice()
            {
                if( InputDeviceManager.Instance == null )
                    return;

                ExampleCustomInputDevice device = new ExampleCustomInputDevice( "ExampleCustomDevice" );

                if( !device.Init() )
                    return;

                InputDeviceManager.Instance.RegisterDevice( device );
            }
       
    }
}
