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

        [FieldSerialize]
        private bool rotatable = true;

        [FieldSerialize]
        private int id = -1;

        public bool Rotatable
        {
            get { return rotatable; }
            set { rotatable = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public delegate void RotateRingDelegate(Vec3 pos, Quat rot);


        [LogicSystemBrowsable(true)]
        public event RotateRingDelegate RotateRing;


        public Ring() : base()
        {
            base.ShapeType = ShapeTypes.Box;
            base.Filter = Filters.All;
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (loaded && Id >= 0)
                StationSystem.Instance.RegisterRing(this);
        }

        [LogicSystemBrowsable(true)]
        public void Rotate(Quat rot)
        {

            //Vllt Error wenn man versucht statischen ring zu drehen
            if (!Rotatable)
                return;

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
