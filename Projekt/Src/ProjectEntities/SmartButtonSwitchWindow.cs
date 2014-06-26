using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    class SmartButtonSwitchWindow : SmartButtonWindow
    {
        enum NetworkMessages
        {
            SwitchButtonClick,
        }

        public SmartButtonSwitchWindow(SmartButton button)
            : base(button)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\SwitchActionGUI.gui");
            ((Button)CurWindow.Controls["SwitchButton"]).Click += SwitchButton_Click;
            ((Button)CurWindow.Controls["SwitchButton"]).Click += SmartClick;

            button.Server_WindowDataReceived += Server_WindowDataReceived;
        }

        private void Server_WindowDataReceived(ushort message)
        {
            if (button.IsActive)
            {
                NetworkMessages msg = (NetworkMessages) message;
                switch (msg)
                {
                    case NetworkMessages.SwitchButtonClick:
                        button.Terminal.DoSwitchEvent();
                        break;
                }
            }
                
        }

        private void SwitchButton_Click(Button sender)
        {
            button.Client_SendWindowData((UInt16)NetworkMessages.SwitchButtonClick);
        }

        


    }
}
