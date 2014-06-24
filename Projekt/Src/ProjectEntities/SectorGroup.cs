using Engine;
using Engine.MapSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class SectorGroupType : RegionType
    { }

    public class SectorGroup : Region
    {
        SectorGroupType _type = null; public new SectorGroupType Type { get { return _type; } }

        [FieldSerialize]
        bool lightStatus = true;

        public bool LightStatus
        {
            get { return lightStatus; }
            set
            {
                lightStatus = value;
                DoSwitchLight(value);
            }
        }

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void SwitchLightDelegate(bool status);

        [LogicSystemBrowsable(true)]
        public event SwitchLightDelegate SwitchLight;
        //*****************************

        [LogicSystemBrowsable(true)]
        public void DoSwitchLight(bool status)
        {
            if (SwitchLight != null)
                SwitchLight(status);
            lightStatus = status;
        }
    }
}
