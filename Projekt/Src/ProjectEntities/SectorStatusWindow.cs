using Engine.EntitySystem;
using Engine.MathEx;
using Engine.UISystem;
using ProjectCommon;
using System;
using System.Collections.Generic;
namespace ProjectEntities
{
    public class SectorStatusWindow : Window
    {
        private EngineConsole console = EngineConsole.Instance;

        private Ring ringOuter, ringInner, ringMiddle;
        private RotControl ringOuterCntrl, ringInnerCntrl, ringMiddleCntrl;
        private RotControl secgrpAR7Cntrl, secgrpAR8Cntrl, secgrpBCntrl, secgrpCCntrl, secgrpDCntrl, secgrpECntrl, secgrpFCntrl, secgrpGCntrl;
        Control window, ringFullCntrl;
        private SectorGroup secgrpA, secgrpB, secgrpC, secgrpD, secgrpE, secgrpF, secgrpG;

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
            //scale setzen
            if (ringFullCntrl.Size.Value.X == ringFullCntrl.Size.Value.Y && ringInnerCntrl.Size.Value.X == ringInnerCntrl.Size.Value.Y
                && ringOuterCntrl.Size.Value.X == ringOuterCntrl.Size.Value.Y && ringMiddleCntrl.Size.Value.X == ringMiddleCntrl.Size.Value.Y
                && ringFullCntrl.Size.Value.X == ringOuterCntrl.Size.Value.X && ringOuterCntrl.Size.Value.X == ringMiddleCntrl.Size.Value.X
                && ringMiddleCntrl.Size.Value.X == ringInnerCntrl.Size.Value.X)
            {
                scale = ringFullCntrl.Size.Value.X;
            }
            else
                throw new Exception("Scale of the FullRing Control is not quadratic");

            secgrpAR7Cntrl = (RotControl)ringFullCntrl.Controls["Lights_Overlay"].Controls["Secgrp_A_R7"];
            secgrpAR8Cntrl = (RotControl)ringFullCntrl.Controls["Lights_Overlay"].Controls["Secgrp_A_R8"];
            secgrpBCntrl = (RotControl)ringFullCntrl.Controls["Lights_Overlay"].Controls["Secgrp_B"];
            secgrpCCntrl = (RotControl)ringFullCntrl.Controls["Lights_Overlay"].Controls["Secgrp_C"];
            secgrpDCntrl = (RotControl)ringFullCntrl.Controls["Lights_Overlay"].Controls["Secgrp_D"];
            secgrpECntrl = (RotControl)ringFullCntrl.Controls["Lights_Overlay"].Controls["Secgrp_E"];
            secgrpFCntrl = (RotControl)ringFullCntrl.Controls["Lights_Overlay"].Controls["Secgrp_F"];
            secgrpGCntrl = (RotControl)ringFullCntrl.Controls["Lights_Overlay"].Controls["Secgrp_G"];

        }

        public void scaleAllRings(float size)
        {
            ringInnerCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            ringMiddleCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            ringOuterCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            ringFullCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));

            secgrpAR7Cntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            secgrpAR8Cntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            secgrpBCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            secgrpCCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            secgrpDCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            secgrpECntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            secgrpFCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
            secgrpGCntrl.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));

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

            // Ringe drehen entsprechend der Computer-Konfig
            //for (int ring = 0; ring < Computer.RingRotations.Length; ring++)
            //{
            //    if (Computer.RingRotations[0] != 0)
            //    {
            //        // Negative Anzahl an Rotierungen heißt links herum wurde gedreht
            //        int direction = (Computer.RingRotations[ring] < 0) ? -1 : 1;
            //        for (int rot = 0; rot < Math.Abs(Computer.RingRotations[ring]); rot++)
            //        {
            //            Vec3 pos = new Vec3();
            //            Quat rotation = new Quat(pos, direction);
            //            switch (ring)
            //            {
            //                case 0:
            //                    OnOuterRotation(pos, rotation);
            //                    break;
            //                case 1:
            //                    OnMiddleRotation(pos, rotation);
            //                    break;
            //                case 2:
            //                    OnInnerRotation(pos, rotation);
            //                    break;
            //            }
            //        }
            //    }
            //}

            secgrpA = ((SectorGroup)Entities.Instance.GetByName("F1SG-A"));
            secgrpB = ((SectorGroup)Entities.Instance.GetByName("F1SG-B"));
            secgrpC = ((SectorGroup)Entities.Instance.GetByName("F1SG-C"));
            secgrpD = ((SectorGroup)Entities.Instance.GetByName("F2SG-D"));
            secgrpE = ((SectorGroup)Entities.Instance.GetByName("F2SG-E"));
            secgrpF = ((SectorGroup)Entities.Instance.GetByName("F3SG-F"));
            secgrpG = ((SectorGroup)Entities.Instance.GetByName("F3SG-G"));

            secgrpAR7Cntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F1SG-A")).LightStatus;
            secgrpAR8Cntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F1SG-A")).LightStatus;
            secgrpBCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F1SG-B")).LightStatus;
            secgrpCCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F1SG-C")).LightStatus;
            secgrpDCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F2SG-D")).LightStatus;
            secgrpECntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F2SG-E")).LightStatus;
            secgrpFCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F3SG-F")).LightStatus;
            secgrpGCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F3SG-G")).LightStatus;

            secgrpA.SwitchLight += OnSwitchLightsA;
            secgrpB.SwitchLight += OnSwitchLightsB;
            secgrpC.SwitchLight += OnSwitchLightsC;
            secgrpD.SwitchLight += OnSwitchLightsD;
            secgrpE.SwitchLight += OnSwitchLightsE;
            secgrpF.SwitchLight += OnSwitchLightsF;
            secgrpG.SwitchLight += OnSwitchLightsG;
        }

        private void OnSwitchLightsG(bool status)
        {
            secgrpGCntrl.Visible = status;
        }

        private void OnSwitchLightsF(bool status)
        {
            secgrpFCntrl.Visible = status;
        }

        private void OnSwitchLightsE(bool status)
        {
            secgrpFCntrl.Visible = status;
        }

        private void OnSwitchLightsD(bool status)
        {
            secgrpDCntrl.Visible = status;
        }

        private void OnSwitchLightsC(bool status)
        {
            secgrpCCntrl.Visible = status;
        }

        private void OnSwitchLightsB(bool status)
        {
            secgrpBCntrl.Visible = status;
        }

        private void OnSwitchLightsA(bool status)
        {
            secgrpAR7Cntrl.Visible = status;
            secgrpAR8Cntrl.Visible = status;
        }


        private void OnInnerRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot)
        {
            if (rot.W > 0)
            {
                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree + 45) % 360;
                secgrpFCntrl.RotateDegree = (secgrpFCntrl.RotateDegree + 45) % 360;
                secgrpGCntrl.RotateDegree = (secgrpGCntrl.RotateDegree + 45) % 360;
            }
            else if (rot.W < 0)
            {
                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree - 45) % 360;
                secgrpFCntrl.RotateDegree = (secgrpFCntrl.RotateDegree - 45) % 360;
                secgrpGCntrl.RotateDegree = (secgrpGCntrl.RotateDegree - 45) % 360;
            }
        }

        private void OnMiddleRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot)
        {
            if (rot.W > 0)
            {
                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree + 45) % 360;
                secgrpAR7Cntrl.RotateDegree = (secgrpAR7Cntrl.RotateDegree + 45) % 360;
                secgrpECntrl.RotateDegree = (secgrpECntrl.RotateDegree + 45) % 360;
                secgrpDCntrl.RotateDegree = (secgrpDCntrl.RotateDegree + 45) % 360;
            }
                
            else if (rot.W < 0)
            {
                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree - 45) % 360;
                secgrpAR7Cntrl.RotateDegree = (secgrpAR7Cntrl.RotateDegree - 45) % 360;
                secgrpECntrl.RotateDegree = (secgrpECntrl.RotateDegree - 45) % 360;
                secgrpDCntrl.RotateDegree = (secgrpDCntrl.RotateDegree - 45) % 360;
            }
        }

        private void OnOuterRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot)
        {
            if (rot.W > 0)
            {
                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree + 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree + 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree + 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree + 45) % 360;
            }
            else if (rot.W < 0)
            {
                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree - 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree - 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree - 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree - 45) % 360;
            }
        }

    }
}
