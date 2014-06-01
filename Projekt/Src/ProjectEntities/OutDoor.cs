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


        private OutDoor partnerDoor;

        //***************************
        //*******Getter-Setter*******
        //***************************
        public OutDoor PartnerDoor
        {
            get { return partnerDoor; }
            set { partnerDoor = value; }
        }
        //***************************


        public void OnRotate(Vec3 pos, Quat rot)
        {
            if (partnerDoor != null)
                partnerDoor.Opened = false;
            this.Opened = false;
            partnerDoor = null;

            Box bounds = GetBox();
            bounds.Expand(15);

            foreach( MapObject obj in Map.Instance.GetObjects(bounds) )
            {


                OutDoor d = obj as OutDoor;
                if(d != null && d != this)
                {
                    partnerDoor = d;
                    partnerDoor.Opened = true;
                    d.PartnerDoor = this;
                    this.Opened = true;
                    return;
                }
            }

        }

             


    }
}
