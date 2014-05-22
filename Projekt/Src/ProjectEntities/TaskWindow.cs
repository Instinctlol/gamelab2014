using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class TaskWindow : Window
    {

        protected Task task;

        public TaskWindow(Task task)
        {
            this.task = task;
        }
    }
}
