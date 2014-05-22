using Engine;
using Engine.MapSystem;
using Engine.UISystem;
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
            PIN
        };

        //SmartButton type, spezifizieren welche art von Button initial angezeigt wird
        //None = Task wird direkt angezeigt
        public enum TerminalSmartButtonType
        {
            None,
            Default
        };

        //Repairable das zunächst reperiert werden muss
        [FieldSerialize]
        private Repairable repairable = null;

        //Zusätzliche daten die an die Task übergeben werden können
        [FieldSerialize]
        private string taskData;

        //ButtonType
        [FieldSerialize]
        private TerminalSmartButtonType buttonType;

        //TaskType
        [FieldSerialize]
        private TerminalTaskType taskType;

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
        public string TaskData
        {
            get { return taskData; }
            set { taskData = value; }
        }

        public TerminalTaskType TaskType
        {
            get { return taskType; }
            set
            {
                taskType = value;
            }
        }

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
        //*********************************

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void TerminalActionDelegate(Terminal entity);

        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalAction;
        //*****************************


        //Ausführen beim laden des Objektes
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (!loaded)
                return;

            //SmartButton setzen
            button = new SmartButton(this);

            //Ggfs repairable an Button weiter reichen
            if (repairable != null)
                button.AttachRepairable(repairable);

            //Task setzen
            task = new Task(this);
            task.TaskFinished += OnTaskFinished;

            //Button refreshen
            button.RefreshButton();
            
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
        }

        //Beim zerstören alles wieder rückgängig
        protected override void OnDestroy()
        {
            mainControl = null;
            controlManager = null;
            base.OnDestroy();
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
            EngineConsole.Instance.Print("Failed");
        }

        private void TaskSuccessful()
        {
            EngineConsole.Instance.Print("Successful");
        }
    }
}
