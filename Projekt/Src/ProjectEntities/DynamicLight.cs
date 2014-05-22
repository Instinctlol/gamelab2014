using Engine.MapSystem;
using Engine.MathEx;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectEntities
{
    public class DynamicLightType : LightType
    { }

    /*
     * Klasse für dynamische Lichter die ein und ausgeschaltet werden können, ggf auch dimmen
     */
    public class DynamicLight : Light
    {
        DynamicLightType _type = null; public new DynamicLightType Type { get { return _type; } }

        //Alternative Lichtfarbe
        [FieldSerialize]
        private ColorValue altDiffuseColor;


        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public ColorValue AltDiffuseColor
        {
            get { return altDiffuseColor; }
            set { altDiffuseColor = value; }
        }
        //***************************

        //Licht an
        public void TurnOn()
        {
            DiffuseColor = AltDiffuseColor;
        }

        //Licht aus
        public void TurnOff()
        {
            DiffuseColor = new ColorValue(0, 0, 0);
        }



        protected override void OnPostCreate(bool loaded)
        {
            AltDiffuseColor = DiffuseColor;
            base.OnPostCreate(loaded);
        }
    }
}
