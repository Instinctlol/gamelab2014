using Engine;
using Engine.MapSystem;
using Engine.UISystem;
using Engine.Utils;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;

namespace ProjectEntities
{

    public class TerminalType : DynamicType
    {
    }


    public class Terminal : Dynamic
    {

        TerminalType _type = null; public new TerminalType Type { get { return _type; } }


        //Task typen, spezifizieren welche art von Task ausgeführt wird.
        //None = Task succesfull nach button betätigen
        public enum TerminalTaskType
        {
            None,
            PIN,
            ColorSequence
        };

        //SmartButton type, spezifizieren welche art von Button initial angezeigt wird
        //None = Task wird direkt angezeigt
        public enum TerminalSmartButtonType
        {
            None,
            Default
        };

        public enum TerminalActionType
        {
            Rotation,
            Switch,
            DoubleSwitch,
            RotateAndDoubleSwitch,
            RotateAndSingleSwitch
        }

        //Repairable das zunächst reperiert werden muss
        [FieldSerialize]
        private Repairable repairable = null;

        //Zusätzliche daten die an die Task übergeben werden können
        [FieldSerialize]
        private string taskData;

        //Sound der abgespielt wird, falls ein Task nicht erfolgreich beendet wurde
        [FieldSerialize]
        private string taskFailSound = "Sounds\\taskFail.ogg";

        //Sound der abgespielt wird, falls ein Task erfolgreich beendet wurde
        [FieldSerialize]
        private string taskSuccessSound = "Sounds\\taskSuccess.ogg";
        
        //ButtonType
        [FieldSerialize]
        private TerminalSmartButtonType buttonType;

        //TaskType
        [FieldSerialize]
        private TerminalTaskType taskType;

        [FieldSerialize]
        private TerminalActionType actionType;

        

        //GUI Fenster das auf dem Terminal angezeigt wird
        private Window initialWindow;

        //Angefügtes GUI objekt
        private MapObjectAttachedGui attachedGuiObject;

        //ControlManager zum anzeigen der GUI
        private In3dControlManager controlManager;

        //MainControl, GUI die angezeigt wird
        private Control mainControl;

        //Task die angezeigt werden soll
        private Task task;

        //SmartButton der angezeigt werden soll
        private SmartButton button;

        //***************************
        //*******Getter-Setter*******
        //***************************   
        [LocalizedDescription("A string which some tasks use. \n"+"For PINTasks enter the code e.g. 1234", "TaskData")]
        public string TaskData
        {
            get { return taskData; }
            set { taskData = value; }
        }

        [LocalizedDescription("This will show a different GUI for every ActionType, Logic for the GUI is defined in the LogicClass of this Object", "ActionType")]
        [DefaultValue(TerminalActionType.Switch)]
        public TerminalActionType ActionType
        {
            get { return actionType; }
            set { actionType = value; }
        }

        [LocalizedDescription("Choose what kind of task is to be assigned to this Terminal", "TaskType")]
        public TerminalTaskType TaskType
        {
            get { return taskType; }
            set
            {
                taskType = value;
            }
        }

        [LocalizedDescription("None: Task is directly shown \n"+"Default: You have to click on a button to start and show the task", "ButtonType")]
        public TerminalSmartButtonType ButtonType
        {
            get { return buttonType; }
            set
            {
                buttonType = value;
                button.RefreshButton();

                //Unschön
                if (repairable != null)
                {
                    button.DetachRepairable(repairable);
                    button.AttachRepairable(repairable);
                }

            }
        }

        public Repairable Repairable
        {
            get { return repairable; }
            set
            {
                if (repairable != null)
                    button.DetachRepairable(repairable);

                repairable = value;
                if (repairable != null)
                    button.AttachRepairable(repairable);
            }
        }

        [Browsable(false)]
        public Task Task
        {
            get { return task; }
            set { task = value; }
        }

        [Browsable(false)]
        public SmartButton Button
        {
            get { return button; }
            set { button = value; }
        }

        [Browsable(false)]
        public In3dControlManager ControlManager
        {
            get { return controlManager; }
        }

        [Browsable(false)]
        public Control MainControl
        {
            get { return mainControl; }
            set { mainControl = value; }
        }

        [Browsable(false)]
        public Window Window
        {
            get { return initialWindow; }
            set
            {
                initialWindow = value;
                CreateMainControl();
            }
        }

        [LocalizedDescription("The file name of the sound to play, when a task was failed.", "TaskFailSound")]
        [DefaultValue("Sounds\\taskFail.ogg")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string TaskFailSound
        {
            get { return taskFailSound; }
            set { taskFailSound = value; }
        }

        [LocalizedDescription("The file name of the sound to play, when a task was failed.", "TaskFailSound")]
        [DefaultValue("Sounds\\taskSuccess.ogg")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        public string TaskSuccessSound
        {
            get { return taskSuccessSound; }
            set { taskSuccessSound = value; }
        }
        //*********************************

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void TerminalActionDelegate(Terminal entity);

        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalSwitchAction;
        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalLightAction;
        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalDoorAction;
        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalRotateLeftAction;
        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalRotateRightAction;

        //*****************************

        public void OnTerminalFinishedDoLight()
        {
            if (TerminalLightAction != null)
                TerminalLightAction(this);
        }
        public void OnTerminalFinishedDoDoor()
        {
            if (TerminalDoorAction != null)
                TerminalDoorAction(this);
        }
        public void OnTerminalFinishedDoSwitch()
        {
            if (TerminalSwitchAction != null)
                TerminalSwitchAction(this);
        }
        public void OnTerminalFinishedDoRotateLeft()
        {
            if (TerminalRotateLeftAction != null)
                TerminalRotateLeftAction(this);
        }
        public void OnTerminalFinishedDoRotateRight()
        {
            if (TerminalRotateRightAction != null)
                TerminalRotateRightAction(this);
        }


        //Ausführen beim laden des Objektes
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (!loaded)
                return;

            //SmartButton setzen
            button = new SmartButton(this);


            //Task setzen
            task = new Task(this);
            task.TaskFinished += OnTaskFinished;

            //Button refreshen
            button.RefreshButton();

            //Ggfs repairable an Button weiter reichen
            if (repairable != null)
                button.AttachRepairable(repairable);
            
            //AttachedGUIObjekt finden
            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                attachedGuiObject = attachedObject as MapObjectAttachedGui;
                if (attachedGuiObject != null)
                {
                    controlManager = attachedGuiObject.ControlManager;
                    break;
                }
            }

            //MainControl erzeugen zum anzeigen der GUI
            CreateMainControl();

            SubscribeToTickEvent();
        }

        //Beim zerstören alles wieder rückgängig
        protected override void OnDestroy()
        {
            mainControl = null;
            controlManager = null;
            base.OnDestroy();
        }

        protected override void OnTick()
        {
            base.OnTick();
        }
        
        private void OnTaskFinished(Task task)
        {
            Window = button.Window;
            if (task.Success)
                TaskSuccessful();
            else
                TaskFailed();
        }

        //MainControl erzeugen
        private void CreateMainControl()
        {
            if (mainControl != null)
            {
                mainControl.Parent.Controls.Remove(mainControl);
                mainControl = null;
            }

            if (controlManager != null && initialWindow != null)
            {
                mainControl = initialWindow.CurWindow;
                if (mainControl != null)
                    controlManager.Controls.Add(mainControl);
            }

            //update MapBounds
            SetTransform(Position, Rotation, Scale);
        }



        private void TaskFailed()
        {
            button.RefreshButton();
            SoundPlay3D(TaskFailSound, .5f, false);
        }

        private void TaskSuccessful()
        {
            Window = new FinishedWindow(this);
            SoundPlay3D(TaskSuccessSound, .5f, false);
        }
    }
}
