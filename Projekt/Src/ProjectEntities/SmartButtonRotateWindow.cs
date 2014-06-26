﻿using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    class SmartButtonRotateWindow : SmartButtonWindow
    {
        enum NetworkMessages
        {
            RotateLeftButtonClick,
            RotateRightButtonClick,
        }

        public SmartButtonRotateWindow(SmartButton button)
            : base(button)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\RotateActionGUI.gui");
            ((Button)CurWindow.Controls["LeftButton"]).Click += RotateLeftButton_Click;
            ((Button)CurWindow.Controls["RightButton"]).Click += RotateRightButton_Click;
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

        


    }
}