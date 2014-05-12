using Engine;
using Engine.MapSystem;
using Engine.UISystem;
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

        [FieldSerialize]
        private Repairable repairable = null;

        [FieldSerialize]
        private SmartButton initialButton;

        [FieldSerialize]
        private Task initialTask;

        [FieldSerialize]
        private TerminalTaskType taskType;


        private MapObjectAttachedGui attachedGuiObject;
        private In3dControlManager controlManager;
        private Control mainControl;

        public delegate void TerminalActionDelegate(Terminal entity);

        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalAction;


        public TerminalTaskType TaskType
        {
            get { return taskType;}
            set { 
                taskType = value;

                if (taskType == TerminalTaskType.None)
                {
                    InitialButton = null;
                    InitialTask = null;
                    
                }
                else if (taskType == TerminalTaskType.PIN)
                {
                    InitialButton = new PINStartSmartButton();
                    InitialTask = new PINTask();
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
            set { repairable = value; }
        }

        protected Task InitialTask
        {
            get { return initialTask; }
            set {   
                    initialTask = value;
                    if (initialButton != null)
                    {
                        if (initialTask != null)
                            initialButton.Pressed += new SmartButton.PressedDelegate(initialTask.OnSmartButtonPressed);
                    }
            }
        }

        protected SmartButton InitialButton
        {
            get { return initialButton; }
            set
            {
                if (initialButton != null)
                {
                    initialButton.DetachRepairable(Repairable);
                    if (initialTask != null)
                        initialButton.Pressed -= new SmartButton.PressedDelegate(initialTask.OnSmartButtonPressed);
                }

                initialButton = value;

                if (initialButton != null)
                {
                    initialButton.AttachRepairable(Repairable);
                    if (initialTask != null)
                        initialButton.Pressed += new SmartButton.PressedDelegate(initialTask.OnSmartButtonPressed);
                }

                CreateMainControl();
            }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                attachedGuiObject = attachedObject as MapObjectAttachedGui;
                if (attachedGuiObject != null)
                {
                    controlManager = attachedGuiObject.ControlManager;
                    break;
                }
            }

            if (initialButton != null)
            {
                if(initialTask != null)
                    initialButton.Pressed += new SmartButton.PressedDelegate(initialTask.OnSmartButtonPressed);
                initialButton.AttachRepairable(repairable);
            }
            CreateMainControl();
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

            if (controlManager != null && initialButton != null)
            {
                mainControl = initialButton;
                if (mainControl != null)
                    controlManager.Controls.Add(mainControl);
            }

            //update MapBounds
            SetTransform(Position, Rotation, Scale);
        }
    }
}
