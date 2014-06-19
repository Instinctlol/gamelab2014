using Engine.EntitySystem;
using Engine.MathEx;
using Engine.UISystem;
using ProjectCommon;
using System;
namespace ProjectEntities
{
    public class SectorStatusWindow : Window
    {
        private EngineConsole console = EngineConsole.Instance;

        private Ring ringOuter, ringInner, ringMiddle;
        private RotControl ringOuterCntrl, ringInnerCntrl, ringMiddleCntrl;
        Control window, ringFullCntrl;

        [Engine.EntitySystem.EntityType.FieldSerialize]
        private float scale;

        public float Scale
        {
            get { return scale; }
            set { scale = value; scaleAllRings(value); }
        }

        public SectorStatusWindow()
        {
            window = ControlDeclarationManager.Instance.CreateControl("GUI\\Minimap\\Minimap.gui");
            Controls.Add(window);
            ringFullCntrl = (Control)window.Controls["FullRing"];

            ringInnerCntrl = (RotControl)ringFullCntrl.Controls["InnerRing"];
            ringMiddleCntrl = (RotControl)ringFullCntrl.Controls["MiddleRing"];
            ringOuterCntrl = (RotControl)ringFullCntrl.Controls["OuterRing"];
            if (ringFullCntrl.Size.Value.X == ringFullCntrl.Size.Value.Y)
            {
                scale = ringFullCntrl.Size.Value.X;
            }
            else
                throw new Exception("Scale of the FullRing Control is not quadratic");
        }

        public void scaleAllRings(float size)
        {
            ringInnerCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            ringMiddleCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            ringOuterCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            ringFullCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            window.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
        }

        public void initialize()
        {
            ringOuter = ((Ring)Entities.Instance.GetByName("F1_Ring"));
            ringInner = ((Ring)Entities.Instance.GetByName("F3_Ring"));
            ringMiddle = ((Ring)Entities.Instance.GetByName("F2_Ring"));

            
            ringOuter.RotateRing += OnOuterRotation;
            ringInner.RotateRing += OnInnerRotation;
            ringMiddle.RotateRing += OnMiddleRotation;
        }


        private void OnInnerRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot)
        {
            if (rot.W > 0)
                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree + 45) % 360;
            else if (rot.W < 0)
                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree - 45) % 360;
        }

        private void OnMiddleRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot)
        {
            if (rot.W > 0)
                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree + 45) % 360;
            else if (rot.W < 0)
                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree - 45) % 360;
        }

        private void OnOuterRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot)
        {
            if (rot.W > 0)
                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree + 45) % 360;
            else if (rot.W < 0)
                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree - 45) % 360;
        }

    }
}
