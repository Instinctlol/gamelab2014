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
        private List<RotControl> highlightedMiddleRingControls = new List<RotControl>();
        private List<RotControl> highlightedInnerRingControls = new List<RotControl>();
        private List<RotControl> highlightedOuterRingControls = new List<RotControl>();
        private SectorGroup secgrpA, secgrpB, secgrpC, secgrpD, secgrpE, secgrpF, secgrpG;
        private int[] currRingRotations=new int[3];

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

            
            for(int i=1; i<=8; i++)
            {
                highlightedOuterRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f1r" + i + "_highlighted"]);
                highlightedMiddleRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f2r" + i + "_highlighted"]);
                if(i<=4)
                    highlightedInnerRingControls.Add((RotControl)ringFullCntrl.Controls["Highlight_Overlay"].Controls["f3r" + i + "_highlighted"]);
            }

            for(int i=0; i<currRingRotations.Length; i++)
                currRingRotations[i]=0;

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

            //dummies..
            Vec3 pos = new Vec3();
            Quat rotation = new Quat();

            //optimized rotation
            for (int i = 0; i < Computer.RingRotations.Length; i++)
            {
                int dist = Math.Abs(Computer.RingRotations[i] - currRingRotations[i]);      //Distance between curr and newest, if dist=0 no rotation needed
                if (dist == 4)                                                              //If distance is in the 'middle', it doesnt matter if you rotate left or right
                {
                    switch (i)
                    {
                        case 0:
                            for (int x = 0; x < 4; x++)
                                OnOuterRotation(pos, rotation, true);    //also sends to clients
                            break;
                        case 1:
                            for (int x = 0; x < 4; x++)
                                OnMiddleRotation(pos, rotation, true);   //also sends to clients
                            break;
                        case 2:
                            for (int x = 0; x < 4; x++)
                                OnInnerRotation(pos, rotation, true);    //also sends to clients
                            break;
                    }
                }
                if ((dist < 4 && currRingRotations[i] < Computer.RingRotations[i]) ||     //if this applies, rotate right: e.g.
                    (dist > 4 && currRingRotations[i] > Computer.RingRotations[i]))       //curr=4,new=7  dist=3 curr<new || curr=7,new=0  dist=7 curr>new: rotate right
                {
                    switch (i)
                    {
                        case 0:
                            for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x - 1, 8))
                                OnOuterRotation(pos, rotation, false);   //also sends to clients
                            break;
                        case 1:
                            for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x - 1, 8))
                                OnMiddleRotation(pos, rotation, false);  //also sends to clients
                            break;
                        case 2:
                            for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x - 1, 8))
                                OnInnerRotation(pos, rotation, false);   //also sends to clients
                            break;
                    }
                }
                if ((dist < 4 && currRingRotations[i] > Computer.RingRotations[i]) ||     //if this applies, rotate left: e.g.
                    (dist > 4 && currRingRotations[i] < Computer.RingRotations[i]))       //curr=0,new=7  dist=7 curr<new: || curr=7,new=4  dist=3 curr>new: rotate left
                {
                    switch (i)
                    {
                        case 0:
                            for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x + 1, 8))
                                OnOuterRotation(pos, rotation, true);    //also sends to clients
                            break;
                        case 1:
                            for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x + 1, 8))
                                OnMiddleRotation(pos, rotation, true);   //also sends to clients
                            break;
                        case 2:
                            for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x + 1, 8))
                                OnInnerRotation(pos, rotation, true);    //also sends to clients
                            break;
                    }
                }


            }

            // Ringe drehen entsprechend der Computer-Konfig
            //Vec3 pos = new Vec3();
            //Quat rotation = new Quat();

            //for (int i = 0; i < Computer.RingRotations.Length; i++)
            //{
            //    int dist = Math.Abs(Computer.RingRotations[i] - currRingRotations[i]);      //Distanz zwischen curr und newest, wenn dist=0 braucht nicht gedreht werden
            //    if (dist == 4)                                                              //Distanz in der Mitte: 4x egal wohin drehen
            //    {
            //        switch (i)
            //        {
            //            case 0:
            //                for (int x = 0; x < 4; x++)
            //                    OnOuterRotation(pos, rotation, true);
            //                break;
            //            case 1:
            //                for (int x = 0; x < 4; x++)
            //                    OnMiddleRotation(pos, rotation, true);
            //                break;
            //            case 2:
            //                for (int x = 0; x < 4; x++)
            //                    OnInnerRotation(pos, rotation, true);
            //                break;
            //        }
            //    }
            //    if ((dist < 4 && currRingRotations[i] < Computer.RingRotations[i]) ||     //Rechts drehen wenn das gilt. Beispiel:
            //        (dist > 4 && currRingRotations[i] > Computer.RingRotations[i]))       //curr=4,new=7  dist=3 curr<new || curr=7,new=0  dist=7 curr>new: rechts drehen
            //    {
            //        switch (i)
            //        {
            //            case 0:
            //                for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x + 1, 8))
            //                    OnOuterRotation(pos, rotation, false);
            //                break;
            //            case 1:
            //                for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x + 1, 8))
            //                    OnMiddleRotation(pos, rotation, false);
            //                break;
            //            case 2:
            //                for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x + 1, 8))
            //                    OnInnerRotation(pos, rotation, false);
            //                break;
            //        }
            //    }
            //    if ((dist < 4 && currRingRotations[i] > Computer.RingRotations[i]) ||     //Links drehen wenn das gilt. Beispiel:
            //        (dist > 4 && currRingRotations[i] < Computer.RingRotations[i]))       //curr=0,new=7  dist=7 curr<new: || curr=7,new=4  dist=3 curr>new: links drehen
            //    {
            //        switch (i)
            //        {
            //            case 0:
            //                for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x - 1, 8))
            //                    OnOuterRotation(pos, rotation, true);
            //                break;
            //            case 1:
            //                for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x - 1, 8))
            //                    OnMiddleRotation(pos, rotation, true);
            //                break;
            //            case 2:
            //                for (int x = currRingRotations[i]; x != Computer.RingRotations[i]; x = mod(x - 1, 8))
            //                    OnInnerRotation(pos, rotation, true);
            //                break;
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
            secgrpECntrl.Visible = status;
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

        //Zeigt oder versteckt das highlight Control fuer einen bestimmten Raum (z.B. "f1r3", true), case-sensitive
        public void highlight(String s, bool b)
        {
            ringFullCntrl.Controls["Highlight_Overlay"].Controls[s+"_highlighted"].Visible=b;
        }


        private void OnInnerRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
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
                currRingRotations[2] = mod(currRingRotations[2] + 1, 8);
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
                currRingRotations[2] = mod(currRingRotations[2] - 1, 8);
            }
        }

        private void OnMiddleRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
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
                currRingRotations[1] = mod(currRingRotations[1] + 1, 8);
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
                currRingRotations[1] = mod(currRingRotations[1] - 1, 8);
            }
        }

        private void OnOuterRotation(Engine.MathEx.Vec3 pos, Engine.MathEx.Quat rot, bool left)
        {
            if (!left)
            {

                EngineConsole.Instance.Print("sectorwindow rechts drehen");
                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree + 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree + 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree + 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree + 45) % 360;
                foreach (RotControl r in highlightedOuterRingControls)
                {
                    r.RotateDegree = (r.RotateDegree + 45) % 360;
                }
                currRingRotations[0] = mod(currRingRotations[0] + 1, 8);
            }
            else
            {
                EngineConsole.Instance.Print("sectorwindow links drehen");
                ringOuterCntrl.RotateDegree = (ringOuterCntrl.RotateDegree - 45) % 360;

                secgrpAR8Cntrl.RotateDegree = (secgrpAR8Cntrl.RotateDegree - 45) % 360;
                secgrpBCntrl.RotateDegree = (secgrpBCntrl.RotateDegree - 45) % 360;
                secgrpCCntrl.RotateDegree = (secgrpCCntrl.RotateDegree - 45) % 360;
                foreach (RotControl r in highlightedOuterRingControls)
                {
                    r.RotateDegree = (r.RotateDegree - 45) % 360;
                }
                currRingRotations[0] = mod(currRingRotations[0] - 1, 8);
            }
        }

        private int mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }
}
