using Engine;
using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class Task
    {

        private Terminal terminal;
        private TaskWindow window;
        private bool success = false;

        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public Terminal Terminal
        {
            get { return terminal; }
            set { terminal = value; }
        }

        public TaskWindow Window
        {
            get { return window; }
            set
            {
                window = value;
                if (terminal != null)
                    terminal.Window = window;
            }
        }

        public bool Success
        {
            get { return success; }
            set
            {
                success = value;
                if( TaskFinished != null)
                     TaskFinished(this);
            }
        }
        //***************************

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void TaskFinishedDelegate(Task entity);

        [LogicSystemBrowsable(true)]
        public event TaskFinishedDelegate TaskFinished;
        //******************************

        public Task(Terminal terminal)
        {
            Terminal = terminal;
            if(terminal != null)
                //beim smartbutton des Terminals unterschreiben
                terminal.Button.Pressed += new SmartButton.PressedDelegate(OnButtonPressed);
        }

        //Wenn der Button gedrückt wird Task erzeugen
        private void OnButtonPressed(SmartButton entity)
        {
            switch (terminal.TaskType)
            {
                case Terminal.TerminalTaskType.None:
                    Window = null;
                    break;
                case Terminal.TerminalTaskType.PIN:
                    Window = new PINTaskWindow(this);
                    break;
                default:
                    Window = null;
                    break;
            }
        }
    }
}
