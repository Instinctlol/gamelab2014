using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    class SmartButtonRotateAndDoubleSwitchWindow : SmartButtonWindow
    {

        enum NetworkMessages
        {
            RotateLeftButtonClick,
            RotateRightButtonClick,
            LightSwitchButtonClick,
            DoorSwitchButtonClick,

        }

        public SmartButtonRotateAndDoubleSwitchWindow(SmartButton button)
            : base(button)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\RotateActionGUI.gui");
            ((Button)CurWindow.Controls["LeftButton"]).Click += RotateLeftButton_Click;
            ((Button)CurWindow.Controls["RightButton"]).Click += RotateRightButton_Click;
            ((Button)CurWindow.Controls["LightSwitchButton"]).Click += LightSwitchButton_Click;
            ((Button)CurWindow.Controls["DoorSwitchButton"]).Click += DoorSwitchButton_Click;
            ((Button)CurWindow.Controls["LeftButton"]).Click += SmartClick;
            ((Button)CurWindow.Controls["RightButton"]).Click += SmartClick;

            button.Server_WindowDataReceived += Server_WindowDataReceived;
        }

        private void Server_WindowDataReceived(ushort message)
        {
            if (button.IsActive)
            {
                NetworkMessages msg = (NetworkMessages)message;
                switch (msg)
                {
                    case NetworkMessages.RotateLeftButtonClick:
                        button.Terminal.DoRotateLeftEvent();
                        break;
                    case NetworkMessages.RotateRightButtonClick:
                        button.Terminal.DoRotateRightEvent();
                        break;
                    case NetworkMessages.LightSwitchButtonClick:
                        button.Terminal.DoLightEvent();
                        break;
                    case NetworkMessages.DoorSwitchButtonClick:
                        button.Terminal.DoDoorEvent();
                        break;
                }
            }
        }


        private void RotateLeftButton_Click(Button sender)
        {
            button.Client_SendWindowData((UInt16)NetworkMessages.RotateLeftButtonClick);
        }

        private void RotateRightButton_Click(Button sender)
        {
            button.Client_SendWindowData((UInt16)NetworkMessages.RotateRightButtonClick);
        }

        private void LightSwitchButton_Click(Button sender)
        {
            button.Client_SendWindowData((UInt16)NetworkMessages.LightSwitchButtonClick);
        }

        private void DoorSwitchButton_Click(Button sender)
        {
            button.Client_SendWindowData((UInt16)NetworkMessages.DoorSwitchButtonClick);
        }

        


    }
}
