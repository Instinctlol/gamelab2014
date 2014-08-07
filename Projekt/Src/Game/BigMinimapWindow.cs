// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Engine.UISystem;
using Engine.MapSystem;
using Engine.EntitySystem;
using Engine.MathEx;
using ProjectCommon;
using ProjectEntities;

namespace Game
{
	/// <summary>
	/// Defines a system game menu.
	/// </summary>
	public class BigMinimapWindow : Control
	{
        // BigMinimap mode
        Sector selectedSector;
        Control bigMinimapControl;
        string lastSelectedSector;
        Control window;

        public BigMinimapWindow(Control control)
        {
            window = control;

            // Big Minimap
            //initializing BigMinimap, only works on runtime
            bigMinimapControl = window.Controls["BigMinimap"];
            ((SectorStatusWindow)bigMinimapControl).initialize();
            //bigMinimapControl.MouseDoubleClick += BigMinimapClick;

            // Buttons
            /* ((Button)window.Controls["Close"]).Click += closeButton_Click;
            ((Button)window.Controls["RotateLeft"]).Click += rotateLeftButton_Click;
            ((Button)window.Controls["RotateRight"]).Click += rotateRightButton_Click;
            ((Button)window.Controls["Power"]).Click += powerButton_Click; */



            MouseCover = true;
            BackColor = new ColorValue(0, 0, 0, .5f);
        }

        // löschen
        protected override bool OnMouseDown(EMouseButtons button)
        {
            EngineConsole.Instance.Print("bigminimap mousedown");
            

            return base.OnMouseDown(button);
        }

        void closeButton_Click(object sender)
        {
            window.Visible = false;
            EngineConsole.Instance.Print("minimap clos");
            //SetShouldDetach();
            
        }

        void rotateLeftButton_Click(object sender)
        {
            EngineConsole.Instance.Print("links drehen");
            if (selectedSector == null)
            {
                StatusMessageHandler.sendMessage("Kein Sector eines Rings ausgewählt. (Raum mit Doppel-Click auswählen)");
            }
            else
            {
                Computer.RotateRing(selectedSector.Ring, true);
            }
        }

        void rotateRightButton_Click(object sender)
        {
            EngineConsole.Instance.Print("rechts drehen");
            if (selectedSector == null)
            {
                StatusMessageHandler.sendMessage("Kein Sector eines Rings ausgewählt. (Raum mit Doppel-Click auswählen)");
            }
            else
            {
                Computer.RotateRing(selectedSector.Ring, false);
            }
        }

		void powerButton_Click( object sender )
        {
            if (selectedSector == null)
            {
                StatusMessageHandler.sendMessage("Kein Sector ausgewählt. (Raum mit Doppel-Click auswählen)");
            }
            else
            {
                Computer.SetSectorGroupPower(selectedSector.Group, !selectedSector.Group.LightStatus);
                EngineConsole.Instance.Print("Turn Power off for secgrp: "+selectedSector.Group.Name);
            }
        }
        
		protected override void OnControlDetach( Control control )
		{
            EngineConsole.Instance.Print("oncoltroldetach");
			base.OnControlDetach( control );

			if( ( control as OptionsWindow ) != null ||
				//( control as MapsWindow ) != null ||
				//( control as WorldLoadSaveWindow ) != null ||
				( control as AboutWindow ) != null )
			{
				foreach( Control c in Controls )
					c.Visible = true;
			}
		}

        /// Sector finden anhand eines Klicks auf das Bild in der BigMinimap.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="button"></param>
        public void BigMinimapClick(Vec2 MousePosition)
        {
            EngineConsole.Instance.Print("binminimap bigminimapclick");
            Vec2 pos = GetMapPositionByMouseOnMinimap(MousePosition);
            Rect rect = new Rect(pos);
            Sphere sphere = new Sphere(new Vec3(pos.X, pos.Y, 0), 0.001f);
            // Sector finden
            Map.Instance.GetObjects(sphere, delegate(MapObject obj)
            {
                if (obj is Sector)
                {
                    selectedSector = (Sector)obj;
                    EngineConsole.Instance.Print("Sector: " + ((Sector)obj).Name);

                    if (lastSelectedSector != null)
                        ((SectorStatusWindow)bigMinimapControl).highlight("f" + lastSelectedSector.Substring(1, 1) + "r" + lastSelectedSector.Substring(3, 1), false);  //unhilight last sector
                    lastSelectedSector = selectedSector.Name;

                    ((SectorStatusWindow)bigMinimapControl).highlight("f" + selectedSector.Name.Substring(1, 1) + "r" + selectedSector.Name.Substring(3, 1), true); //hilight new sector
                    TuioInputDevice.detectgestures(true);
                    TuioInputDevice.cleardata();
                    
                }
            });
        }

        /// <summary>
        /// Gets the position in Map by Mouse Position in BigMinimap. So that we can get the sector.
        /// </summary>
        /// <returns></returns>
        Vec2 GetMapPositionByMouseOnMinimap(Vec2 MousePosition)
        {
            Rect screenMapRect = bigMinimapControl.GetScreenRectangle();

            Bounds initialBounds = Map.Instance.InitialCollisionBounds;

            // Die Vektoren für das Rechteck
            Vec2 vec1 = initialBounds.Minimum.ToVec2();
            Vec2 vec2 = new Vec2(Math.Abs(initialBounds.Minimum.ToVec2().X), Math.Abs(initialBounds.Minimum.ToVec2().Y));

            // 10% vergrößern
            vec1 = new Vec2(vec1.X * 1.1f, vec1.Y * 1.1f);
            vec2 = new Vec2(vec2.X * 1.1f, vec2.Y * 1.1f);

            Rect mapRect = new Rect(vec1, vec2);

            Vec2 point = MousePosition;
            point -= screenMapRect.Minimum;
            point /= screenMapRect.Size;
            point = new Vec2(point.X, 1.0f - point.Y);
            point *= mapRect.Size;
            point += mapRect.Minimum;

            return point;
        }

        public void workbench_Click(Vec2 mousepos)
        {

            if (isinarea((Button)window.Controls["Close"], mousepos))
            {
                closeButton_Click(window.Controls["Close"]);
            }
            else if (isinarea((Button)window.Controls["RotateLeft"], mousepos))
            {
                rotateLeftButton_Click(window.Controls["RotateLeft"]);
            }
            else if (isinarea((Button)window.Controls["RotateRight"], mousepos))
            {
                rotateRightButton_Click(window.Controls["RotateRight"]);
            }
            else if (isinarea((Button)window.Controls["Power"], mousepos))
            {
                powerButton_Click(window.Controls["Power"]);
            }
            else if (window.Controls["BigMinimap"].GetScreenRectangle().IsContainsPoint(mousepos))
            {
                BigMinimapClick(mousepos);
            }
        }

        public void workbench_gestures(opType evnt) {
            if (evnt == opType.iuhr) {
                rotateRightButton_Click(window.Controls["RotateRight"]);
            }

            if (evnt == opType.guhr)
            {
                rotateLeftButton_Click(window.Controls["RotateLeft"]);
            }
            if (evnt == opType.blitz)
            {
                powerButton_Click(window.Controls["Power"]); 
            }
        }
        bool isinarea(Button button, Vec2 pos)
        {

            return button.GetScreenRectangle().IsContainsPoint(pos);
        }
	}
}
