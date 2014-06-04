using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace ProjectEntities
{
    public class FinishedWindow : Window
    {
        //Textfeld in dem ausgegeben werden soll
        private Button switchButton;

        private Terminal terminal;

        public FinishedWindow(Terminal terminal)
        {
            //GUI erzeugen
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\LightActionGUI.gui");
            switchButton = ((Button)CurWindow.Controls["SwitchButton"]);

            //Methode anmelden
            switchButton.Click += SwitchButton_click;

            this.terminal = terminal;
        }

        private void SwitchButton_click(Button sender)
        {
            terminal.OnTerminalFinished();
        }
    }
}
