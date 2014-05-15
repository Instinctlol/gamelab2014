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

        private List<DynamicLight> lights;
        private bool lightStatus;
        private List<Dynamic> dynamics;
        private List<MapObject> statics;

        public Sector() : base()
        {
            base.ShapeType = ShapeTypes.Box;
            base.Filter = Filters.All;
        }

        /*
        [Browsable(false)]
        public override ShapeTypes ShapeType
        {
            get { return base.ShapeType; }
            set { base.ShapeType = value; }
        }

        [Browsable(false)]
        public override Filters Filter
        {
            get { return base.Filter; }
            set { base.Filter = value; }
        }
         */

        protected override void OnPostCreate(bool loaded)
        {
            Box box = GetBox();

            MapObject[] result = Map.Instance.GetObjects(box);

            foreach (MapObject m in result)
            {
                if (m is DynamicLight)
                    AddLight((DynamicLight)m);
                else if (m is Dynamic)
                    AddDynamic((Dynamic)m);
                else
                    AddStatic(m);

            }

            base.OnPostCreate(loaded);
        }

        private void AddStatic(MapObject m)
        {
            
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


        public class SectorCollectionEditor : Prot
        {
            public SectorCollectionEditor()
                : base(typeof(List<Sector>))
            { }
        }

    }
}
