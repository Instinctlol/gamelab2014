// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Engine.MathEx;
using TUIO;
using WobbrockLib;
using WobbrockLib.Extensions;

namespace ProjectCommon
{

    public enum opType
    {
        click,
        translation,
        rotation,
        selection,
        blitz,
        iuhr,
        guhr,
        term
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
        public static float oldangle;
        public static bool wastranslating = false, wasrotating = false, wasselecting = false;
        public static bool detectgesturesState = false;
        public static Recognizer.Dollar.Recognizer Recog;
        private float[] failsafe;
        private bool failsafebool = false;
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
            //Gesten einlesen
            Recog = new Recognizer.Dollar.Recognizer();
            Recognizer.Dollar.Recognizer.load(Recog);
            #region muell
            ////buttons
            //Button[] buttons = new Button[ 2 ];
            //buttons[ 0 ] = new Button( JoystickButtons.Button1, 0 );
            
            //buttons[ 1 ] = new Button(JoystickButtons.Button2, 1);

            ////axes
            //Axis[] axes = new Axis[ 1 ];
            //axes[ 0 ] = new JoystickInputDevice.Axis( JoystickAxes.X, new Range( -1, 1 ), false );

            ////povs
            //POV[] povs = new POV[ 0 ];
            ////povs[ 0 ] = new JoystickInputDevice.POV( JoystickPOVs.POV1 );

            ////sliders
            //Slider[] sliders = new Slider[ 0 ];
            ////sliders[ 0 ] = new Slider( JoystickSliders.Slider1 );

            ////forceFeedbackController
            //ForceFeedbackController forceFeedbackController = null;

            ////initialize data
            //InitDeviceData( buttons, axes, povs, sliders, forceFeedbackController );
            #endregion

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
            if (!detectgesturesState)
            {
                #region Navigation
                //Entferne Eintraege aelter als 3 Sekunden
                float timestamp = DateTime.Now.Millisecond + DateTime.Now.Second * 1000 + DateTime.Now.Minute * 60000 + DateTime.Now.Hour * 3600000;
                #region deleteold
                if (tuioInputData.Count != 0)
                {
                    bool delete = true;
                    while (delete)
                    {
                        if (tuioInputData[0][2] + 3000 <= timestamp)
                        {
                            tuioInputData.Remove(tuioInputData[0]);
                            if (tuioInputData.Count != 0)
                            {
                                delete = true;
                            }
                            else
                            {
                                delete = false;
                            }
                        }
                        else
                        {
                            delete = false;
                        }
                    }
                }
                #endregion

                //block 1
                #region vardefs
                List<float[]> used = new List<float[]>();
                bool start1 = false, start2 = false, end1 = false, end2 = false;
                float[] line1changed = new float[8];
                float[] line2changed = new float[8];
                float[] workelement1 = new float[8];
                float[] workelement2 = new float[8];
                float[] workelement3 = new float[8];
                float[] workelement4 = new float[8];
                #endregion

                //Analyse

                #region start stop detection
                foreach (float[] elemt in tuioInputData)
                {
                    if (elemt[0] == 0 && elemt[3] == 1)
                    {
                        #region start 1
                        start1 = true;
                        workelement1 = elemt;
                        used.Add(elemt);
                        line1changed = elemt;
                        //Console.WriteLine("Start detected");
                        if (oldcoords == null)
                        {
                            oldcoords = elemt;
                        }
                        #endregion
                    }
                    if (elemt[0] == 0 && elemt[3] == 2)
                    {
                        #region update 1
                        line1changed = elemt;
                        used.Add(elemt);
                        #endregion
                    }
                    if (elemt[0] == 0 && elemt[3] == 3)
                    {
                        #region end 1
                        end1 = true;
                        workelement2 = elemt;
                        used.Add(elemt);
                        oldcoords = null;
                        //Console.WriteLine("End detected");
                        #endregion
                    }
                    if (elemt[0] == 1 && elemt[3] == 1)
                    {
                        #region start 2
                        start2 = true;
                        workelement3 = elemt;
                        used.Add(elemt);
                        #endregion
                    }
                    if (elemt[0] == 1 && elemt[3] == 2)
                    {
                        #region update 2
                        line2changed = elemt;
                        used.Add(elemt);
                        # endregion
                    }
                    if (elemt[0] == 1 && elemt[3] == 3)
                    {
                        #region end2
                        end2 = true;
                        workelement4 = elemt;
                        used.Add(elemt);
                        #endregion
                    }
                    if (elemt[0] >= 2)
                    {
                        used.Add(elemt);
                        Console.WriteLine("out of range");
                    }
                }
                #endregion
                float d = (float)Math.Sqrt(Math.Pow((line2changed[4] - line1changed[4]), 2) + Math.Pow((line2changed[5] - line1changed[5]), 2));
                //Console.WriteLine(start1 + " " + end1 + " " + start2 + " " + end2 + " " + Math.Abs(workelement1[4] - line1changed[4]));
                if (wastranslating || start1 && !end1 && (!start2 && !end2) && (Math.Abs(workelement1[4] - line1changed[4]) > 0.01 || Math.Abs(workelement1[5] - line1changed[5]) > 0.01))
                {
                    #region Translation
                    if (!end1)
                    {
                        #region send
                        TuioInputDeviceSpecialEvent customEvent =
                        new TuioInputDeviceSpecialEvent(this, opType.translation, (line1changed[4] - oldcoords[4]), (line1changed[5] - oldcoords[5]));
                        InputDeviceManager.Instance.SendEvent(customEvent);
                        #endregion send
                        wastranslating = true;
                        used.Remove(workelement1);
                        used.Remove(line1changed);
                        oldcoords = line1changed;
                        //Console.WriteLine("Translate");
                    }
                    else
                    {
                        wastranslating = false;
                        //Console.WriteLine("Translate Clear");
                    }
                    foreach (float[] usedelemt in used)
                    {
                        tuioInputData.Remove(usedelemt);
                    }
                    #endregion
                }
                else if (wasselecting || (start1 && start2 && d < 0.05f && d > 0f && line2changed[4] != 0f))
                {
                    #region Selection
                    if (!end1 && !end2)
                    {
                        float dx = line1changed[4] + (line2changed[4] - line1changed[4]);
                        float dy = line1changed[5] + (line2changed[5] - line1changed[5]);
                        used.Remove(line1changed);
                        used.Remove(line2changed);
                        used.Remove(workelement1);
                        used.Remove(workelement3);
                        wasselecting = true;
                        #region send
                        TuioInputDeviceSpecialEvent customEvent =
                            new TuioInputDeviceSpecialEvent(this, opType.selection, dx, dy);
                        InputDeviceManager.Instance.SendEvent(customEvent);
                        #endregion
                        Console.WriteLine("Selection");
                    }
                    else
                    {
                        if (end1 && end2)
                        {
                            #region send
                            TuioInputDeviceSpecialEvent customEvent =
                                new TuioInputDeviceSpecialEvent(this, opType.selection, 0f, 0f);
                            InputDeviceManager.Instance.SendEvent(customEvent);
                            #endregion
                            wasselecting = false;
                            Console.WriteLine("Selection Clear");
                        }
                        else
                        {
                            used.Remove(line1changed);
                            used.Remove(line2changed);
                            used.Remove(workelement1);
                            used.Remove(workelement3);
                            if (end1) used.Remove(workelement2);
                            if (end2) used.Remove(workelement4);
                        }
                    }

                    foreach (float[] usedelemt in used)
                    {
                        tuioInputData.Remove(usedelemt);
                    }
                    #endregion
                }
                else if (wasrotating || (start1 && start2 && d >= 0.05f && line2changed[4] != 0f))
                {
                    #region Rotation
                    //Console.WriteLine(d +" @ " + wasrotating + " - " + line1changed[4] + " / " + line1changed[5] + " - " + line2changed[4] + " / " + line2changed[5]);
                    float dy = (line2changed[5] - line1changed[5]);
                    float dx = (line2changed[4] - line1changed[4]);
                    float rotangle = (float)-Math.Atan(dy / dx);

                    if (oldangle == 1000) oldangle = rotangle;

                    #region send
                    TuioInputDeviceSpecialEvent customEvent =
                    new TuioInputDeviceSpecialEvent(this, opType.rotation, rotangle, oldangle);
                    InputDeviceManager.Instance.SendEvent(customEvent);
                    #endregion
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
                        if (end1 && end2)
                        {

                            oldangle = 1000;
                            wasrotating = false;
                            Console.WriteLine("Rotate Clear");
                        }
                        else
                        {
                            used.Remove(line1changed);
                            used.Remove(line2changed);
                            used.Remove(workelement1);
                            used.Remove(workelement3);
                            if (end1) used.Remove(workelement2);
                            if (end2) used.Remove(workelement4);
                        }
                    }

                    foreach (float[] usedelemt in used)
                    {
                        tuioInputData.Remove(usedelemt);
                    }
                    #endregion
                }
                else if (start1 && end1 && !start2 && !end2 && !wastranslating && !wasrotating && !wasselecting && Math.Abs(workelement1[4] - line1changed[4]) <= 0.01f && Math.Abs(workelement1[5] - line1changed[5]) <= 0.01f)
                {
                    #region click
                    #region debugprint
                    //Console.WriteLine(used.Count + " - " + tuioInputData.Count);
                    //foreach (float[] element in tuioInputData)
                    //{
                    //    Console.WriteLine(element[0] + " " + element[3] + " " + element[4] + "/" + element[5] + " @ " + element[2]);
                    //}
                    #endregion
                    #region send
                    TuioInputDeviceSpecialEvent customEvent =
                            new TuioInputDeviceSpecialEvent(this, opType.click, workelement1[4], workelement1[5]);
                    InputDeviceManager.Instance.SendEvent(customEvent);
                    #endregion
                    foreach (float[] usedelemt in used)
                    {
                        tuioInputData.Remove(usedelemt);
                    }

                    #endregion
                }
                //TuioInputDeviceSpecialEvent customEvent =
                //    new TuioInputDeviceSpecialEvent(this, opType.rotation);
                //InputDeviceManager.Instance.SendEvent(customEvent);
                #endregion
            }
            else {
                //Console.WriteLine("Gesture Detection Started");
                int threshold = 100;
                bool startb = false, endb = false;
                float[] start = new float[8], end = new float[8];
                List<float[]> used = new List<float[]>();

                foreach (float[] elemt in tuioInputData)
                {
                    if (elemt[0] == 0 && elemt[3] == 1)
                    {
                        if (failsafebool && failsafe[2] + threshold >= elemt[2] && failsafe[2] < elemt[2])
                        {
                            used.Add(failsafe);
                            used.Add(elemt);
                            failsafebool = false;
                            failsafe = null;
                        }
                        startb = true;
                        start = elemt;
                    }
                    if (elemt[0] == 0 && elemt[3] == 3)
                    {
                        failsafe = elemt;
                        failsafebool = true;
                        endb = true;
                        end = elemt;
                    }

                }
                if (used.Count > 0) {
                    foreach (float[] usedelemt in used)
                    {
                        tuioInputData.Remove(usedelemt);
                    }
                }
                float timestamp = DateTime.Now.Millisecond + DateTime.Now.Second * 1000 + DateTime.Now.Minute * 60000 + DateTime.Now.Hour * 3600000;
                if (startb && endb && Math.Abs(start[4] - end[4]) <= 0.01f && Math.Abs(start[5] - end[5]) <= 0.01f)
                {
                    TuioInputDeviceSpecialEvent customEvent =
                    new TuioInputDeviceSpecialEvent(this, opType.click, end[4], end[5]);
                    InputDeviceManager.Instance.SendEvent(customEvent);
                }
                else if (failsafebool && failsafe[2] + threshold < timestamp)
                {
 
                        //Daten konvertieren
                        List<TimePointF> data = new List<TimePointF>();
                        foreach (float[] elemt in tuioInputData)
                        {
                            if (elemt[0] == 0 && elemt[3] != 3)
                                data.Add(new TimePointF(elemt[4], elemt[5], elemt[2]));
                        }

                        //Gesten erkennen
                        Recognizer.Dollar.NBestList list = new Recognizer.Dollar.NBestList();
                        list = Recog.Recognize(data, false);
                        TuioInputDeviceSpecialEvent customEvent =
                                    new TuioInputDeviceSpecialEvent(this, opType.click, end[4], end[5]);
                        switch (list.Name) {
                            case "blitz": {
                                customEvent = new TuioInputDeviceSpecialEvent(this, opType.blitz, 0, 0);
                                break;
                            }
                            case "term":
                                {
                                    customEvent = new TuioInputDeviceSpecialEvent(this, opType.term, 0, 0);
                                    break;
                                }
                            case "iuhr":
                                {
                                    customEvent = new TuioInputDeviceSpecialEvent(this, opType.iuhr, 0, 0);
                                    break;
                                }
                            case "guhr":
                                {
                                    customEvent = new TuioInputDeviceSpecialEvent(this, opType.guhr, 0, 0);
                                    break;
                                }
                            default:
                                {
                                    customEvent = new TuioInputDeviceSpecialEvent(this, opType.click, end[4], end[5]);
                                    break;
                                }
                            
                        }
                        InputDeviceManager.Instance.SendEvent(customEvent);
                        Console.WriteLine(list.Name);
                        tuioInputData.Clear();
                        detectgestures(false);
                        
                } 
                //else if(startb && endb&& end[2]-start[2]>5000){
                ////abbruch nach 5 sekunden
                //    Console.WriteLine("no Gesture");
                //    tuioInputData.Clear();
                //    detectgestures(false);
                //}
            }


        }


        public static void detectgestures(bool state) {
            detectgesturesState = state;
        }


		/// <summary>
		/// Initialize the device and register them in the InputDeviceManager
		/// </summary>
		public static void InitDevice()
		{
            TuioDump.runTuio();
            #region gestureswitch
            //detectgestures(true);
            #endregion 

            //if (InputDeviceManager.Instance == null) {
            //}
            //    return;
            
			TuioInputDevice device = new TuioInputDevice( "TUIO" );
    
			if( !device.Init() )
				return;

			InputDeviceManager.Instance.RegisterDevice( device );
            Console.WriteLine("Workbench hinzugefügt");
		}
	}
}
