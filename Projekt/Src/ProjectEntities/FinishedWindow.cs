using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace ProjectEntities
{
    /*
    public class FinishedWindow : SmartButtonWindow
    {
        //Textfeld in dem ausgegeben werden soll
        private Button lightSwitchButton, doorSwitchButton, rightButton, leftButton, switchButton;

        private Terminal terminal;

        public FinishedWindow(SmartButton button)
        {
            //GUI erzeugen
            switch(terminal.ActionType)
            {
                case Terminal.TerminalActionType.Switch:
                    {
                        CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\SwitchActionGUI.gui");
                        switchButton = ((Button)CurWindow.Controls["SwitchButton"]);
                        switchButton.Click += SwitchButton_click;
                        break;
                    }
                case Terminal.TerminalActionType.DoubleSwitch:
                    {
                        CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\DoubleSwitchActionGUI.gui");
                        lightSwitchButton = ((Button)CurWindow.Controls["LightSwitchButton"]);
                        doorSwitchButton = ((Button)CurWindow.Controls["DoorSwitchButton"]);
                        lightSwitchButton.Click += LightSwitchButton_click;
                        doorSwitchButton.Click += DoorSwitchButton_click;
                        break;
                    }
                case Terminal.TerminalActionType.RotateAndDoubleSwitch:
                    {
                        CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\RotateAndDoubleSwitchActionGUI.gui");
                        leftButton = ((Button)CurWindow.Controls["LeftButton"]);
                        rightButton = ((Button)CurWindow.Controls["RightButton"]);
                        lightSwitchButton = ((Button)CurWindow.Controls["LightSwitchButton"]);
                        doorSwitchButton = ((Button)CurWindow.Controls["DoorSwitchButton"]);
                        leftButton.Click += RotateLeftButton_click;
                        rightButton.Click += RotateRightButton_click;
                        lightSwitchButton.Click += LightSwitchButton_click;
                        doorSwitchButton.Click += DoorSwitchButton_click;
                        break;
                    }
                case Terminal.TerminalActionType.RotateAndSingleSwitch:
                    {
                        CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\RotateAndSingleSwitchActionGUI.gui");
                        leftButton = ((Button)CurWindow.Controls["LeftButton"]);
                        rightButton = ((Button)CurWindow.Controls["RightButton"]);
                        switchButton = ((Button)CurWindow.Controls["SwitchButton"]);
                        leftButton.Click += RotateLeftButton_click;
                        rightButton.Click += RotateRightButton_click;
                        switchButton.Click += SwitchButton_click;
                        break;
                    }
                case Terminal.TerminalActionType.Rotation:
                    {
                        CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\RotateActionGUI.gui");
                        leftButton = ((Button)CurWindow.Controls["LeftButton"]);
                        rightButton = ((Button)CurWindow.Controls["RightButton"]);
                        leftButton.Click += RotateLeftButton_click;
                        rightButton.Click += RotateRightButton_click;
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

            this.terminal = terminal;
        }

        private void RotateLeftButton_click(Button sender)
        {
            terminal.OnTerminalFinishedDoRotateLeft();
        }

        private void RotateRightButton_click(Button sender)
        {
            terminal.OnTerminalFinishedDoRotateRight();
        }

        private void LightSwitchButton_click(Button sender)
        {
            terminal.OnTerminalFinishedDoLight();
        }

        private void DoorSwitchButton_click(Button sender)
        {
            terminal.OnTerminalFinishedDoDoor();
        }

        private void SwitchButton_click(Button sender)
        {
            terminal.OnTerminalFinishedDoSwitch();
        }
    }*/
}
