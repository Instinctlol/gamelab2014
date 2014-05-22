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


        public enum TerminalTaskType
        {
            None,
            PIN
        };

        public enum TerminalSmartButtonType
        {
            None,
            Default
        };

        [FieldSerialize]
        private Repairable repairable = null;

        [FieldSerialize]
        private Window initialWindow;

        [FieldSerialize]
        private string taskData;

        [FieldSerialize]
        private TerminalSmartButtonType buttonType;

        [FieldSerialize]
        private TerminalTaskType taskType;


        private MapObjectAttachedGui attachedGuiObject;
        private In3dControlManager controlManager;
        private Control mainControl;

        private Task task;

        

        

        public delegate void TerminalActionDelegate(Terminal entity);

        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalAction;


        public string TaskData
        {
            get { return taskData; }
            set { taskData = value; }
        }

        public Task Task
        {
            get { return task; }
            set { task = value; }
        }
        private SmartButton button;

        public SmartButton Button
        {
            get { return button; }
            set { button = value; }
        }

        public TerminalTaskType TaskType
        {
            get { return taskType;}
            set { 
                taskType = value;
            }
        }

        public TerminalSmartButtonType ButtonType
        {
            get { return buttonType; }
            set { 
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


        [Browsable(false)]
        [LogicSystemBrowsable(true)]
        public In3dControlManager ControlManager
        {
            get { return controlManager; }
        }

        [Browsable(false)]
        [LogicSystemBrowsable(true)]
        public Control MainControl
        {
            get { return mainControl; }
            set { mainControl = value; }
        }

        public Repairable Repairable
        {
            get { return repairable; }
            set { 
                if(repairable != null)
                    button.DetachRepairable(repairable);
                
                repairable = value;
                if (repairable != null)
                    button.AttachRepairable(repairable);
            }
        }


        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            button = new SmartButton(this);
            if (repairable != null)
                button.AttachRepairable(repairable);
            task = new Task(this);
            task.TaskFinished += OnTaskFinished;
            button.RefreshButton();
            

            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                attachedGuiObject = attachedObject as MapObjectAttachedGui;
                if (attachedGuiObject != null)
                {
                    controlManager = attachedGuiObject.ControlManager;
                    break;
                }
            }

            CreateMainControl();
        }

        private void OnTaskFinished(Task task)
        {
            Window = button.Window;
            if (task.Success)
                TaskSuccessful();
            else
                TaskFailed();
        }

        protected override void OnDestroy()
        {
            mainControl = null;
            controlManager = null;
            base.OnDestroy();
        }


        void CreateMainControl()
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

        public Window Window 
        { 
            get { return initialWindow; } 
            set { 
                initialWindow = value;
                CreateMainControl();
            } 
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
