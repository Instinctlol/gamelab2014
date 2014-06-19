// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Engine.MathEx;
using TUIO;

namespace ProjectCommon
{

    public enum opType
    {
        selection,
        translation,
        rotation
    };
	//For enabling this example device you need uncomment "ExampleCustomInputDevice.InitDevice();"
	//in the GameEngineApp.cs. After it you will see this device in the Game Options window.

	public class TuioInputDeviceSpecialEvent : InputEvent
	{

        opType type;
        float dx,dy;

		public TuioInputDeviceSpecialEvent( InputDevice device, opType type, float dx, float dy)
			: base( device)
		{
            this.type = type;
            this.dx = dx;
            this.dy = dy;
        }

        public opType getOPType() {
            return this.type;
        }
        public float getx()
        {
            return this.dx;
        }
        public float gety()
        {
            return this.dy;
        }
	}

	public class TuioInputDevice : JoystickInputDevice
	{
        public static List<float[]> tuioInputData = new List<float[]>();
        public static bool[] detected = new bool[200];
        public static float[] oldcoords;
        public static float[] oldrot1;
        public static float[] oldrot2;
        public static float oldangle;
        public static bool wastranslating = false, wasrotating = false;
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
            //block 1
            List<float[]> used = new List<float[]>();
            bool start1 = false, start2 = false, end1 = false, end2  = false;
            float[] workelement1 = new float[8];
            float[] line1changed = new float[8];
            float[] line2changed = new float[8];
            float[] workelement2 = new float[8];
            float[] workelement3 = new float[8];
            float[] workelement4 = new float[8];
            //Analyse
            foreach (float[] elemt in tuioInputData) {
                if (elemt[0] == 0 && elemt[3] == 1) {
                    start1 = true;
                    workelement1 = elemt;
                    used.Add(elemt);
                    line1changed = elemt;
                    //Console.WriteLine("Start detected");
                    if (oldcoords == null) {
                        oldcoords = elemt;
                    }
                    if (oldrot1 == null)
                    {
                        oldrot1 = elemt;
                    }
                }
                if (elemt[0] == 0 && elemt[3] == 2)
                {
                    line1changed = elemt;
                    used.Add(elemt);
                }
                if (elemt[0] == 0 && elemt[3] == 3) {
                    end1 = true;
                    workelement2 = elemt;
                    used.Add(elemt);
                    oldcoords = null;
                    oldrot1 = null;
                    //Console.WriteLine("End detected");
                }
                if (elemt[0] == 1 && elemt[3] == 1)
                {
                    start2 = true;
                    workelement3 = elemt;
                    used.Add(elemt);
                    if (oldrot2 == null)
                    {
                        oldrot2 = elemt;
                    }
                }
                if (elemt[0] == 1 && elemt[3] == 2)
                {
                    line2changed = elemt;
                    used.Add(elemt);
                    
                }
                if (elemt[0] == 1 && elemt[3] == 3)
                {
                    end2 = true;
                    workelement4 = elemt;
                    used.Add(elemt);
                    oldrot2 = null;
                }
            }

            if (start1 && end1 && !start2 && !wastranslating && !wasrotating) {
                //Console.WriteLine("Cordset 1 " + workelement1[4] + " - " + line1changed[4] + " /// " + Math.Abs(workelement1[4] - line1changed[4]));
                //Console.WriteLine("Cordset 2 " + workelement1[5] + " - " + line1changed[5] + " /// " + Math.Abs(workelement1[5] - line1changed[5]));
                if (Math.Abs(workelement1[4] - line1changed[4]) < 0.01 && Math.Abs(workelement1[5] - line1changed[5]) < 0.01)
                {
                    oldcoords = null;
                    Console.WriteLine("Select");
                    TuioInputDeviceSpecialEvent customEvent =
                        new TuioInputDeviceSpecialEvent(this, opType.selection, workelement1[4], workelement1[5]);
                    InputDeviceManager.Instance.SendEvent(customEvent);
                    foreach (float[] usedelemt in used)
                    {
                        tuioInputData.Remove(usedelemt);
                    }
                }
            }
            else if ((start1 && start2) || wasrotating)
            {
                //Console.WriteLine("Cordset 1 " + line1changed[4] + " | " + line2changed[4] + " /// " + (line2changed[4] - line1changed[4]));
                //Console.WriteLine("Cordset 2 " + line1changed[5] + " | " + line2changed[5] + " /// " + (line2changed[5] - line1changed[5]));
                
                float dy = (line2changed[5] - line1changed[5]);
                float dx = (line2changed[4] - line1changed[4]);
                float rotangle = (float)-Math.Atan(dy/dx);
                //float pi = MathFunctions.PI;
                //Console.WriteLine("Angle: " + (rotangle/(2 * pi) *360));

                if (oldangle == 1000) oldangle = rotangle;


                TuioInputDeviceSpecialEvent customEvent =
                new TuioInputDeviceSpecialEvent(this, opType.rotation, rotangle, oldangle);
                InputDeviceManager.Instance.SendEvent(customEvent);
                oldangle = rotangle;
                if (!end1 && !end2)
                {
                    used.Remove(line1changed);
                    used.Remove(line2changed);
                    used.Remove(workelement1);
                    used.Remove(workelement3);
                    wasrotating = true;
                }
                else
                {
                    oldangle = 1000;
                    wasrotating = false;
                    Console.WriteLine("Rotate Clear");
                }

                foreach (float[] usedelemt in used)
                {
                    tuioInputData.Remove(usedelemt);
                }

            }
            else if ((end1 && wastranslating) || !wasrotating && (!start2 && oldcoords != null && (Math.Abs(line1changed[4] - oldcoords[4]) > 0.01 || Math.Abs(line1changed[5] - oldcoords[5]) > 0.01)))
            {
                if (oldcoords != null)
                {
                    //Console.WriteLine("Cordset 1 " + oldcoords[4] + " | " + line1changed[4] + " /// " + (line1changed[4] - oldcoords[4]));
                    //Console.WriteLine("Cordset 2 " + oldcoords[5] + " | " + line1changed[5] + " /// " + (line1changed[5] - oldcoords[5]));
                    TuioInputDeviceSpecialEvent customEvent =
                    new TuioInputDeviceSpecialEvent(this, opType.translation, (line1changed[4] - oldcoords[4]), (line1changed[5] - oldcoords[5]));
                    InputDeviceManager.Instance.SendEvent(customEvent);
                }
                if (!end1)
                {
                    wastranslating = true;
                    used.Remove(workelement1);
                    used.Remove(line1changed);
                    oldcoords = line1changed;
                    Console.WriteLine("Translate");
                } else {
                    wastranslating = false;
                    Console.WriteLine("Translate Clear");
                }
      
                foreach (float[] usedelemt in used)
                {
                    tuioInputData.Remove(usedelemt);
                }


            }

                    //TuioInputDeviceSpecialEvent customEvent =
                    //    new TuioInputDeviceSpecialEvent(this, opType.rotation);
                    //InputDeviceManager.Instance.SendEvent(customEvent);




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
