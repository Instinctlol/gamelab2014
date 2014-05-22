using Engine;
using Engine.MathEx;
using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class SmartButton
    {

        private Terminal terminal;
        private SmartButtonWindow window;

        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public Terminal Terminal
        {
            get { return terminal; }
            set { terminal = value; }
        }

        public SmartButtonWindow Window
        {
            get { return window; }
            set
            {
                window = value;
                if (terminal != null)
                    terminal.Window = window;
            }
        }
        //*************************** 

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void PressedDelegate(SmartButton entity);

        [LogicSystemBrowsable(true)]
        public event PressedDelegate Pressed;
        //****************************** 

        public SmartButton(Terminal terminal)
        {
            Terminal = terminal;
        }

        //Aktualisiert den Button
        public void RefreshButton()
        {
            switch (terminal.ButtonType)
            {
                case Terminal.TerminalSmartButtonType.None:
                    Window = null;
                    SmartButtonPressed();
                    break;
                case Terminal.TerminalSmartButtonType.Default:
                    Window = new DefaultSmartButtonWindow(this);
                    break;
                default:
                    Window = null;
                    SmartButtonPressed();
                    break;
            }
        }

        public void SmartButtonPressed()
        {
            if (Pressed != null)
            {
                Pressed(this);
            }
        }

        public void AttachRepairable(Repairable r)
        {
            if (r != null)
            {
                r.Repair += new Repairable.RepairDelegate(OnRepair);
                OnRepair(r);
            }
        }

        public void DetachRepairable(Repairable r)
        {
            if (r != null)
            {
                r.Repair -= new Repairable.RepairDelegate(OnRepair);
                OnRepair(r);
            }
        }


        public void OnRepair(Repairable r)
        {
            if (r.Repaired)
            {
                if (window != null)
                    Window = window;
                else
                    SmartButtonPressed();
            }
            else
            {
                terminal.Window = null;
            }
        }


    }
}
