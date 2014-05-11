using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class RepairableType : DynamicType
    {
    }

    public class Repairable : Dynamic
    {
        RepairableType _type = null; public new RepairableType Type { get { return _type; } }

        bool repaired = false;



        public delegate void RepairDelegate(Repairable entity);

        [LogicSystemBrowsable(true)]
        public event RepairDelegate Repair;

        protected virtual void OnRepair()
        {
            if (Repair != null)
                Repair(this);
        }


        //TODO: Network, updating physical object
        public bool Repaired
        {
            get { return repaired; }
            set
            {
                if (this.repaired == value)
                    return;

                this.repaired = value;

                OnRepair();

            }
        }

        public void Press()
        {
            Repaired = true;
        }

    }
}
