using Engine.MapSystem;
using Engine.MathEx;
using System;
using System.Collections.Generic;
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

            if (sector != null)
                sector.AddDoor(this);

            if (partnerDoor != null)
                partnerDoor.Opened = false;
            Opened = false;
            partnerDoor = null;

            Box bounds = GetBox();
            bounds.Expand(15);

            foreach (MapObject obj in Map.Instance.GetObjects(bounds))
            {
                OutDoor d = obj as OutDoor;
                if (d != null && d != this)
                {
                    partnerDoor = d;
                    d.Opened = true;
                    d.PartnerDoor = this;
                    Opened = true;
                    return;
                }
            }
        }


        public void OnRotate(Vec3 pos, Quat rot, bool left)
        {
            if (partnerDoor != null)
                partnerDoor.Opened = false;
            Opened = false;
            partnerDoor = null;

            Box bounds = GetBox();
            bounds.Expand(15);

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
