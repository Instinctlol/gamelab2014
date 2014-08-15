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
        private List<RotControl> extraMiddleRingControls = new List<RotControl>();  //highlights, trigger and stuff
        private List<RotControl> extraInnerRingControls = new List<RotControl>();   //highlights, trigger and stuff
        private List<RotControl> extraOuterRingControls = new List<RotControl>();   //highlights, trigger and stuff
        GameNetworkClient client = GameNetworkClient.Instance;
        // ringRotations[0] is Ring F1, ringRotations[1] is Ring F2 and ringRotations[2] is Ring F3 (inner Ring)
        private int[] currRingRotations = new int[3];   //saves the current ring positions currently shown
        private int[] newestRingRotations;  //server 'writes' newest ring positions here

        [Engine.EntitySystem.EntityType.FieldSerialize]
        private float scale = 650;

        public float Scale
        {
            get { return scale; }
            set { scale = value; scaleAllRings(value); }
        }

        enum NetworkMessages
        {
            //Client_ means, the message is directed to the client
            Client_RotateInnerRingLeft,
            Client_RotateInnerRingRight,
            Client_RotateMiddleRingLeft,
            Client_RotateMiddleRingRight,
            Client_RotateOuterRingLeft,
            Client_RotateOuterRingRight,
            Client_TurnALightsOn,
            Client_TurnALightsOff,
            Client_TurnBLightsOn,
            Client_TurnBLightsOff,
            Client_TurnCLightsOn,
            Client_TurnCLightsOff,
            Client_TurnDLightsOn,
            Client_TurnDLightsOff,
            Client_TurnELightsOn,
            Client_TurnELightsOff,
            Client_TurnFLightsOn,
            Client_TurnFLightsOff,
            Client_TurnGLightsOn,
            Client_TurnGLightsOff,
            Server_UpdateNewestRingRotationsForClient,      //to initialize the client
            Server_UpdateLightsForClient,                   //to initialize the client
            Client_ReceiveNewestRingRotations,
        }

        public SmartButtonSectorStatusWindow(SmartButton button)
            : base(button)
        {
            CurWindow = ControlDeclarationManager.Instance.CreateControl("GUI\\Minimap\\Minimap.gui");
            ringFullCntrl = (Control)CurWindow.Controls["FullRing"];

            ringInnerCntrl = (RotControl)ringFullCntrl.Controls["InnerRing"];
            ringMiddleCntrl = (RotControl)ringFullCntrl.Controls["MiddleRing"];
            ringOuterCntrl = (RotControl)ringFullCntrl.Controls["OuterRing"];
            //set control scale
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

            //initialize currRingRotations
            for(int i=0; i<currRingRotations.Length; i++)
                currRingRotations[i]=0;

            //the server initializes the controls with the help of the actual rings
            Server_initializeWithRings();

            //network register method events..
            button.Client_WindowDataReceived += Client_WindowDataReceived;
            button.Client_WindowStringReceived += Client_StringReceived;
            button.Server_WindowDataReceived += Server_WindowDataReceived;

            extraInnerRingControls.Add((RotControl)ringFullCntrl.Controls["Trigger+TP"].Controls["f3r3_tp"]);
            extraInnerRingControls.Add((RotControl)ringFullCntrl.Controls["Trigger+TP"].Controls["f3r1_trigger"]);
            extraMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Trigger+TP"].Controls["f2r5_trigger"]);
            extraOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Trigger+TP"].Controls["f1r4_trigger"]);
            ((RotControl)ringFullCntrl.Controls["Trigger+TP"].Controls["f3r3_tp"]).Visible = true;
            ((RotControl)ringFullCntrl.Controls["Trigger+TP"].Controls["f3r1_trigger"]).Visible = true;
            ((RotControl)ringFullCntrl.Controls["Trigger+TP"].Controls["f2r5_trigger"]).Visible = true;
            ((RotControl)ringFullCntrl.Controls["Trigger+TP"].Controls["f1r4_trigger"]).Visible = true;



            switch(button.Terminal.SectorStatusData.ToLower())
            {
                case "f1r1":
                    highlight("f1r1", true);
                    extraOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r1_highlighted"]);
                    break;
                case "f1r2":
                    highlight("f1r2", true);
                    extraOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r2_highlighted"]);
                    break;
                case "f1r3":
                    highlight("f1r3", true);
                    extraOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r3_highlighted"]);
                    break;
                case "f1r4":
                    highlight("f1r4", true);
                    extraOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r4_highlighted"]);
                    break;
                case "f1r5":
                    highlight("f1r5", true);
                    extraOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r5_highlighted"]);
                    break;
                case "f1r6":
                    highlight("f1r6", true);
                    extraOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r6_highlighted"]);
                    break;
                case "f1r7":
                    highlight("f1r7", true);
                    extraOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r7_highlighted"]);
                    break;
                case "f1r8":
                    highlight("f1r8", true);
                    extraOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r8_highlighted"]);
                    break;
                case "f2r1":
                    highlight("f2r1", true);
                    extraMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r1_highlighted"]);
                    break;
                case "f2r2":
                    highlight("f2r2", true);
                    extraMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r2_highlighted"]);
                    break;
                case "f2r3":
                    highlight("f2r3", true);
                    extraMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r3_highlighted"]);
                    break;
                case "f2r4":
                    highlight("f2r4", true);
                    extraMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r4_highlighted"]);
                    break;
                case "f2r5":
                    highlight("f2r5", true);
                    extraMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r5_highlighted"]);
                    break;
                case "f2r6":
                    highlight("f2r6", true);
                    extraMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r6_highlighted"]);
                    break;
                case "f2r7":
                    highlight("f2r7", true);
                    extraMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r7_highlighted"]);
                    break;
                case "f2r8":
                    highlight("f2r8", true);
                    extraMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r8_highlighted"]);
                    break;
                case "f3r1":
                    highlight("f3r1", true);
                    extraInnerRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f3r1_highlighted"]);
                    break;
                case "f3r2":
                    highlight("f3r2", true);
                    extraInnerRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f3r2_highlighted"]);
                    break;
                case "f3r3":
                    highlight("f3r3", true);
                    extraInnerRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f3r3_highlighted"]);
                    break;
                case "f3r4":
                    highlight("f3r4", true);
                    extraInnerRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f3r4_highlighted"]);
                    break;
                default:
                    break;
            }



            //center window
            CurWindow.HorizontalAlign = HorizontalAlign.Center;
            CurWindow.VerticalAlign = VerticalAlign.Center;

            //if client and not editor: receive new ring status update
            if (!button.IsServer && !(EntitySystemWorld.Instance.WorldSimulationType == WorldSimulationTypes.Editor))
            {
                button.Client_SendWindowData((UInt16)NetworkMessages.Server_UpdateNewestRingRotationsForClient);
                button.Client_SendWindowData((UInt16)NetworkMessages.Server_UpdateLightsForClient);
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

        private void Server_initializeWithRings()
        {
            if(button.IsServer)
            {
                ringOuter = ((Ring)Entities.Instance.GetByName("F1_Ring"));
                ringInner = ((Ring)Entities.Instance.GetByName("F3_Ring"));
                ringMiddle = ((Ring)Entities.Instance.GetByName("F2_Ring"));

                ringOuter.RotateRing += Server_OnOuterRotation;
                ringInner.RotateRing += Server_OnInnerRotation;
                ringMiddle.RotateRing += Server_OnMiddleRotation;

                Server_updateRotationsAndLights();

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

        //used for server initialization, to get the actual ring statuses
        private void Server_updateRotationsAndLights()
        {
            if (button.IsServer)
            {
                //dummies..
                Vec3 pos = new Vec3();
                Quat rotation = new Quat();

                //optimized rotation
                for (int i = 0; i < Computer.Instance.RingRotations.Length; i++)
                {
                    int dist = Math.Abs(Computer.Instance.RingRotations[i] - currRingRotations[i]);      //Distance between curr and newest, if dist=0 no rotation needed
                    if (dist == 4)                                                              //If distance is in the 'middle', it doesnt matter if you rotate left or right
                    {
                        switch (i)
                        {
                            case 0:
                                for (int x = 0; x < 4; x++)
                                    Server_OnOuterRotation(pos, rotation, true);    //also sends to clients
                                break;
                            case 1:
                                for (int x = 0; x < 4; x++)
                                    Server_OnMiddleRotation(pos, rotation, true);   //also sends to clients
                                break;
                            case 2:
                                for (int x = 0; x < 4; x++)
                                    Server_OnInnerRotation(pos, rotation, true);    //also sends to clients
                                break;
                        }
                    }
                    if ((dist < 4 && currRingRotations[i] < Computer.Instance.RingRotations[i]) ||     //if this applies, rotate right: e.g.
                        (dist > 4 && currRingRotations[i] > Computer.Instance.RingRotations[i]))       //curr=4,new=7  dist=3 curr<new || curr=7,new=0  dist=7 curr>new: rotate right
                    {
                        switch (i)
                        {
                            case 0:
                                for (int x = currRingRotations[i]; x != Computer.Instance.RingRotations[i]; x = mod(x - 1, 8))
                                    Server_OnOuterRotation(pos, rotation, false);   //also sends to clients
                                break;
                            case 1:
                                for (int x = currRingRotations[i]; x != Computer.Instance.RingRotations[i]; x = mod(x - 1, 8))
                                    Server_OnMiddleRotation(pos, rotation, false);  //also sends to clients
                                break;
                            case 2:
                                for (int x = currRingRotations[i]; x != Computer.Instance.RingRotations[i]; x = mod(x - 1, 8))
                                    Server_OnInnerRotation(pos, rotation, false);   //also sends to clients
                                break;
                        }
                    }
                    if ((dist < 4 && currRingRotations[i] > Computer.Instance.RingRotations[i]) ||     //if this applies, rotate left: e.g.
                        (dist > 4 && currRingRotations[i] < Computer.Instance.RingRotations[i]))       //curr=0,new=7  dist=7 curr<new: || curr=7,new=4  dist=3 curr>new: rotate left
                    {
                        switch (i)
                        {
                            case 0:
                                for (int x = currRingRotations[i]; x != Computer.Instance.RingRotations[i]; x = mod(x + 1, 8))
                                    Server_OnOuterRotation(pos, rotation, true);    //also sends to clients
                                break;
                            case 1:
                                for (int x = currRingRotations[i]; x != Computer.Instance.RingRotations[i]; x = mod(x + 1, 8))
                                    Server_OnMiddleRotation(pos, rotation, true);   //also sends to clients
                                break;
                            case 2:
                                for (int x = currRingRotations[i]; x != Computer.Instance.RingRotations[i]; x = mod(x + 1, 8))
                                    Server_OnInnerRotation(pos, rotation, true);    //also sends to clients
                                break;
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

                //send update to client
                if (((SectorGroup)Entities.Instance.GetByName("F1SG-A")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnALightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnALightsOff);
                if (((SectorGroup)Entities.Instance.GetByName("F1SG-B")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnBLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnBLightsOff);
                if (((SectorGroup)Entities.Instance.GetByName("F1SG-C")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnCLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnCLightsOff);
                if (((SectorGroup)Entities.Instance.GetByName("F2SG-D")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnDLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnDLightsOff);
                if (((SectorGroup)Entities.Instance.GetByName("F2SG-E")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnELightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnELightsOff);
                if (((SectorGroup)Entities.Instance.GetByName("F3SG-F")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnFLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnFLightsOff);
                if (((SectorGroup)Entities.Instance.GetByName("F3SG-G")).LightStatus)
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnGLightsOn);
                else
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnGLightsOff);


            }
        }

        //Zeigt oder versteckt das highlight Control fuer einen bestimmten Raum (z.B. "f1r3", true), case-sensitive
        private void highlight(String s, bool b)
        {
            ringFullCntrl.Controls["Highlight_Overlay"].Controls[s + "_highlighted"].Visible = b;
        }

        //used for client initialization
        private void Client_updateRotations()
        {
            if(!button.IsServer)
            {
                //check if curr == new
                bool done = false;
                if (newestRingRotations != null)
                {
                    for (int i = 0; i < newestRingRotations.Length; i++)
                    {
                        if (newestRingRotations[i] == currRingRotations[i])
                            done = true;
                        else
                        {
                            done = false;
                            break;
                        }

                    }

                    if (!done)
                    {   //Optimized rotation, analog to server
                        for (int i = 0; i < newestRingRotations.Length; i++)
                        {
                            int dist = Math.Abs(newestRingRotations[i] - currRingRotations[i]);
                            if (dist == 4)
                            {
                                switch (i)
                                {
                                    case 0:
                                        for (int x = 0; x < 4; x++)
                                            Client_OnOuterRotation(true);
                                        break;
                                    case 1:
                                        for (int x = 0; x < 4; x++)
                                            Client_OnMiddleRotation(true);
                                        break;
                                    case 2:
                                        for (int x = 0; x < 4; x++)
                                            Client_OnInnerRotation(true);
                                        break;
                                }
                            }
                            if ((dist < 4 && currRingRotations[i] < newestRingRotations[i]) ||
                                (dist > 4 && currRingRotations[i] > newestRingRotations[i]))
                            {
                                switch (i)
                                {
                                    case 0:
                                        for (int x = currRingRotations[i]; x != newestRingRotations[i]; x = mod(x + 1, 8))
                                            Client_OnOuterRotation(false);
                                        break;
                                    case 1:
                                        for (int x = currRingRotations[i]; x != newestRingRotations[i]; x = mod(x + 1, 8))
                                            Client_OnMiddleRotation(false);
                                        break;
                                    case 2:
                                        for (int x = currRingRotations[i]; x != newestRingRotations[i]; x = mod(x + 1, 8))
                                            Client_OnInnerRotation(false);
                                        break;
                                }
                            }
                            if ((dist < 4 && currRingRotations[i] > newestRingRotations[i]) ||     
                                (dist > 4 && currRingRotations[i] < newestRingRotations[i]))
                            {
                                switch (i)
                                {
                                    case 0:
                                        for (int x = currRingRotations[i]; x != newestRingRotations[i]; x = mod(x - 1, 8))
                                            Client_OnOuterRotation(true);
                                        break;
                                    case 1:
                                        for (int x = currRingRotations[i]; x != newestRingRotations[i]; x = mod(x - 1, 8))
                                            Client_OnMiddleRotation(true);
                                        break;
                                    case 2:
                                        for (int x = currRingRotations[i]; x != newestRingRotations[i]; x = mod(x - 1, 8))
                                            Client_OnInnerRotation(true);
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void Server_OnSwitchLightsG(bool status)
        {
            if (button.IsServer)
            {
                if (status)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnGLightsOn);
                }
                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnGLightsOff);
                }


                secgrpGCntrl.Visible = status;
            }
        }

        private void Server_OnSwitchLightsF(bool status)
        {
            if (button.IsServer)
            {
                if (status)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnFLightsOn);
                }
                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnFLightsOff);
                }

                secgrpFCntrl.Visible = status;
            }
        }

        private void Server_OnSwitchLightsE(bool status)
        {
            if (button.IsServer)
            {
                if (status)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnELightsOn);
                }
                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnELightsOff);
                }

                secgrpECntrl.Visible = status;
            }
        }

        private void Server_OnSwitchLightsD(bool status)
        {
            if (button.IsServer)
            {
                if (status)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnDLightsOn);
                }
                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnDLightsOff);
                }

                secgrpDCntrl.Visible = status;
            }
        }

        private void Server_OnSwitchLightsC(bool status)
        {
            if (button.IsServer)
            {
                if (status)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnCLightsOn);
                }
                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnCLightsOff);
                }

                secgrpCCntrl.Visible = status;
            }
        }

        private void Server_OnSwitchLightsB(bool status)
        {
            if (button.IsServer)
            {
                if (status)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnBLightsOn);
                }
                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnBLightsOff);
                }

                secgrpBCntrl.Visible = status;
            }
        }

        private void Server_OnSwitchLightsA(bool status)
        {
            if (button.IsServer)
            {
                if (status)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnALightsOn);
                }
                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnALightsOff);
                }

                secgrpAR7Cntrl.Visible = status;
                secgrpAR8Cntrl.Visible = status;
            }
        }

        private void Server_OnInnerRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (button.IsServer)
            {
                if (!left)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_RotateInnerRingRight);

                    ringInnerCntrl.RotateDegree = mod(ringInnerCntrl.RotateDegree + 45, 360);
                    secgrpFCntrl.RotateDegree = mod(secgrpFCntrl.RotateDegree + 45, 360);
                    secgrpGCntrl.RotateDegree = mod(secgrpGCntrl.RotateDegree + 45, 360);
                    foreach (RotControl r in extraInnerRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree + 45, 360);
                    }
                    currRingRotations[2] = mod(currRingRotations[2] + 1, 8);
                }
                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_RotateInnerRingLeft);

                    ringInnerCntrl.RotateDegree = mod(ringInnerCntrl.RotateDegree - 45, 360);
                    secgrpFCntrl.RotateDegree = mod(secgrpFCntrl.RotateDegree - 45, 360);
                    secgrpGCntrl.RotateDegree = mod(secgrpGCntrl.RotateDegree - 45, 360);
                    foreach (RotControl r in extraInnerRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree - 45, 360);
                    }
                    currRingRotations[2] = mod(currRingRotations[2] - 1, 8);
                }
            }
        }

        private void Server_OnMiddleRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (button.IsServer)
            {
                if (!left)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_RotateMiddleRingRight);

                    ringMiddleCntrl.RotateDegree = mod(ringMiddleCntrl.RotateDegree + 45, 360);
                    secgrpAR7Cntrl.RotateDegree = mod(secgrpAR7Cntrl.RotateDegree + 45, 360);
                    secgrpECntrl.RotateDegree = mod(secgrpECntrl.RotateDegree + 45, 360);
                    secgrpDCntrl.RotateDegree = mod(secgrpDCntrl.RotateDegree + 45, 360);
                    foreach (RotControl r in extraMiddleRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree + 45, 360);
                    }
                    currRingRotations[1] = mod(currRingRotations[1] + 1, 8);
                }

                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_RotateMiddleRingLeft);

                    ringMiddleCntrl.RotateDegree = mod(ringMiddleCntrl.RotateDegree - 45, 360);
                    secgrpAR7Cntrl.RotateDegree = mod(secgrpAR7Cntrl.RotateDegree - 45, 360);
                    secgrpECntrl.RotateDegree = mod(secgrpECntrl.RotateDegree - 45, 360);
                    secgrpDCntrl.RotateDegree = mod(secgrpDCntrl.RotateDegree - 45, 360);
                    foreach (RotControl r in extraMiddleRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree - 45, 360);
                    }
                    currRingRotations[1] = mod(currRingRotations[1] - 1, 8);
                }
            }
        }

        private void Server_OnOuterRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (button.IsServer)
            {
                if (!left)
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_RotateOuterRingRight);

                    ringOuterCntrl.RotateDegree = mod(ringOuterCntrl.RotateDegree + 45, 360);

                    secgrpAR8Cntrl.RotateDegree = mod(secgrpAR8Cntrl.RotateDegree + 45, 360);
                    secgrpBCntrl.RotateDegree = mod(secgrpBCntrl.RotateDegree + 45, 360);
                    secgrpCCntrl.RotateDegree = mod(secgrpCCntrl.RotateDegree + 45, 360);
                    foreach (RotControl r in extraOuterRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree + 45, 360);
                    }
                    currRingRotations[0] = mod(currRingRotations[0] + 1, 8);
                }
                else
                {
                    button.Server_SendWindowData((UInt16)NetworkMessages.Client_RotateOuterRingLeft);

                    ringOuterCntrl.RotateDegree = mod(ringOuterCntrl.RotateDegree - 45, 360);

                    secgrpAR8Cntrl.RotateDegree = mod(secgrpAR8Cntrl.RotateDegree - 45, 360);
                    secgrpBCntrl.RotateDegree = mod(secgrpBCntrl.RotateDegree - 45, 360);
                    secgrpCCntrl.RotateDegree = mod(secgrpCCntrl.RotateDegree - 45, 360);
                    foreach (RotControl r in extraOuterRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree - 45, 360);
                    }
                    currRingRotations[0] = mod(currRingRotations[0] - 1, 8);
                }
            }
        }


        private void Server_WindowDataReceived(ushort message)
        {
            if (button.IsServer)
            {
                NetworkMessages msg = (NetworkMessages)message;
                switch (msg)
                {
                    case NetworkMessages.Server_UpdateNewestRingRotationsForClient:
                        string ringRotationsAsString = string.Join(",", Computer.Instance.RingRotations); // e.g. "1,7,3" where 1 of F3, 7 of F2, 3 of F1
                        button.Server_SendWindowString(ringRotationsAsString, (UInt16)NetworkMessages.Client_ReceiveNewestRingRotations);
                        break;
                    case NetworkMessages.Server_UpdateLightsForClient:
                        if (((SectorGroup)Entities.Instance.GetByName("F1SG-A")).LightStatus)
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnALightsOn);
                        else
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnALightsOff);
                        if (((SectorGroup)Entities.Instance.GetByName("F1SG-B")).LightStatus)
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnBLightsOn);
                        else
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnBLightsOff);
                        if (((SectorGroup)Entities.Instance.GetByName("F1SG-C")).LightStatus)
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnCLightsOn);
                        else
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnCLightsOff);
                        if (((SectorGroup)Entities.Instance.GetByName("F2SG-D")).LightStatus)
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnDLightsOn);
                        else
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnDLightsOff);
                        if (((SectorGroup)Entities.Instance.GetByName("F2SG-E")).LightStatus)
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnELightsOn);
                        else
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnELightsOff);
                        if (((SectorGroup)Entities.Instance.GetByName("F3SG-F")).LightStatus)
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnFLightsOn);
                        else
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnFLightsOff);
                        if (((SectorGroup)Entities.Instance.GetByName("F3SG-G")).LightStatus)
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnGLightsOn);
                        else
                            button.Server_SendWindowData((UInt16)NetworkMessages.Client_TurnGLightsOff);

                        break;
                }
            }
        }



        private void Client_StringReceived(string message, UInt16 netMessage)
        {
            if (!button.IsServer)
            {
                NetworkMessages msg = (NetworkMessages)netMessage;
                switch (msg)
                {
                    case NetworkMessages.Client_ReceiveNewestRingRotations:
                        //check if new string is 'legal'
                        string[] stringTempArray = message.Split(',');
                        if (stringTempArray.Length == 3)
                        {
                            int[] intTempArray = Array.ConvertAll(stringTempArray, int.Parse);
                            if (intTempArray[0] <= 7 && intTempArray[1] <= 7 && intTempArray[2] <= 7 && intTempArray[0] >= 0 && intTempArray[1] >= 0 && intTempArray[2] >= 0)
                            {
                                newestRingRotations = intTempArray; //if legal: assign to newestRingRotations
                            }
                        }
                        Client_updateRotations();   //and update the ring controls
                        break;
                        
                }
            }
        }

        private void Client_WindowDataReceived(ushort message)
        {
            if (!button.IsServer)
            {
                NetworkMessages msg = (NetworkMessages)message;
                switch (msg)
                {
                    case NetworkMessages.Client_TurnALightsOff:
                        Client_OnSwitchLightsA(false);
                        break;
                    case NetworkMessages.Client_TurnALightsOn:
                        Client_OnSwitchLightsA(true);
                        break;
                    case NetworkMessages.Client_TurnBLightsOff:
                        Client_OnSwitchLightsB(false);
                        break;
                    case NetworkMessages.Client_TurnBLightsOn:
                        Client_OnSwitchLightsB(true);
                        break;
                    case NetworkMessages.Client_TurnCLightsOff:
                        Client_OnSwitchLightsC(false);
                        break;
                    case NetworkMessages.Client_TurnCLightsOn:
                        Client_OnSwitchLightsC(true);
                        break;
                    case NetworkMessages.Client_TurnDLightsOff:
                        Client_OnSwitchLightsD(false);
                        break;
                    case NetworkMessages.Client_TurnDLightsOn:
                        Client_OnSwitchLightsD(true);
                        break;
                    case NetworkMessages.Client_TurnELightsOff:
                        Client_OnSwitchLightsE(false);
                        break;
                    case NetworkMessages.Client_TurnELightsOn:
                        Client_OnSwitchLightsE(true);
                        break;
                    case NetworkMessages.Client_TurnFLightsOff:
                        Client_OnSwitchLightsF(false);
                        break;
                    case NetworkMessages.Client_TurnFLightsOn:
                        Client_OnSwitchLightsF(true);
                        break;
                    case NetworkMessages.Client_TurnGLightsOff:
                        Client_OnSwitchLightsG(false);
                        break;
                    case NetworkMessages.Client_TurnGLightsOn:
                        Client_OnSwitchLightsG(true);
                        break;
                    case NetworkMessages.Client_RotateInnerRingLeft:
                        Client_OnInnerRotation(true);
                        break;
                    case NetworkMessages.Client_RotateInnerRingRight:
                        Client_OnInnerRotation(false);
                        break;
                    case NetworkMessages.Client_RotateOuterRingLeft:
                        Client_OnOuterRotation(true);
                        break;
                    case NetworkMessages.Client_RotateOuterRingRight:
                        Client_OnOuterRotation(false);
                        break;
                    case NetworkMessages.Client_RotateMiddleRingLeft:
                        Client_OnMiddleRotation(true);
                        break;
                    case NetworkMessages.Client_RotateMiddleRingRight:
                        Client_OnMiddleRotation(false);
                        break;
                }
            }
        }

        private void Client_OnSwitchLightsG(bool status)
        {
            if (!button.IsServer)
            {
                secgrpGCntrl.Visible = status;
            }
        }

        private void Client_OnSwitchLightsF(bool status)
        {
            if (!button.IsServer)
            {
                secgrpFCntrl.Visible = status;
            }
        }

        private void Client_OnSwitchLightsE(bool status)
        {
            if (!button.IsServer)
            {
                secgrpECntrl.Visible = status;
            }
        }

        private void Client_OnSwitchLightsD(bool status)
        {
            if (!button.IsServer)
            {
                secgrpDCntrl.Visible = status;
            }
        }

        private void Client_OnSwitchLightsC(bool status)
        {
            if (!button.IsServer)
            {
                secgrpCCntrl.Visible = status;
            }
        }

        private void Client_OnSwitchLightsB(bool status)
        {
            if (!button.IsServer)
            {
                secgrpBCntrl.Visible = status;
            }
        }

        private void Client_OnSwitchLightsA(bool status)
        {
            if (!button.IsServer)
            {
                secgrpAR7Cntrl.Visible = status;
                secgrpAR8Cntrl.Visible = status;
            }
        }

        private void Client_OnInnerRotation(bool left)
        {
            if (!button.IsServer)
            {
                if (!left)
                {
                    ringInnerCntrl.RotateDegree = mod(ringInnerCntrl.RotateDegree + 45, 360);
                    secgrpFCntrl.RotateDegree = mod(secgrpFCntrl.RotateDegree + 45, 360);
                    secgrpGCntrl.RotateDegree = mod(secgrpGCntrl.RotateDegree + 45, 360);
                    foreach (RotControl r in extraInnerRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree + 45, 360);
                    }
                    currRingRotations[2] = mod(currRingRotations[2] + 1, 8);
                }
                else
                {
                    ringInnerCntrl.RotateDegree = mod(ringInnerCntrl.RotateDegree - 45, 360);
                    secgrpFCntrl.RotateDegree = mod(secgrpFCntrl.RotateDegree - 45, 360);
                    secgrpGCntrl.RotateDegree = mod(secgrpGCntrl.RotateDegree - 45, 360);
                    foreach (RotControl r in extraInnerRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree - 45, 360);
                    }
                    currRingRotations[2] = mod(currRingRotations[2] - 1, 8);
                }
            }
        }

        private void Client_OnMiddleRotation(bool left)
        {
            if (!button.IsServer)
            {
                if (!left)
                {
                    ringMiddleCntrl.RotateDegree = mod(ringMiddleCntrl.RotateDegree + 45, 360);
                    secgrpAR7Cntrl.RotateDegree = mod(secgrpAR7Cntrl.RotateDegree + 45, 360);
                    secgrpECntrl.RotateDegree = mod(secgrpECntrl.RotateDegree + 45, 360);
                    secgrpDCntrl.RotateDegree = mod(secgrpDCntrl.RotateDegree + 45, 360);
                    foreach (RotControl r in extraMiddleRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree + 45, 360);
                    }
                    currRingRotations[1] = mod(currRingRotations[1] + 1, 8);
                }

                else
                {
                    ringMiddleCntrl.RotateDegree = mod(ringMiddleCntrl.RotateDegree - 45, 360);
                    secgrpAR7Cntrl.RotateDegree = mod(secgrpAR7Cntrl.RotateDegree - 45, 360);
                    secgrpECntrl.RotateDegree = mod(secgrpECntrl.RotateDegree - 45, 360);
                    secgrpDCntrl.RotateDegree = mod(secgrpDCntrl.RotateDegree - 45, 360);
                    foreach (RotControl r in extraMiddleRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree - 45, 360);
                    }
                    currRingRotations[1] = mod(currRingRotations[1] - 1, 8);
                }
            }
        }

        private void Client_OnOuterRotation(bool left)
        {
            if (!button.IsServer)
            {
                if (!left)
                {
                    ringOuterCntrl.RotateDegree = mod(ringOuterCntrl.RotateDegree + 45, 360);

                    secgrpAR8Cntrl.RotateDegree = mod(secgrpAR8Cntrl.RotateDegree + 45, 360);
                    secgrpBCntrl.RotateDegree = mod(secgrpBCntrl.RotateDegree + 45, 360);
                    secgrpCCntrl.RotateDegree = mod(secgrpCCntrl.RotateDegree + 45, 360);
                    foreach (RotControl r in extraOuterRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree + 45, 360);
                    }
                    currRingRotations[0] = mod(currRingRotations[0] + 1, 8);
                }
                else
                {
                    ringOuterCntrl.RotateDegree = mod(ringOuterCntrl.RotateDegree - 45, 360);

                    secgrpAR8Cntrl.RotateDegree = mod(secgrpAR8Cntrl.RotateDegree - 45, 360);
                    secgrpBCntrl.RotateDegree = mod(secgrpBCntrl.RotateDegree - 45, 360);
                    secgrpCCntrl.RotateDegree = mod(secgrpCCntrl.RotateDegree - 45, 360);
                    foreach (RotControl r in extraOuterRingControls)
                    {
                        r.RotateDegree = mod(r.RotateDegree - 45, 360);
                    }
                    currRingRotations[0] = mod(currRingRotations[0] - 1, 8);
                }
            }
        }

        private int mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        private float mod(float x, float m)
        {
            return (x % m + m) % m;
        }
    }
}
