using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    class RotateAndSingleSwitchSmartButtonWindow : SmartButtonWindow
    {
        enum NetworkMessages
        {
            RotateLeftButtonClick,
            RotateRightButtonClick,
            SwitchButtonClick
        }

        public RotateAndSingleSwitchSmartButtonWindow(SmartButton button)
            : base(button)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\RotateAndSingleSwitchActionGUI.gui");
            ((Button)CurWindow.Controls["LeftButton"]).Click += RotateLeftButton_Click;
            ((Button)CurWindow.Controls["RightButton"]).Click += RotateRightButton_Click;
            ((Button)CurWindow.Controls["LeftButton"]).Click += SmartClick;
            ((Button)CurWindow.Controls["RightButton"]).Click += SmartClick;
            ((Button)CurWindow.Controls["SwitchButton"]).Click += SwitchButton_Click;
            ((Button)CurWindow.Controls["SwitchButton"]).Click += SmartClick;

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
                    case NetworkMessages.SwitchButtonClick:
                        button.Terminal.DoSwitchEvent();
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

        private void SwitchButton_Click(Button sender)
        {
            button.Client_SendWindowData((UInt16)NetworkMessages.SwitchButtonClick);
        }


    }
}
