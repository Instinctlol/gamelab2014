using Engine;
using Engine.MapSystem;
using Engine.MathEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;

namespace ProjectEntities
{

    public class RingType : RegionType
    {}
    public class Ring : Region
    {
        RingType _type = null; public new RingType Type { get { return _type; } }


        public delegate void RotateRingDelegate(Vec3 pos, Quat rot);


        [LogicSystemBrowsable(true)]
        public event RotateRingDelegate RotateRing;


        public Ring() : base()
        {
            base.ShapeType = ShapeTypes.Box;
            base.Filter = Filters.All;
        }




        //TODO: Add parameters and code maybe
        [LogicSystemBrowsable(true)]
        public void Rotate(Quat rot)
        {

            Quat newRot = rot * Rotation.GetInverse();
            newRot.Normalize();

            Rotation = rot;

            if(RotateRing != null)
            {
                RotateRing(this.Position, newRot);
            }
        
        }

    }
}
