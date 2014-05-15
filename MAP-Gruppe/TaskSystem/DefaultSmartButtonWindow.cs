using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class DefaultSmartButtonWindow : SmartButtonWindow
    {


        public DefaultSmartButtonWindow(SmartButton button) : base(button)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\SmartButtons\\PINSmartButtonGUI.gui");

            ((Button)CurWindow.Controls["Enter"]).Click += SmartClick;
        }

    }
}
