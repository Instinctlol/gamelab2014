using Engine.EntitySystem;
using Engine.MathEx;
using Engine.Renderer;
using Engine.UISystem;
using ProjectCommon;
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
        private SectorGroup secgrpA, secgrpB, secgrpC, secgrpD, secgrpE, secgrpF, secgrpG;
        GameNetworkClient client = GameNetworkClient.Instance;

        [Engine.EntitySystem.EntityType.FieldSerialize]
        private float scale = 650;

        public float Scale
        {
            get { return scale; }
            set { scale = value; scaleAllRings(value); }
        }

        enum NetworkMessages
        {
            Client_Oculus_RotateInnerRingLeft,
            Client_Oculus_RotateInnerRingRight,
            Client_Oculus_RotateMiddleRingLeft,
            Client_Oculus_RotateMiddleRingRight,
            Client_Oculus_RotateOuterRingLeft,
            Client_Oculus_RotateOuterRingRight,
            Client_Cave_RotateInnerRingLeft,
            Client_Cave_RotateInnerRingRight,
            Client_Cave_RotateOuterRingLeft,
            Client_Cave_RotateOuterRingRight,
            Client_Cave_RotateMiddleRingLeft,
            Client_Cave_RotateMiddleRingRight,
            Client_Oculus_TurnALightsOn,
            Client_Oculus_TurnALightsOff,
            Client_Oculus_TurnBLightsOn,
            Client_Oculus_TurnBLightsOff,
            Client_Oculus_TurnCLightsOn,
            Client_Oculus_TurnCLightsOff,
            Client_Oculus_TurnDLightsOn,
            Client_Oculus_TurnDLightsOff,
            Client_Oculus_TurnELightsOn,
            Client_Oculus_TurnELightsOff,
            Client_Oculus_TurnFLightsOn,
            Client_Oculus_TurnFLightsOff,
            Client_Oculus_TurnGLightsOn,
            Client_Oculus_TurnGLightsOff,
            Client_Oculus_AskForRotationsUpdate,
            Client_Cave_AskForRotationsUpdate,
            Server_UpdateRotationsForOculus,
            Server_UpdateRotationsForCave,
            Client_Cave_TurnALightsOff,
            Client_Cave_TurnALightsOn,
            Client_Cave_TurnBLightsOff,
            Client_Cave_TurnBLightsOn,
            Client_Cave_TurnCLightsOff,
            Client_Cave_TurnCLightsOn,
            Client_Cave_TurnDLightsOff,
            Client_Cave_TurnDLightsOn,
            Client_Cave_TurnELightsOff,
            Client_Cave_TurnELightsOn,
            Client_Cave_TurnFLightsOff,
            Client_Cave_TurnFLightsOn,
            Client_Cave_TurnGLightsOff,
            Client_Cave_TurnGLightsOn,
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

            initializeWithRings();

            button.Client_WindowDataReceived += Client_WindowDataReceived;

            VerticalAlign = VerticalAlign.Center;

            if (!button.IsServer)
            {

                if (client.isOculus)
                    button.Client_SendWindowData((UInt16)NetworkMessages.Server_UpdateRotationsForOculus);
                else
                    button.Client_SendWindowData((UInt16)NetworkMessages.Server_UpdateRotationsForCave);
            }
                
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

        public void initializeWithRings()
        {
            if(button.IsServer)
            {
                ringOuter = ((Ring)Entities.Instance.GetByName("F1_Ring"));
                ringInner = ((Ring)Entities.Instance.GetByName("F3_Ring"));
                ringMiddle = ((Ring)Entities.Instance.GetByName("F2_Ring"));

                ringOuter.RotateRing += Server_OnOuterRotation;
                ringInner.RotateRing += Server_OnInnerRotation;
                ringMiddle.RotateRing += Server_OnMiddleRotation;

                updateRotationsAndLights();

                secgrpA = ((SectorGroup)Entities.Instance.GetByName("F1SG-A"));
                secgrpB = ((SectorGroup)Entities.Instance.GetByName("F1SG-B"));
                secgrpC = ((SectorGroup)Entities.Instance.GetByName("F1SG-C"));
                secgrpD = ((SectorGroup)Entities.Instance.GetByName("F2SG-D"));
                secgrpE = ((SectorGroup)Entities.Instance.GetByName("F2SG-E"));
                secgrpF = ((SectorGroup)Entities.Instance.GetByName("F3SG-F"));
                secgrpG = ((SectorGroup)Entities.Instance.GetByName("F3SG-G"));

                

                secgrpA.SwitchLight += Server_OnSwitchLightsA;
                secgrpB.SwitchLight += Server_OnSwitchLightsB;
                secgrpC.SwitchLight += Server_OnSwitchLightsC;
                secgrpD.SwitchLight += Server_OnSwitchLightsD;
                secgrpE.SwitchLight += Server_OnSwitchLightsE;
                secgrpF.SwitchLight += Server_OnSwitchLightsF;
                secgrpG.SwitchLight += Server_OnSwitchLightsG;
            }
        }

        private void updateRotationsAndLights()
        {
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
                                Server_OnOuterRotation(pos, rotation, left);
                                break;
                            case 1:
                                Server_OnMiddleRotation(pos, rotation, left);
                                break;
                            case 2:
                                Server_OnInnerRotation(pos, rotation, left);
                                break;
                        }
                    }
                }
            }


            secgrpAR7Cntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F1SG-A")).LightStatus;
            secgrpAR8Cntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F1SG-A")).LightStatus;
            secgrpBCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F1SG-B")).LightStatus;
            secgrpCCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F1SG-C")).LightStatus;
            secgrpDCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F2SG-D")).LightStatus;
            secgrpECntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F2SG-E")).LightStatus;
            secgrpFCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F3SG-F")).LightStatus;
            secgrpGCntrl.Visible = ((SectorGroup)Entities.Instance.GetByName("F3SG-G")).LightStatus;


            
        }

        private void updateRotationsAndLightsForClients(bool forOcunaut)
        {
            if(forOcunaut)
            {
                // Ringe drehen entsprechend der Computer-Konfig
                for (int ring = 0; ring < Computer.RingRotations.Length; ring++)
                {
                    if (Computer.RingRotations[0] != 0)
                    {
                        // Negative Anzahl an Rotierungen heißt links herum wurde gedreht
                        bool left = (Computer.RingRotations[ring] < 0);
                        for (int rot = 0; rot < Math.Abs(Computer.RingRotations[ring]); rot++)
                        {
                            switch (ring)
                            {
                                case 0:
                                    if(left)
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateOuterRingLeft);
                                    else
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateOuterRingRight);
                                    break;
                                case 1:
                                    if (left)
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateMiddleRingLeft);
                                    else
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateMiddleRingRight);
                                    break;
                                case 2:
                                    if (left)
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateInnerRingLeft);
                                    else
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateInnerRingRight);
                                    break;
                            }
                        }
                    }
                }
                if (((SectorGroup)Entities.Instance.GetByName("F1SG-A")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnALightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnALightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F1SG-B")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnBLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnBLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F1SG-C")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnCLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnCLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F2SG-D")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnDLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnDLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F2SG-E")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnELightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnELightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F3SG-F")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnFLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnFLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F3SG-G")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnGLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnGLightsOff);
            }
            else
            {
                // Ringe drehen entsprechend der Computer-Konfig
                for (int ring = 0; ring < Computer.RingRotations.Length; ring++)
                {
                    if (Computer.RingRotations[0] != 0)
                    {
                        // Negative Anzahl an Rotierungen heißt links herum wurde gedreht
                        bool left = (Computer.RingRotations[ring] < 0);
                        for (int rot = 0; rot < Math.Abs(Computer.RingRotations[ring]); rot++)
                        {
                            switch (ring)
                            {
                                case 0:
                                    if (left)
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateOuterRingLeft);
                                    else
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateOuterRingRight);
                                    break;
                                case 1:
                                    if (left)
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateMiddleRingLeft);
                                    else
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateMiddleRingRight);
                                    break;
                                case 2:
                                    if (left)
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateInnerRingLeft);
                                    else
                                        button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateInnerRingRight);
                                    break;
                            }
                        }
                    }
                }

                if (((SectorGroup)Entities.Instance.GetByName("F1SG-A")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnALightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnALightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F1SG-B")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnBLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnBLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F1SG-C")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnCLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnCLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F2SG-D")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnDLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnDLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F2SG-E")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnELightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnELightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F3SG-F")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnFLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnFLightsOff);

                if (((SectorGroup)Entities.Instance.GetByName("F3SG-G")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnGLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnGLightsOff);
            }
        }

        private void Server_OnSwitchLightsG(bool status)
        {
            if(status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnGLightsOn);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnGLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnGLightsOff);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnGLightsOff);
            }
            

            secgrpGCntrl.Visible = status;
        }

        private void Server_OnSwitchLightsF(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnFLightsOn);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnFLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnFLightsOff);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnFLightsOff);
            }

            secgrpFCntrl.Visible = status;
        }

        private void Server_OnSwitchLightsE(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnELightsOn);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnELightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnELightsOff);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnELightsOff);
            }

            secgrpFCntrl.Visible = status;
        }

        private void Server_OnSwitchLightsD(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnDLightsOn);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnDLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnDLightsOff);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnDLightsOff);
            }

            secgrpDCntrl.Visible = status;
        }

        private void Server_OnSwitchLightsC(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnCLightsOn);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnCLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnCLightsOff);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnCLightsOff);
            }

            secgrpCCntrl.Visible = status;
        }

        private void Server_OnSwitchLightsB(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnBLightsOn);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnBLightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnBLightsOff);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnBLightsOff);
            }

            secgrpBCntrl.Visible = status;
        }

        private void Server_OnSwitchLightsA(bool status)
        {
            if (status)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnALightsOn);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnALightsOn);
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_TurnALightsOff);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_TurnALightsOff);
            }

            secgrpAR7Cntrl.Visible = status;
            secgrpAR8Cntrl.Visible = status;
        }

        private void Server_OnInnerRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (!left)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateInnerRingRight);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateInnerRingRight);

                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree + 45) % 360;
                secgrpFCntrl.RotateDegree = (secgrpFCntrl.RotateDegree + 45) % 360;
                secgrpGCntrl.RotateDegree = (secgrpGCntrl.RotateDegree + 45) % 360;
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateInnerRingLeft);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateInnerRingLeft);

                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree - 45) % 360;
                secgrpFCntrl.RotateDegree = (secgrpFCntrl.RotateDegree - 45) % 360;
                secgrpGCntrl.RotateDegree = (secgrpGCntrl.RotateDegree - 45) % 360;
            }
        }

        private void Server_OnMiddleRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (!left)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateMiddleRingRight);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateMiddleRingRight);

                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree + 45) % 360;
                secgrpAR7Cntrl.RotateDegree = (secgrpAR7Cntrl.RotateDegree + 45) % 360;
                secgrpECntrl.RotateDegree = (secgrpECntrl.RotateDegree + 45) % 360;
                secgrpDCntrl.RotateDegree = (secgrpDCntrl.RotateDegree + 45) % 360;
            }

            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateMiddleRingLeft);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateMiddleRingLeft);

                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree - 45) % 360;
                secgrpAR7Cntrl.RotateDegree = (secgrpAR7Cntrl.RotateDegree - 45) % 360;
                secgrpECntrl.RotateDegree = (secgrpECntrl.RotateDegree - 45) % 360;
                secgrpDCntrl.RotateDegree = (secgrpDCntrl.RotateDegree - 45) % 360;
            }
        }

        private void Server_OnOuterRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (!left)
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateOuterRingRight);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateOuterRingRight);

                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree + 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree + 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree + 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree + 45) % 360;
            }
            else
            {
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Oculus_RotateOuterRingLeft);
                button.Server_SendWindowData((UInt16)NetworkMessages.Client_Cave_RotateOuterRingLeft);

                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree - 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree - 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree - 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree - 45) % 360;
            }
        }


        private void Server_WindowDataReceived(ushort message)
        {
            NetworkMessages msg = (NetworkMessages)message;
            switch(msg)
            {
                case NetworkMessages.Server_UpdateRotationsForOculus:
                    updateRotationsAndLightsForClients(true);
                    break;
                case NetworkMessages.Server_UpdateRotationsForCave:
                    updateRotationsAndLightsForClients(false);
                    break;
            }
        }

        private void Client_WindowDataReceived(ushort message)
        {
            
            NetworkMessages msg = (NetworkMessages)message;
            switch (msg)
            {
                case NetworkMessages.Client_Oculus_TurnALightsOff:
                    Client_Oculus_OnSwitchLightsA(false);
                    break;
                case NetworkMessages.Client_Oculus_TurnALightsOn:
                    Client_Oculus_OnSwitchLightsA(true);
                    break;
                case NetworkMessages.Client_Oculus_TurnBLightsOff:
                    Client_Oculus_OnSwitchLightsB(false);
                    break;
                case NetworkMessages.Client_Oculus_TurnBLightsOn:
                    Client_Oculus_OnSwitchLightsB(true);
                    break;
                case NetworkMessages.Client_Oculus_TurnCLightsOff:
                    Client_Oculus_OnSwitchLightsC(false);
                    break;
                case NetworkMessages.Client_Oculus_TurnCLightsOn:
                    Client_Oculus_OnSwitchLightsC(true);
                    break;
                case NetworkMessages.Client_Oculus_TurnDLightsOff:
                    Client_Oculus_OnSwitchLightsD(false);
                    break;
                case NetworkMessages.Client_Oculus_TurnDLightsOn:
                    Client_Oculus_OnSwitchLightsD(true);
                    break;
                case NetworkMessages.Client_Oculus_TurnELightsOff:
                    Client_Oculus_OnSwitchLightsE(false);
                    break;
                case NetworkMessages.Client_Oculus_TurnELightsOn:
                    Client_Oculus_OnSwitchLightsE(true);
                    break;
                case NetworkMessages.Client_Oculus_TurnFLightsOff:
                    Client_Oculus_OnSwitchLightsF(false);
                    break;
                case NetworkMessages.Client_Oculus_TurnFLightsOn:
                    Client_Oculus_OnSwitchLightsF(true);
                    break;
                case NetworkMessages.Client_Oculus_TurnGLightsOff:
                    Client_Oculus_OnSwitchLightsG(false);
                    break;
                case NetworkMessages.Client_Oculus_TurnGLightsOn:
                    Client_Oculus_OnSwitchLightsG(true);
                    break;
                case NetworkMessages.Client_Cave_TurnALightsOff:
                    Client_Cave_OnSwitchLightsA(false);
                    break;
                case NetworkMessages.Client_Cave_TurnALightsOn:
                    Client_Cave_OnSwitchLightsA(true);
                    break;
                case NetworkMessages.Client_Cave_TurnBLightsOff:
                    Client_Cave_OnSwitchLightsB(false);
                    break;
                case NetworkMessages.Client_Cave_TurnBLightsOn:
                    Client_Cave_OnSwitchLightsB(true);
                    break;
                case NetworkMessages.Client_Cave_TurnCLightsOff:
                    Client_Cave_OnSwitchLightsC(false);
                    break;
                case NetworkMessages.Client_Cave_TurnCLightsOn:
                    Client_Cave_OnSwitchLightsC(true);
                    break;
                case NetworkMessages.Client_Cave_TurnDLightsOff:
                    Client_Cave_OnSwitchLightsD(false);
                    break;
                case NetworkMessages.Client_Cave_TurnDLightsOn:
                    Client_Cave_OnSwitchLightsD(true);
                    break;
                case NetworkMessages.Client_Cave_TurnELightsOff:
                    Client_Cave_OnSwitchLightsE(false);
                    break;
                case NetworkMessages.Client_Cave_TurnELightsOn:
                    Client_Cave_OnSwitchLightsE(true);
                    break;
                case NetworkMessages.Client_Cave_TurnFLightsOff:
                    Client_Cave_OnSwitchLightsF(false);
                    break;
                case NetworkMessages.Client_Cave_TurnFLightsOn:
                    Client_Cave_OnSwitchLightsF(true);
                    break;
                case NetworkMessages.Client_Cave_TurnGLightsOff:
                    Client_Cave_OnSwitchLightsG(false);
                    break;
                case NetworkMessages.Client_Cave_TurnGLightsOn:
                    Client_Cave_OnSwitchLightsG(true);
                    break;
                case NetworkMessages.Client_Oculus_RotateInnerRingLeft:
                    Client_Oculus_OnInnerRotation(true);
                    break;
                case NetworkMessages.Client_Oculus_RotateInnerRingRight:
                    Client_Oculus_OnInnerRotation(false);
                    break;
                case NetworkMessages.Client_Oculus_RotateOuterRingLeft:
                    Client_Oculus_OnOuterRotation(true);
                    break;
                case NetworkMessages.Client_Oculus_RotateOuterRingRight:
                    Client_Oculus_OnOuterRotation(false);
                    break;
                case NetworkMessages.Client_Oculus_RotateMiddleRingLeft:
                    Client_Oculus_OnMiddleRotation(true);
                    break;
                case NetworkMessages.Client_Oculus_RotateMiddleRingRight:
                    Client_Oculus_OnMiddleRotation(false);
                    break;
                case NetworkMessages.Client_Cave_RotateInnerRingLeft:
                    Client_Cave_OnInnerRotation(true);
                    break;
                case NetworkMessages.Client_Cave_RotateInnerRingRight:
                    Client_Cave_OnInnerRotation(false);
                    break;
                case NetworkMessages.Client_Cave_RotateOuterRingLeft:
                    Client_Cave_OnOuterRotation(true);
                    break;
                case NetworkMessages.Client_Cave_RotateOuterRingRight:
                    Client_Cave_OnOuterRotation(false);
                    break;
                case NetworkMessages.Client_Cave_RotateMiddleRingLeft:
                    Client_Cave_OnMiddleRotation(true);
                    break;
                case NetworkMessages.Client_Cave_RotateMiddleRingRight:
                    Client_Cave_OnMiddleRotation(false);
                    break;
            }
        }

        private void Client_Cave_OnSwitchLightsG(bool p)
        {
            if (!client.isOculus)
                Client_OnSwitchLightsG(p);
        }

        private void Client_Cave_OnSwitchLightsF(bool p)
        {
            if (!client.isOculus)
                Client_OnSwitchLightsF(p);
        }

        private void Client_Cave_OnSwitchLightsE(bool p)
        {
            if (!client.isOculus)
                Client_OnSwitchLightsE(p);
        }

        private void Client_Cave_OnSwitchLightsD(bool p)
        {
            if (!client.isOculus)
                Client_OnSwitchLightsD(p);
        }

        private void Client_Cave_OnSwitchLightsC(bool p)
        {
            if (!client.isOculus)
                Client_OnSwitchLightsC(p);
        }

        private void Client_Cave_OnSwitchLightsB(bool p)
        {
            if (!client.isOculus)
                Client_OnSwitchLightsB(p);
        }

        private void Client_Cave_OnSwitchLightsA(bool p)
        {
            if (!client.isOculus)
                Client_OnSwitchLightsA(p);
        }

        private void Client_Oculus_OnSwitchLightsG(bool p)
        {
            if (client.isOculus)
                Client_OnSwitchLightsG(p);
        }

        private void Client_Oculus_OnSwitchLightsF(bool p)
        {
            if (client.isOculus)
                Client_OnSwitchLightsF(p);
        }

        private void Client_Oculus_OnSwitchLightsE(bool p)
        {
            if (client.isOculus)
                Client_OnSwitchLightsE(p);
        }

        private void Client_Oculus_OnSwitchLightsD(bool p)
        {
            if (client.isOculus)
                Client_OnSwitchLightsD(p);
        }

        private void Client_Oculus_OnSwitchLightsC(bool p)
        {
            if (client.isOculus)
                Client_OnSwitchLightsC(p);
        }

        private void Client_Oculus_OnSwitchLightsB(bool p)
        {
            if (client.isOculus)
                Client_OnSwitchLightsB(p);
        }

        private void Client_Oculus_OnSwitchLightsA(bool p)
        {
            if (client.isOculus)
                Client_OnSwitchLightsA(p);
        }

        private void Client_Cave_OnMiddleRotation(bool p)
        {
            if (!client.isOculus)
                Client_OnMiddleRotation(p);
        }

        private void Client_Cave_OnOuterRotation(bool p)
        {
            if (!client.isOculus)
                Client_OnOuterRotation(p);
        }

        private void Client_Cave_OnInnerRotation(bool p)
        {
            if (!client.isOculus)
                Client_OnInnerRotation(p);
        }

        private void Client_Oculus_OnMiddleRotation(bool p)
        {
            if (client.isOculus)
                Client_OnMiddleRotation(p);
        }

        private void Client_Oculus_OnOuterRotation(bool p)
        {
            if (client.isOculus)
                Client_OnOuterRotation(p);
        }

        private void Client_Oculus_OnInnerRotation(bool p)
        {
            if (client.isOculus)
                Client_OnInnerRotation(p);
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
            }
            else
            {
                ringInnerCntrl.RotateDegree = (ringInnerCntrl.RotateDegree - 45) % 360;
                secgrpFCntrl.RotateDegree = (secgrpFCntrl.RotateDegree - 45) % 360;
                secgrpGCntrl.RotateDegree = (secgrpGCntrl.RotateDegree - 45) % 360;
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
            }

            else
            {
                ringMiddleCntrl.RotateDegree = (ringMiddleCntrl.RotateDegree - 45) % 360;
                secgrpAR7Cntrl.RotateDegree = (secgrpAR7Cntrl.RotateDegree - 45) % 360;
                secgrpECntrl.RotateDegree = (secgrpECntrl.RotateDegree - 45) % 360;
                secgrpDCntrl.RotateDegree = (secgrpDCntrl.RotateDegree - 45) % 360;
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
            }
            else
            {
                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree - 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree - 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree - 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree - 45) % 360;
            }
        }
    }
}
