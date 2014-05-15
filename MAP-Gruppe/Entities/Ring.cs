using Engine.MapSystem;
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
        List<Sector> sectors = new List<Sector>();

        public Ring() : base()
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

        [Editor(typeof (Sector.SectorCollectionEditor),typeof (UITypeEditor))]
        public List<Sector> Sectors
        {
            get { return sectors; }
            set { sectors = value; }
        }


        //TODO: Add parameters and code maybe
        public void Rotate()
        { }

    }
}
