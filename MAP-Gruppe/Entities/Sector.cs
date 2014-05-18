using Engine.MapSystem;
using Engine.MathEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;

namespace ProjectEntities
{
    public class SectorType : RegionType
    { }

    public class Sector : Region
    {
        SectorType _type = null; public new SectorType Type { get { return _type; } }

        [FieldSerialize]
        private List<DynamicLight> lights = new List<DynamicLight>();
        [FieldSerialize]
        private bool lightStatus;
        [FieldSerialize]
        private List<Dynamic> dynamics = new List<Dynamic>();
        [FieldSerialize]
        private List<MapObject> statics = new List<MapObject>();

        [FieldSerialize]
        private Ring ring;

        public Ring Ring
        {
            get { return ring; }
            set {
                ring = value; 
            }
        }

        void OnRotateRing(Vec3 pos, Quat rot)
        {

            Rotation = rot * Rotation;
            Vec3 offset = Position - pos;
            Position = rot * offset + pos;

            Quat newRot = Rotation * OldRotation.GetInverse();
            newRot.Normalize();

            foreach (MapObject m in lights)
            {
                m.Rotation = newRot * m.Rotation;
                offset = m.Position - OldPosition;
                m.Position = newRot * offset + Position;
            }

            foreach (MapObject m in dynamics)
            {
                m.Rotation = newRot * m.Rotation;
                offset = m.Position - OldPosition;
                m.Position = newRot * offset + Position;
            }

            foreach (MapObject m in statics)
            {
                m.Rotation = newRot * m.Rotation;
                offset = m.Position - OldPosition;
                m.Position = newRot * offset + Position;
            }
        }



        public Sector() : base()
        {
            base.ShapeType = ShapeTypes.Box;
            base.Filter = Filters.All;
        }

        protected override void OnObjectIn(MapObject obj)
        {
            base.OnObjectIn(obj);

            if (obj is Sector || obj is Ring)
                return;

            if (obj is DynamicLight)
                AddLight((DynamicLight)obj);
            else if (obj is Dynamic)
                AddDynamic((Dynamic)obj);
            else
                AddStatic(obj);
        }

        protected override void OnPostCreate(bool loaded)
        {

            base.OnPostCreate(loaded);


            if(ring != null)
                ring.RotateRing += new ProjectEntities.Ring.RotateRingDelegate(OnRotateRing);
            
        }

        private void AddStatic(MapObject m)
        {
            statics.Add(m);
        }

        private void AddLight(DynamicLight l)
        {
           
        }

        public void ToggleLights()
        {
            lightStatus = !lightStatus;
            SetLights(lightStatus);
        }

        public void SetLights(bool status)
        {
            if(lightStatus != status)
                lightStatus = status;

            if (lightStatus)
                foreach (DynamicLight l in lights)
                {
                    l.DiffuseColor = l.AltDiffuseColor;
                }
            else
                foreach (DynamicLight l in lights)
                {
                    l.DiffuseColor = new ColorValue(0,0,0);
                }
        }


        public void AddDynamic(Dynamic d)
        { }

        public void RemoveDynamic(Dynamic d)
        { }


    }
}
