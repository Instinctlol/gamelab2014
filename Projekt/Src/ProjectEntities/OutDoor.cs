using Engine.MapSystem;
using Engine.MathEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class OutDoorType : DoorType
    {

    }


    public class OutDoor : Door
    {
        OutDoorType _type = null; public new OutDoorType Type { get { return _type; } }

        [FieldSerialize]
        private Sector sector;

        

        private OutDoor partnerDoor;

        //***************************
        //*******Getter-Setter*******
        //***************************
        [Browsable(false)]
        public OutDoor PartnerDoor
        {
            get { return partnerDoor; }
            set { partnerDoor = value; }
        }

        public Sector Sector
        {
            get { return sector; }
            set { 
                if(sector != null)
                    sector.RemoveDoor(this);
                sector = value;
                if (sector != null)
                    sector.AddDoor(this);
            }
        }
        //***************************


        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            CheckForPartner();
        }


        public void CheckForPartner()
        {
            if (partnerDoor != null)
                partnerDoor.Opened = false;
            Opened = false;
            partnerDoor = null;

            Box bounds = GetBox();
            bounds.Expand(5);

            foreach( MapObject obj in Map.Instance.GetObjects(bounds) )
            {
                OutDoor d = obj as OutDoor;
                if(d != null && d != this)
                {
                    partnerDoor = d;
                    d.Opened = true;
                    d.PartnerDoor = this;
                    Opened = true;
                    return;
                }
            }

        }

             


    }
}
