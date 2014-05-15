using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class SmartButtonWindow : Window
    {
        private SmartButton button;

        public SmartButtonWindow(SmartButton button)
        {
            this.button = button;
        }

        protected void SmartClick(Button sender)
        {
            button.SmartButtonPressed();
        }

    }
}
