using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace ProjectEntities
{
    public class LightActionWindow : Window
    {
        //Textfeld in dem ausgegeben werden soll
        private TextBox countdownBox;
        private Button switchButton;
        private Sector currSector;

        public LightActionWindow(Terminal terminal)
        {
            //GUI erzeugen
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\LightActionGUI.gui");

            countdownBox = (TextBox)CurWindow.Controls["Output"];
            switchButton = ((Button)CurWindow.Controls["SwitchButton"]);

            //Methode anmelden
            switchButton.Click += SwitchButton_click;

            currSector = StationSystem.Instance.GetSector(terminal.Position);
        }

        private void SwitchButton_click(Button sender)
        {
            currSector.SwitchLights(!currSector.LightStatus);
        }
    }
}
