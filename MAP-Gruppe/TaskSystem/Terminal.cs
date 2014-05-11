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

        [FieldSerialize]
        private Repairable repairable = null;

        [FieldSerialize]
        private SmartButton initialButton = new SmartButton();



        private MapObjectAttachedGui attachedGuiObject;
        private In3dControlManager controlManager;
        private Control mainControl;

        public delegate void TerminalActionDelegate(Terminal entity);

        [LogicSystemBrowsable(true)]
        public event TerminalActionDelegate TerminalAction;

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

        /*
        [Editor(typeof(EditorGuiUITypeEditor), typeof(UITypeEditor))]
        public string InitialButton
        {
            get { return initialButton; }
            set
            {
                if (initialButton != null)
                {
                    initialButton.DetachRepairable(Repairable);
                    initialButton.Pressed -= new SmartButton.PressedDelegate(OnButtonPressed);
                }

                initialButton = value;

                if (initialButton != null)
                {
                    initialButton.AttachRepairable(Repairable);
                    initialButton.Pressed += new SmartButton.PressedDelegate(OnButtonPressed);
                }

                CreateMainControl();
            }
        }*/

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
            initialButton.Pressed += new SmartButton.PressedDelegate(OnButtonPressed);
            initialButton.AttachRepairable(repairable);
            CreateMainControl();
        }

        protected override void OnDestroy()
        {
            mainControl = null;
            controlManager = null;
            base.OnDestroy();
        }

        public void OnButtonPressed(SmartButton b)
        {
            if (TerminalAction != null)
                TerminalAction(this);
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
