using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WiimoteLib;
using ProjectCommon;
using Engine;
using Engine.MathEx;
using GestureLib;
using System.Timers;
using System.Threading;




//using GestureLib.Implementation;

namespace ProjectCommon
{
    public class WiiManager
    {

        static WiiManager instanceWM;

        //WiimoteCollection wmc = null;
        bool wiiMoteInitialized = false;
        public Wiimote wm = new Wiimote();
        public WiimoteState lastState = null;
        bool useWiiMote = true; // 
        bool nonUse = true;
        private Object Lock = new Object();

        public static bool openInventory = false;

        private delegate void UpdateWiimoteStateDelegate(WiimoteChangedEventArgs args);
        private delegate void UpdateExtensionChangedDelegate(WiimoteExtensionChangedEventArgs args);

        public delegate void RecordingFinishedDelegate();
        public static GestureLib.GestureLib gl;


        public static WiiManager InstanceWM
        {

            get { return instanceWM; }

        }

        public static void InitMote()
        {
            System.Console.WriteLine("Starting initMote");

            instanceWM = new WiiManager();

            if (instanceWM.useWiiMote)
            {
                try
                {
                    InstanceWM.wm = new Wiimote();
                    InstanceWM.lastState = new WiimoteState();
                    //wm.SetReportType(InputReport.IRAccel, true);
                    InstanceWM.wm.Connect();
                    InstanceWM.wiiMoteInitialized = true;
                    InstanceWM.wm.SetLEDs(true, true, false, false);
                    InstanceWM.lastState = InstanceWM.wm.WiimoteState;
                    InstanceWM.wm.SetReportType(InputReport.ButtonsAccel, true);// gestenerkennung
                    //InstanceWM.wm.SetReportType(InputReport.IRAccel, true);
                    //InstanceWM.wm.SetReportType(InputReport.Status, true);
                    InstanceWM.wm.SetReportType(InputReport.ButtonsExtension, true); // steuerung

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



            //initialisierung von Gesten & Aktionen
            #region Algorithms
            IGestureAlgorithm shakingTopBottom = new ShakingTopBottomGestureAlgorithm();
            IGestureAlgorithm shakingLeftRight = new ShakingLeftRightGestureAlgorithm();
            IGestureAlgorithm leftRight = new LeftRightAccelerationGestureAlgorithm();
            IGestureAlgorithm rightLeft = new RightLeftAccelerationGestureAlgorithm();
            IGestureAlgorithm roll = new RollGestureAlgorithm();
            IGestureAlgorithm jab = new JabGestureAlgorithm();
            IGestureAlgorithm pitch = new PitchGestureAlgorithm();
            #endregion

            #region Actions

            //string applicationPath = GetApplicationPath();
            //System.IO.TextWriter debugWriter = new System.IO.StreamWriter(System.IO.Path.Combine(applicationPath, "debug.txt"));



            //IGestureAction debugStream = new DebugStreamGestureAction()
            //{
            //    DebugMessage = "My Debug-Test",
            //    WriteNewLine = true,
            //    Output = debugWriter,
            //    AutoFlush = true
            //};

            //IGestureAction fileExecuteCalc = new FileExecuteGestureAction()
            //{
            //    StartInfo = new System.Diagnostics.ProcessStartInfo("calc.exe")
            //};

            //IGestureAction fileExecuteNotepad = new FileExecuteGestureAction()
            //{
            //    StartInfo = new System.Diagnostics.ProcessStartInfo("notepad.exe")
            //};

            //IGestureAction shakeWindowHorizontal = new ShakeWindowAnimatedAction()
            //{
            //    MaxLoops = 15,
            //    Window = this,
            //    VerticalDifference = 5,
            //    Direction = ShakeWindowAnimatedAction.ShakeDirection.Horizontal
            //};

            //IGestureAction shakeWindowVertical = new ShakeWindowAnimatedAction()
            //{
            //    MaxLoops = 15,
            //    Window = this,
            //    VerticalDifference = 5,
            //    Direction = ShakeWindowAnimatedAction.ShakeDirection.Vertical
            //};
            #endregion

            //instanciate GestureLib
            gl = new GestureLib.GestureLib();

            gl.Recording.RecordingStarted += new EventHandler(Recording_RecordingStarted);
            gl.Recording.RecordingFinished += new EventHandler(Recording_RecordingFinished);


            #region Add algorithms and actions
            gl.AvailableGestureAlgorithms.Add(shakingTopBottom);
            gl.AvailableGestureAlgorithms.Add(shakingLeftRight);
            gl.AvailableGestureAlgorithms.Add(leftRight);
            gl.AvailableGestureAlgorithms.Add(rightLeft);
            gl.AvailableGestureAlgorithms.Add(roll);
            gl.AvailableGestureAlgorithms.Add(jab);
            gl.AvailableGestureAlgorithms.Add(pitch);

            //gl.AvailableGestureActions.Add(debugStream);
            //gl.AvailableGestureActions.Add(fileExecuteCalc);
            //gl.AvailableGestureActions.Add(fileExecuteNotepad);
            //gl.AvailableGestureActions.Add(shakeWindowHorizontal);
            //gl.AvailableGestureActions.Add(shakeWindowVertical);
            #endregion


            #region Add trained Gestures

            //TrainedGesture leftRightGesture = new TrainedGesture();
            //leftRightGesture.GestureActions.Add(debugStream);
            //leftRightGesture.GestureAlgorithms.Add(shakingLeftRight);
            //leftRightGesture.Name = "LeftRightGesture";

            //gl.TrainedGestures.Add(leftRightGesture);



            //TrainedGesture topDownGesture = new TrainedGesture();
            //topDownGesture.GestureActions.Add(shakeWindowVertical);
            //topDownGesture.GestureAlgorithms.Add(shakingTopBottom);
            //topDownGesture.Name = "TopDownGesture";

            //gl.TrainedGestures.Add(topDownGesture);

            //TrainedGesture leftRightGesture = new TrainedGesture();
            //leftRightGesture.GestureActions.Add(shakeWindowHorizontal);
            //leftRightGesture.GestureAlgorithms.Add(shakingLeftRight);
            //leftRightGesture.Name = "LeftRightGesture";

            //gl.TrainedGestures.Add(leftRightGesture);
            /*
            TrainedGesture leftRightGesture = new TrainedGesture();
            leftRightGesture.GestureActions.Add(fileExecuteNotepad);
            leftRightGesture.GestureActions.Add(fileExecuteCalc);
            leftRightGesture.GestureAlgorithms.Add(shakingLeftRight);
            leftRightGesture.Name = "LeftRightTest";

            _gl.TrainedGestures.Add(leftRightGesture);*/

            /*TrainedGesture topDownPointerGesture = new TrainedGesture();
            topDownPointerGesture.GestureActions.Add(shakeWindow);
            topDownPointerGesture.GestureAlgorithms.Add(pointerTopBottom);
            topDownPointerGesture.GestureAlgorithms.Add(pointerBottomTop);
            topDownPointerGesture.Name = "TopBottomBottomTop";

            _gl.TrainedGestures.Add(topDownPointerGesture);*/
            #endregion


            gl.GestureDevice = CreateWiiDevice();
            gl.Recording.EventFilterNumber = 1;

        }

        public static WiiGestureDevice CreateWiiDevice()
        {
            WiiGestureDevice wiiDevice = new WiiGestureDevice();
            //InstanceWM.wm.SetReportType(InputReport.ButtonsAccel, true);
            wiiDevice.Wiimote = instanceWM.wm;
            return wiiDevice;
        }



        public static void RecordingFinished()
        {
            if (InstanceWM.nonUse)
            {

                lock (InstanceWM.Lock)
                {
                    InstanceWM.nonUse = false;
                    PointTendenceAnalyzer pointTendenceAnalyzer = gl.Recording.RecognizeRecording(gl.Recording.RecordedPointerGestureStates);
                    GestureAlgorithmCollection matchedPointerAlgorithmes = pointTendenceAnalyzer.MatchedGestureAlgorithms;
                    TrainedGesture trainedGesture = gl.TrainedGestures.GetTrainedGestureByMatchedAlgorithms(matchedPointerAlgorithmes);

                    if (trainedGesture != null)
                    {
                        trainedGesture.GestureActions.ForEach(ga => ga.Execute());
                    }

                    ////////////////////////////////////////////////////////////////////////

                    GestureAlgorithmCollection matchedAccelerationAlgorithmes = gl.Recording.RecognizeRecording(gl.Recording.RecordedAccelerationGestureStates);
                    System.Diagnostics.Debug.WriteLine(matchedAccelerationAlgorithmes + ""); // zu "send message to HUD" um schreiben


                    string tmp = "";
                    foreach (IGestureAlgorithm algo in matchedAccelerationAlgorithmes)
                    {
                        tmp = " " + algo.ToString();
                    }

                    System.Console.WriteLine(tmp + matchedAccelerationAlgorithmes.Count());



                    trainedGesture = gl.TrainedGestures.GetTrainedGestureByMatchedAlgorithms(matchedAccelerationAlgorithmes);

                    if (trainedGesture != null)
                    {
                        trainedGesture.GestureActions.ForEach(ga => ga.Execute());
                    }
                    InstanceWM.nonUse = true;
                }
            }

        }



        static void Recording_RecordingFinished(object sender, EventArgs e)
        {
            System.Console.WriteLine("Recording Finished");
            RecordingFinished();
        }

        static void Recording_RecordingStarted(object sender, EventArgs e)
        {
            System.Console.WriteLine("Recording Startet"); //System.Diagnostics.Debug.WriteLine("GestureRecording started");
            InstanceWM.wm.SetReportType(InputReport.ButtonsExtension, false); // steuerung
            InstanceWM.wm.SetReportType(InputReport.ButtonsAccel, true);// gestenerkennung
        }


        // WiiMotes enkoppeln
        public static void closeMotes()
        {
            if (instanceWM.wiiMoteInitialized)
            {
                instanceWM.wm.SetLEDs(false, false, false, false);
                instanceWM.wm.Disconnect();
            }

        }


        // Ereignisse 


        public static void UpdateWiimoteChanged()
        {
            //WiimoteState ws = args.WiimoteState;

            //InstanceWM.wm.SetReportType(InputReport.ButtonsAccel, false);// gestenerkennung
            //InstanceWM.wm.SetReportType(InputReport.ButtonsExtension, true); // steuerung


            if (InstanceWM.wiiMoteInitialized)
            {
                
                WiimoteState wiiState = InstanceWM.wm.WiimoteState;
                //const float nunchuckThreshold = 0.1f;
                //const float nunchuckScale = 0.01f;
                
                //float nunchuckX = wiiState.NunchukState.Joystick.X; // [-0.5, 0.5], left negative, right positive
                //float nunchuckY = wiiState.NunchukState.Joystick.Y; // [-0.5, 0.5], bottom negative, top positive
                //Console.WriteLine(nunchuckX + "-" + nunchuckY);
                bool buttonUpPressed = wiiState.ButtonState.Up;
                bool buttonDownPressed = wiiState.ButtonState.Down;
                bool buttonLeftPressed = wiiState.ButtonState.Left;
                bool buttonRightPressed = wiiState.ButtonState.Right;
                bool buttonAPressed = wiiState.ButtonState.A;
                bool buttonHomePressed = wiiState.ButtonState.Home;
                bool buttonPlusPressed = wiiState.ButtonState.Plus;
                bool buttonMinusPressed = wiiState.ButtonState.Minus;

                

                if (buttonHomePressed /*&& !buttonUpPressedOld*/)
                {
                    GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.I));
                }
                else /*if (!buttonUpPressed && buttonUpPressedOld)*/
                {
                    GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.I));
                }

                if (buttonPlusPressed && openInventory  /*&& !buttonUpPressedOld*/)
                {
                    GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.P));
                }
                else if (!buttonPlusPressed && openInventory)
                {
                    GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.P));
                }

                if (buttonMinusPressed && openInventory /*&& !buttonUpPressedOld*/)
                {
                    GameControlsManager.Instance.DoKeyDown(new KeyEvent(EKeys.O));
                }
                else if (!buttonMinusPressed && openInventory)
                {
                    GameControlsManager.Instance.DoKeyUp(new KeyEvent(EKeys.O));
                }
               

                

                InstanceWM.lastState = wiiState;
            }

        }

        public static string GetApplicationPath()
        {
            string startupFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
            int lastIndex = startupFile.LastIndexOf('\\');
            startupFile = startupFile.Substring(0, lastIndex);

            return startupFile;
        }


    }
}
