using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    class RotateSmartButtonWindow : SmartButtonWindow
    {
        enum NetworkMessages
        {
            RotateLeftButtonClick,
            RotateRightButtonClick,
        }

        public RotateSmartButtonWindow(SmartButton button)
            : base(button)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\ActionWindows\\RotateActionGUI.gui");
            ((Button)CurWindow.Controls["LeftButton"]).Click += RotateLeftButton_Click;
            ((Button)CurWindow.Controls["RightButton"]).Click += RotateRightButton_Click;
            ((Button)CurWindow.Controls["LeftButton"]).Click += SmartClick;
            ((Button)CurWindow.Controls["RightButton"]).Click += SmartClick;
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
