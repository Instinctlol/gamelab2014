using Engine;
using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class Task : Control
    {


        private bool success = false;

        protected override void OnAttach()
        {
 	         base.OnAttach();
             Enable = false;
        }
        

        public bool Success
        {
            get { return success; }
            set { success = value;
            OnTaskFinished();
            }
        }

        public delegate void TaskFinishedDelegate(Task entity);

        [LogicSystemBrowsable(true)]
        public event TaskFinishedDelegate TaskFinished;


        protected virtual void OnTaskFinished()
        {
            if( TaskFinished != null)
                TaskFinished(this);
        }

        public void OnSmartButtonPressed( SmartButton button )
        {
            Enable = true;
        }



    }
}
