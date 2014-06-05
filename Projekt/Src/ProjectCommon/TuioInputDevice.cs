// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Engine.MathEx;
using TUIO;

namespace ProjectCommon
{


	//For enabling this example device you need uncomment "ExampleCustomInputDevice.InitDevice();"
	//in the GameEngineApp.cs. After it you will see this device in the Game Options window.

	public class ExampleCustomInputDeviceSpecialEvent : InputEvent
	{
		public ExampleCustomInputDeviceSpecialEvent( InputDevice device )
			: base( device )
		{
		}
	}

	public class TuioInputDevice : JoystickInputDevice
	{
        public static List<float[]> tuioInputData = new List<float[]>();
        public static bool[] detected = new bool[200];
		public TuioInputDevice( string name )
			: base( name )
		{
		}

		/// <summary>
		/// Initialize the device
		/// </summary>
		/// <returns>Returns true if initializng was successfully</returns>
		internal bool Init()
		{
            Console.WriteLine("init methode");

			//buttons
			Button[] buttons = new Button[ 2 ];
			buttons[ 0 ] = new Button( JoystickButtons.Button1, 0 );
            
            buttons[ 1 ] = new Button(JoystickButtons.Button2, 1);

			//axes
			Axis[] axes = new Axis[ 1 ];
			axes[ 0 ] = new JoystickInputDevice.Axis( JoystickAxes.X, new Range( -1, 1 ), false );

			//povs
			POV[] povs = new POV[ 0 ];
			//povs[ 0 ] = new JoystickInputDevice.POV( JoystickPOVs.POV1 );

			//sliders
			Slider[] sliders = new Slider[ 0 ];
			//sliders[ 0 ] = new Slider( JoystickSliders.Slider1 );

			//forceFeedbackController
			ForceFeedbackController forceFeedbackController = null;

			//initialize data
			InitDeviceData( buttons, axes, povs, sliders, forceFeedbackController );

			return true;
		}

		/// <summary>
		/// Shutdown the device
		/// </summary>
		protected override void OnShutdown()
		{
		}

		/// <summary>
		/// Update the device state. Calling at each tick.
		/// </summary>
		protected override void OnUpdateState()
		{

            //Importiere Daten aus Tuio
            List<float[]> tuioInputDataTemp = TuioDump.getData();
            for (int i = 0; i < tuioInputDataTemp.Count; i++)
            {
                tuioInputData.Add(tuioInputDataTemp[i]);
            }
            //Entferne Eintraege aelter als 3 Sekunden
            float timestamp = DateTime.Now.Millisecond+DateTime.Now.Second*1000+DateTime.Now.Minute*60000+DateTime.Now.Hour*3600000;
            if (tuioInputData.Count != 0)
            {
                bool delete = true;
                while (delete) {
                    if (tuioInputData[0][2] + 3000 <= timestamp)
                    {
                        tuioInputData.Remove(tuioInputData[0]);
                        if (tuioInputData.Count != 0)
                        {
                            delete = true;
                        }
                        else {
                            delete = false;
                        }
                    }
                    else {
                        delete = false;
                    }
                }
            }

            //primitive Erkennung

            List<float[]> used = new List<float[]>();
            foreach (float[] dataelement in tuioInputData) {
                if (!detected[0] && dataelement[0] == 0 && dataelement[3] == 1) {
                    detected[0] = true; 
                    used.Add(dataelement);
                }
                if (detected[0] && dataelement[0] == 0 && dataelement[3] == 3) {
                    detected[0] = false;

                    ExampleCustomInputDeviceSpecialEvent customEvent =
                        new ExampleCustomInputDeviceSpecialEvent(this);
                    InputDeviceManager.Instance.SendEvent(customEvent);
                    Console.WriteLine("send");
                    used.Add(dataelement);
                }
                if (dataelement[0] == 0 && dataelement[3] == 2 && detected[0]) {
                    ExampleCustomInputDeviceSpecialEvent customEvent =
                        new ExampleCustomInputDeviceSpecialEvent(this);
                    InputDeviceManager.Instance.SendEvent(customEvent);
                }
            }
            foreach(float[] usedelemt in used){
                tuioInputData.Remove(usedelemt);
            }



            //button1





            {
                bool pressed = EngineApp.Instance.IsKeyPressed(EKeys.H);
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

			//button2
			{
				bool pressed = EngineApp.Instance.IsKeyPressed( EKeys.J );
				if( Buttons[ 1 ].Pressed != pressed )
				{
					if( pressed )
					{
						InputDeviceManager.Instance.SendEvent(
							new JoystickButtonDownEvent( this, Buttons[ 1 ] ) );
					}
					else
					{
						InputDeviceManager.Instance.SendEvent(
							new JoystickButtonUpEvent( this, Buttons[ 1 ] ) );
					}
					Buttons[ 1 ].Pressed = pressed;
				}
			}

			//axis X
			{
                float value = 0;

				Axes[ 0 ].Value = value;

				InputDeviceManager.Instance.SendEvent(
					new JoystickAxisChangedEvent( this, Axes[ 0 ] ) );
			}

			//custom event example
			
		}

		/// <summary>
		/// Initialize the device and register them in the InputDeviceManager
		/// </summary>
		public static void InitDevice()
		{
            TuioDump.runTuio();
            Console.WriteLine("TUIO gestartet");
            //if (InputDeviceManager.Instance == null) {
            //}
            //    return;
            Console.WriteLine("instance OK");
			TuioInputDevice device = new TuioInputDevice( "TUIO" );
            Console.WriteLine("Device erstellt");
			if( !device.Init() )
				return;

			InputDeviceManager.Instance.RegisterDevice( device );
            Console.WriteLine("Device hinzugefügt");
		}
	}
}
