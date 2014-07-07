using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class Window : Control
    {


        Control curWindow;


        public Control CurWindow
        {
            get { return curWindow; }
            set { curWindow = value; }
        }
    }
}
