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

		protected override void OnAttach()
		{
            EngineConsole.Instance.Print("bigmap onattach");
			base.OnAttach();
            
			Control window = ControlDeclarationManager.Instance.CreateControl( "Gui\\BigMinimap.gui" );
			Controls.Add( window );

            // Big Minimap
            //initializing BigMinimap, only works on runtime
            bigMinimapControl = window.Controls["BigMinimap"];
            ((SectorStatusWindow)bigMinimapControl).initialize();
            bigMinimapControl.MouseDoubleClick += BigMinimapClick;

            // Buttons
            ((Button)window.Controls["Close"]).Click += closeButton_Click;
            ((Button)window.Controls["RotateLeft"]).Click += rotateLeftButton_Click;
            ((Button)window.Controls["RotateRight"]).Click += rotateRightButton_Click;
            ((Button)window.Controls["Power"]).Click += powerButton_Click;
            
			MouseCover = true;
			BackColor = new ColorValue( 0, 0, 0, .5f );
		}

        void closeButton_Click(object sender)
        {
            SetShouldDetach();
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
                Computer.SetSectorGroupPower(selectedSector.Group);
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
        public void BigMinimapClick(Control sender, EMouseButtons button)
        {
            // hier
            Vec2 pos = GetMapPositionByMouseOnMinimap();
            Rect rect = new Rect(pos);
            Sphere sphere = new Sphere(new Vec3(pos.X, pos.Y, 0), 0.1f);
            // Sector finden
            Map.Instance.GetObjects(sphere, delegate(MapObject obj)
            {
                if (obj is Sector)
                {
                    selectedSector = (Sector)obj;
                    EngineConsole.Instance.Print("Sector: " + ((Sector)obj).Name);
                }
            });
            // TODO Rahmen um Sector anzeigen
        }

        /// <summary>
        /// Gets the position in Map by Mouse Position in BigMinimap. So that we can get the sector.
        /// </summary>
        /// <returns></returns>
        Vec2 GetMapPositionByMouseOnMinimap()
        {
            Rect screenMapRect = bigMinimapControl.GetScreenRectangle();

            Bounds initialBounds = Map.Instance.InitialCollisionBounds;

            Vec2 vec2 = new Vec2(Math.Abs(initialBounds.Minimum.ToVec2().X), Math.Abs(initialBounds.Minimum.ToVec2().Y));
            Rect mapRect = new Rect(initialBounds.Minimum.ToVec2(), vec2);

            Vec2 point = MousePosition;
            point -= screenMapRect.Minimum;
            point /= screenMapRect.Size;
            point = new Vec2(point.X, 1.0f - point.Y);
            point *= mapRect.Size;
            point += mapRect.Minimum;

            return point;
        }

		protected override bool OnKeyDown( KeyEvent e )
		{
			if( base.OnKeyDown( e ) )
				return true;
			if( e.Key == EKeys.Escape )
			{
				SetShouldDetach();
				return true;
			}
			return false;
		}
	}
}
