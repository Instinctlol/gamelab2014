using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.UISystem;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class TaskType : WindowHolderType
    { }


    public class Task : WindowHolder
    {

        TaskType _type = null; public new TaskType Type { get { return _type; } }

        enum NetworkMessages
        {
        }

        private bool success = false;

        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public bool Success
        {
            get { return success; }
            set
            {
                success = value;
                if( TaskFinished != null)
                     TaskFinished(value);
            }
        }

        //***************************

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void TaskFinishedDelegate(bool success);

        [LogicSystemBrowsable(true)]
        public event TaskFinishedDelegate TaskFinished;
        //******************************

        public void SendTaskFinished(bool success)
        {
            IsVisible = false;
            if (TaskFinished != null)
                TaskFinished(success);
        }

        protected override void CreateWindow()
        {
            switch (Terminal.TaskType)
            {
                case Terminal.TerminalTaskType.None:
                    Window = null;
                    break;
                case Terminal.TerminalTaskType.PIN:
                    Window = new PINTaskWindow(this);
                    break;
                case Terminal.TerminalTaskType.ColorSequence:
                    Window = new ColorSequenceTaskWindow(this);
                    break;
                default:
                    Window = null;
                    break;
            }

            if(IsServer)
                Terminal.Button.Pressed += OnButtonPressed;
        }

        //Wenn der Button gedrückt wird Task erzeugen
        private void OnButtonPressed()
        {
            SetTerminalWindow(Window);
            if (Window == null)
                SendTaskFinished(true);
            else
                IsVisible = true;
        }
    }
}
