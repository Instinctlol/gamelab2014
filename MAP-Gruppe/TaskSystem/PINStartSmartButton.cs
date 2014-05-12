using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class PINStartSmartButton : SmartButton
    {
        Control window;

        protected override void OnAttach()
        {
 	        base.OnAttach();

            window = ControlDeclarationManager.Instance.CreateControl( "GUI\\SmartButtons\\PINSmartButtonGUI.gui" );
            Controls.Add(window);

            ((Button)window.Controls["Enter"]).Click += SmartClick;
        }

    }
}
