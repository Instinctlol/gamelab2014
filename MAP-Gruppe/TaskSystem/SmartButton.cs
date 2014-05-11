using Engine;
using Engine.MathEx;
using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class SmartButton : Button
    {

        public delegate void PressedDelegate(SmartButton entity);

        [LogicSystemBrowsable(true)]
        public event PressedDelegate Pressed;



        protected override void OnAttach()
        {
            base.OnAttach();


            SoundClick = "Sounds/ButtonClick.ogg";
            SoundMouseOver = "Sounds/ButtonMouseIntoArea.ogg";

            this.Click += SmrtClick;
        }

        void SmrtClick( Button b )
        {
            if (Pressed != null)
                Pressed(this);
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
