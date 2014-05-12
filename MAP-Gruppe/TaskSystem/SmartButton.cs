using Engine;
using Engine.MathEx;
using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class SmartButton : Control
    {


        public delegate void PressedDelegate(SmartButton entity);

        [LogicSystemBrowsable(true)]
        public event PressedDelegate Pressed;

        protected void SmartClick( Button b )
        {
            if (Pressed != null)
            {
                Pressed(this);
                Enable = false;
            }
        }

        public void AttachRepairable(Repairable r)
        {
            if (r != null)
            {
                r.Repair += new Repairable.RepairDelegate(OnRepair);
                OnRepair(r);
            }
        }

        public void DetachRepairable(Repairable r)
        {
            if (r != null)
            {
                r.Repair -= new Repairable.RepairDelegate(OnRepair);
                OnRepair(r);
            }
        }


        public void OnRepair(Repairable r)
        {
            if (r.Repaired)
            {
                Enable = true;
                Visible = true;
            }
            else
            {
                Enable = false;
                Visible = false;
            }
        }


    }
}
