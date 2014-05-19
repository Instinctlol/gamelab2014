using Engine.MapSystem;
using Engine.MathEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class StaticLightType : LightType
    {
        ColorValue diffuseColor;

        public ColorValue DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }

    }


    public class StaticLight : Light
    {
        StaticLightType _type = null; public new StaticLightType Type { get { return _type; } }

        protected override void OnPostCreate(bool loaded)
        {
            DiffuseColor = Type.DiffuseColor;

            base.OnPostCreate(loaded);
        }
    }
}
