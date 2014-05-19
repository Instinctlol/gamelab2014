using Engine.MapSystem;
using Engine.MathEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class DynamicLightType : LightType
    { }


    public class DynamicLight : Light
    {
        DynamicLightType _type = null; public new DynamicLightType Type { get { return _type; } }
        [FieldSerialize]
        private ColorValue altDiffuseColor;

        public ColorValue AltDiffuseColor
        {
            get { return altDiffuseColor; }
            set { altDiffuseColor = value; }
        }


        protected override void OnPostCreate(bool loaded)
        {
            AltDiffuseColor = DiffuseColor;
            base.OnPostCreate(loaded);
        }



        public void TurnOn()
        {
            DiffuseColor = AltDiffuseColor;
        }

        public void TurnOff()
        {
            DiffuseColor = new ColorValue(0, 0, 0);
        }
    }
}
