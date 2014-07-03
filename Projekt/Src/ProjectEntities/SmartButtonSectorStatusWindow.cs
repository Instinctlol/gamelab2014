using Engine.EntitySystem;
using Engine.MathEx;
using Engine.UISystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    class SmartButtonSectorStatusWindow : SmartButtonWindow
    {
        private Ring ringOuter, ringInner, ringMiddle;
        private RotControl ringOuterCntrl, ringInnerCntrl, ringMiddleCntrl;
        private RotControl secgrpAR7Cntrl, secgrpAR8Cntrl, secgrpBCntrl, secgrpCCntrl, secgrpDCntrl, secgrpECntrl, secgrpFCntrl, secgrpGCntrl;
        Control ringFullCntrl;
        private List<RotControl> highlightedMiddleRingControls = new List<RotControl>();
        private List<RotControl> highlightedInnerRingControls = new List<RotControl>();
        private List<RotControl> highlightedOuterRingControls = new List<RotControl>();
        private SectorGroup secgrpA, secgrpB, secgrpC, secgrpD, secgrpE, secgrpF, secgrpG;

        [Engine.EntitySystem.EntityType.FieldSerialize]
        private float scale = 650;

        public float Scale
        {
            get { return scale; }
            set { scale = value; scaleAllRings(value); }
        }

        enum NetworkMessages
        {
            RotateInnerRingLeft,
            RotateInnerRingRight,
            RotateMiddleRingLeft,
            RotateMiddleRingRight,
            RotateOuterRingLeft,
            RotateOuterRingRight,
            TurnALightsOn,
            TurnALightsOff,
            TurnBLightsOn,
            TurnBLightsOff,
            TurnCLightsOn,
            TurnCLightsOff,
            TurnDLightsOn,
            TurnDLightsOff,
            TurnELightsOn,
            TurnELightsOff,
            TurnFLightsOn,
            TurnFLightsOff,
            TurnGLightsOn,
            TurnGLightsOff
        }

        public SmartButtonSectorStatusWindow(SmartButton button)
            : base(button)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\Minimap\\Minimap.gui");
            ringFullCntrl = (Control)CurWindow.Controls["FullRing"];

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


            for (int i = 1; i <= 8; i++)
            {
                highlightedOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r" + i + "_highlighted"]);
                highlightedMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r" + i + "_highlighted"]);
                if (i <= 4)
                    highlightedInnerRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f3r" + i + "_highlighted"]);
            }

            button.Client_WindowDataReceived += Client_WindowDataReceived;
            initialize();
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

            CurWindow.Size = new ScaleValue(Control.ScaleType.ScaleByResolution, new Vec2(size, size));
        }

        public void initialize()
        {
            if(button.IsServer)
            {
                ringOuter = ((Ring)Entities.Instance.GetByName("F1_Ring"));
                ringInner = ((Ring)Entities.Instance.GetByName("F3_Ring"));
                ringMiddle = ((Ring)Entities.Instance.GetByName("F2_Ring"));

                ringOuter.RotateRing += OnOuterRotation;
                ringInner.RotateRing += OnInnerRotation;
                ringMiddle.RotateRing += OnMiddleRotation;

                // Ringe drehen entsprechend der Computer-Konfig
                for (int ring = 0; ring < Computer.RingRotations.Length; ring++)
                {
                    if (Computer.RingRotations[0] != 0)
                    {
                        // Negative Anzahl an Rotierungen heißt links herum wurde gedreht
                        bool left = (Computer.RingRotations[ring] < 0);
                        for (int rot = 0; rot < Math.Abs(Computer.RingRotations[ring]); rot++)
                        {
                            Vec3 pos = new Vec3();
                            Quat rotation = new Quat();
                            switch (ring)
                            {
                                case 0:
                                    OnOuterRotation(pos, rotation, left);
                                    break;
                                case 1:
                                    OnMiddleRotation(pos, rotation, left);
                                    break;
                                case 2:
                                    OnInnerRotation(pos, rotation, left);
                                    break;
                            }
                        }
                    }
                }

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

                if (((SectorGroup)Entities.Instance.GetByName("F1SG-A")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnALightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnALightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F1SG-B")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnBLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnBLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F1SG-C")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnCLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnCLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F2SG-D")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnDLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnDLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F2SG-E")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnELightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnELightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F3SG-F")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnFLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnFLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F3SG-G")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnGLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.TurnGLightsOff);


                secgrpA.SwitchLight += OnSwitchLightsA;
                secgrpB.SwitchLight += OnSwitchLightsB;
                secgrpC.SwitchLight += OnSwitchLightsC;
                secgrpD.SwitchLight += OnSwitchLightsD;
                secgrpE.SwitchLight += OnSwitchLightsE;
                secgrpF.SwitchLight += OnSwitchLightsF;
                secgrpG.SwitchLight += OnSwitchLightsG;
            }
        }

        private void OnSwitchLightsG(bool status)
        {
            if(status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnGLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnGLightsOff);
            }
            

            secgrpGCntrl.Visible = status;
        }

        private void OnSwitchLightsF(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnFLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnFLightsOff);
            }

            secgrpFCntrl.Visible = status;
        }

        private void OnSwitchLightsE(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnELightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnELightsOff);
            }

            secgrpFCntrl.Visible = status;
        }

        private void OnSwitchLightsD(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnDLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnDLightsOff);
            }

            secgrpDCntrl.Visible = status;
        }

        private void OnSwitchLightsC(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnCLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnCLightsOff);
            }

            secgrpCCntrl.Visible = status;
        }

        private void OnSwitchLightsB(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnBLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnBLightsOff);
            }

            secgrpBCntrl.Visible = status;
        }

        private void OnSwitchLightsA(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnALightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.TurnALightsOff);
            }

            secgrpAR7Cntrl.Visible = status;
            secgrpAR8Cntrl.Visible = status;
        }

        //Zeigt oder versteckt das highlight Control fuer einen bestimmten Raum (z.B. "f1r3", true), case-sensitive
        public void highlight(String s, bool b)
        {
            ringFullCntrl.Controls["Highlight_Overlay"].Controls[s + "_highlighted"].Visible = b;
        }


        private void OnInnerRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (!left)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.RotateInnerRingRight);

                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree + 45) % 360;
                secgrpFCntrl.RotateDegree = (secgrpFCntrl.RotateDegree + 45) % 360;
                secgrpGCntrl.RotateDegree = (secgrpGCntrl.RotateDegree + 45) % 360;
                foreach (RotControl r in highlightedInnerRingControls)
                {
                    r.RotateDegree = (r.RotateDegree + 45) % 360;
                }
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.RotateInnerRingLeft);

                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree - 45) % 360;
                secgrpFCntrl.RotateDegree = (secgrpFCntrl.RotateDegree - 45) % 360;
                secgrpGCntrl.RotateDegree = (secgrpGCntrl.RotateDegree - 45) % 360;
                foreach (RotControl r in highlightedInnerRingControls)
                {
                    r.RotateDegree = (r.RotateDegree - 45) % 360;
                }
            }
        }

        private void OnMiddleRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (!left)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.RotateMiddleRingRight);

                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree + 45) % 360;
                secgrpAR7Cntrl.RotateDegree = (secgrpAR7Cntrl.RotateDegree + 45) % 360;
                secgrpECntrl.RotateDegree = (secgrpECntrl.RotateDegree + 45) % 360;
                secgrpDCntrl.RotateDegree = (secgrpDCntrl.RotateDegree + 45) % 360;
                foreach (RotControl r in highlightedMiddleRingControls)
                {
                    r.RotateDegree = (r.RotateDegree + 45) % 360;
                }
            }

            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.RotateMiddleRingLeft);

                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree - 45) % 360;
                secgrpAR7Cntrl.RotateDegree = (secgrpAR7Cntrl.RotateDegree - 45) % 360;
                secgrpECntrl.RotateDegree = (secgrpECntrl.RotateDegree - 45) % 360;
                secgrpDCntrl.RotateDegree = (secgrpDCntrl.RotateDegree - 45) % 360;
                foreach (RotControl r in highlightedMiddleRingControls)
                {
                    r.RotateDegree = (r.RotateDegree - 45) % 360;
                }
            }
        }

        private void OnOuterRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (!left)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.RotateOuterRingRight);

                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree + 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree + 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree + 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree + 45) % 360;
                foreach (RotControl r in highlightedOuterRingControls)
                {
                    r.RotateDegree = (r.RotateDegree + 45) % 360;
                }
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.RotateOuterRingLeft);

                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree - 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree - 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree - 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree - 45) % 360;
                foreach (RotControl r in highlightedOuterRingControls)
                {
                    r.RotateDegree = (r.RotateDegree - 45) % 360;
                }
            }
        }




        private void Client_WindowDataReceived(ushort message)
        {
            if (button.IsActive)
            {
                NetworkMessages msg = (NetworkMessages)message;
                switch (msg)
                {
                    case NetworkMessages.TurnALightsOff:
                        Client_OnSwitchLightsA(false);
                        break;
                    case NetworkMessages.TurnALightsOn:
                        Client_OnSwitchLightsA(true);
                        break;
                    case NetworkMessages.TurnBLightsOff:
                        Client_OnSwitchLightsB(false);
                        break;
                    case NetworkMessages.TurnBLightsOn:
                        Client_OnSwitchLightsB(true);
                        break;
                    case NetworkMessages.TurnCLightsOff:
                        Client_OnSwitchLightsC(false);
                        break;
                    case NetworkMessages.TurnCLightsOn:
                        Client_OnSwitchLightsC(true);
                        break;
                    case NetworkMessages.TurnDLightsOff:
                        Client_OnSwitchLightsD(false);
                        break;
                    case NetworkMessages.TurnDLightsOn:
                        Client_OnSwitchLightsD(true);
                        break;
                    case NetworkMessages.TurnELightsOff:
                        Client_OnSwitchLightsE(false);
                        break;
                    case NetworkMessages.TurnELightsOn:
                        Client_OnSwitchLightsE(true);
                        break;
                    case NetworkMessages.TurnFLightsOff:
                        Client_OnSwitchLightsF(false);
                        break;
                    case NetworkMessages.TurnFLightsOn:
                        Client_OnSwitchLightsF(true);
                        break;
                    case NetworkMessages.TurnGLightsOff:
                        Client_OnSwitchLightsG(false);
                        break;
                    case NetworkMessages.TurnGLightsOn:
                        Client_OnSwitchLightsG(true);
                        break;
                    case NetworkMessages.RotateInnerRingLeft:
                        Client_OnInnerRotation(true);
                        break;
                    case NetworkMessages.RotateInnerRingRight:
                        Client_OnInnerRotation(false);
                        break;
                    case NetworkMessages.RotateOuterRingLeft:
                        Client_OnOuterRotation(true);
                        break;
                    case NetworkMessages.RotateOuterRingRight:
                        Client_OnOuterRotation(false);
                        break;
                    case NetworkMessages.RotateMiddleRingLeft:
                        Client_OnMiddleRotation(true);
                        break;
                    case NetworkMessages.RotateMiddleRingRight:
                        Client_OnMiddleRotation(false);
                        break;
                }
            }
        }

        private void Client_OnSwitchLightsG(bool status)
        {
            secgrpGCntrl.Visible = status;
        }

        private void Client_OnSwitchLightsF(bool status)
        {
            secgrpFCntrl.Visible = status;
        }

        private void Client_OnSwitchLightsE(bool status)
        {
            secgrpFCntrl.Visible = status;
        }

        private void Client_OnSwitchLightsD(bool status)
        {
            secgrpDCntrl.Visible = status;
        }

        private void Client_OnSwitchLightsC(bool status)
        {
            secgrpCCntrl.Visible = status;
        }

        private void Client_OnSwitchLightsB(bool status)
        {
            secgrpBCntrl.Visible = status;
        }

        private void Client_OnSwitchLightsA(bool status)
        {
            secgrpAR7Cntrl.Visible = status;
            secgrpAR8Cntrl.Visible = status;
        }

        private void Client_OnInnerRotation(bool left)
        {
            if (!left)
            {
                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree + 45) % 360;
                secgrpFCntrl.RotateDegree = (secgrpFCntrl.RotateDegree + 45) % 360;
                secgrpGCntrl.RotateDegree = (secgrpGCntrl.RotateDegree + 45) % 360;
                foreach (RotControl r in highlightedInnerRingControls)
                {
                    r.RotateDegree = (r.RotateDegree + 45) % 360;
                }
            }
            else
            {
                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree - 45) % 360;
                secgrpFCntrl.RotateDegree = (secgrpFCntrl.RotateDegree - 45) % 360;
                secgrpGCntrl.RotateDegree = (secgrpGCntrl.RotateDegree - 45) % 360;
                foreach (RotControl r in highlightedInnerRingControls)
                {
                    r.RotateDegree = (r.RotateDegree - 45) % 360;
                }
            }
        }

        private void Client_OnMiddleRotation(bool left)
        {
            if (!left)
            {
                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree + 45) % 360;
                secgrpAR7Cntrl.RotateDegree = (secgrpAR7Cntrl.RotateDegree + 45) % 360;
                secgrpECntrl.RotateDegree = (secgrpECntrl.RotateDegree + 45) % 360;
                secgrpDCntrl.RotateDegree = (secgrpDCntrl.RotateDegree + 45) % 360;
                foreach (RotControl r in highlightedMiddleRingControls)
                {
                    r.RotateDegree = (r.RotateDegree + 45) % 360;
                }
            }

            else
            {
                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree - 45) % 360;
                secgrpAR7Cntrl.RotateDegree = (secgrpAR7Cntrl.RotateDegree - 45) % 360;
                secgrpECntrl.RotateDegree = (secgrpECntrl.RotateDegree - 45) % 360;
                secgrpDCntrl.RotateDegree = (secgrpDCntrl.RotateDegree - 45) % 360;
                foreach (RotControl r in highlightedMiddleRingControls)
                {
                    r.RotateDegree = (r.RotateDegree - 45) % 360;
                }
            }
        }

        private void Client_OnOuterRotation(bool left)
        {
            if (!left)
            {
                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree + 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree + 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree + 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree + 45) % 360;
                foreach (RotControl r in highlightedOuterRingControls)
                {
                    r.RotateDegree = (r.RotateDegree + 45) % 360;
                }
            }
            else
            {
                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree - 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree - 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree - 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree - 45) % 360;
                foreach (RotControl r in highlightedOuterRingControls)
                {
                    r.RotateDegree = (r.RotateDegree - 45) % 360;
                }
            }
        }
    }
}
