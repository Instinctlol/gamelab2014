using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    class DoubleSwitchSmartButtonWindow : SmartButtonWindow
    {
        enum NetworkMessages
        {
            LightSwitchButtonClick,
            DoorSwitchButtonClick
        }

        public DoubleSwitchSmartButtonWindow(SmartButton button)
            : base(button)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\DoubleSwitchActionGUI.gui");
            ((Button)CurWindow.Controls["LightSwitchButton"]).Click += LightSwitchLeftButton_Click;
            ((Button)CurWindow.Controls["DoorSwitchButton"]).Click += DoorSwitchButton_Click;
            ((Button)CurWindow.Controls["LightSwitchButton"]).Click += SmartClick;
            ((Button)CurWindow.Controls["DoorSwitchButton"]).Click += SmartClick;

            button.Server_WindowDataReceived += Server_WindowDataReceived;
        }

        private void Server_WindowDataReceived(ushort message)
        {
            if (button.IsActive)
            {
                NetworkMessages msg = (NetworkMessages)message;
                switch (msg)
                {
                    case NetworkMessages.LightSwitchButtonClick:
                        button.Terminal.DoRotateLeftEvent();
                        break;
                    case NetworkMessages.DoorSwitchButtonClick:
                        button.Terminal.DoRotateRightEvent();
                        break;
                }
            }
        }

        private void LightSwitchLeftButton_Click(Button sender)
        {
            button.Client_SendWindowData((UInt16)NetworkMessages.LightSwitchButtonClick);
        }

        private void DoorSwitchButton_Click(Button sender)
        {
            button.Client_SendWindowData((UInt16)NetworkMessages.DoorSwitchButtonClick);
        }
    }
}
