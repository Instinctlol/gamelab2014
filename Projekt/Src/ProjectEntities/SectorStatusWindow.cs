using Engine.EntitySystem;
using Engine.UISystem;
namespace ProjectEntities
{
    public class SectorStatusWindow : Window
    {
        private Ring ringOuter, ringInner, ringMiddle;
        private RotControl ringOuterCntrl, ringInnerCntrl, ringMiddleCntrl;

        public SectorStatusWindow()
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\Minimap\\Minimap.gui");
        }

        public void initialize()
        {
            ringOuter = ((Ring)Entities.Instance.GetByName("F1_Ring"));
            ringInner = ((Ring)Entities.Instance.GetByName("F3_Ring"));
            ringMiddle = ((Ring)Entities.Instance.GetByName("F2_Ring"));

            ringInnerCntrl = ((RotControl)CurWindow.Controls["InnerRing"]);
            ringMiddleCntrl = ((RotControl)CurWindow.Controls["MiddleRing"]);
            ringOuterCntrl = ((RotControl)CurWindow.Controls["OuterRing"]);

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
