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
	public class MenuWindow : Control
	{
        Control window;
        
		protected override void OnAttach()
		{
			base.OnAttach();
            
			window = ControlDeclarationManager.Instance.CreateControl( "Gui\\MenuWindow.gui" );
			Controls.Add( window );

			//( (Button)window.Controls[ "LoadSave" ] ).Click += loadSaveButton_Click;
			( (Button)window.Controls[ "Options" ] ).Click += optionsButton_Click;
			( (Button)window.Controls[ "ProfilingTool" ] ).Click += ProfilingToolButton_Click;
			( (Button)window.Controls[ "About" ] ).Click += aboutButton_Click;
			( (Button)window.Controls[ "ExitToMainMenu" ] ).Click += exitToMainMenuButton_Click;
			( (Button)window.Controls[ "Exit" ] ).Click += exitButton_Click;
			( (Button)window.Controls[ "Resume" ] ).Click += resumeButton_Click;

			if( GameWindow.Instance == null )
				window.Controls[ "ExitToMainMenu" ].Enable = false;

			if( GameNetworkServer.Instance != null || GameNetworkClient.Instance != null )
				window.Controls[ "LoadSave" ].Enable = false;

			MouseCover = true;

			BackColor = new ColorValue( 0, 0, 0, .5f );
		}

		void loadSaveButton_Click( object sender )
		{
			foreach( Control control in Controls )
				control.Visible = false;
			//Controls.Add( new WorldLoadSaveWindow() );
		}

		void optionsButton_Click( object sender )
		{
			foreach( Control control in Controls )
				control.Visible = false;
			Controls.Add( new OptionsWindow() );
		}

		void ProfilingToolButton_Click( object sender )
		{
			SetShouldDetach();
			GameEngineApp.ShowProfilingTool( true );
		}

		void aboutButton_Click( object sender )
		{
			foreach( Control control in Controls )
				control.Visible = false;
			Controls.Add( new AboutWindow() );
		}

		protected override void OnControlDetach( Control control )
		{
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

		void exitToMainMenuButton_Click( object sender )
		{
			MapSystemWorld.MapDestroy();
			EntitySystemWorld.Instance.WorldDestroy();

			GameEngineApp.Instance.Server_DestroyServer( "The server has been destroyed" );
			GameEngineApp.Instance.Client_DisconnectFromServer();

			//close all windows
			foreach( Control control in GameEngineApp.Instance.ControlManager.Controls )
				control.SetShouldDetach();
			//create main menu
			GameEngineApp.Instance.ControlManager.Controls.Add( new MainMenuWindow() );
		}
        bool isinarea(Button button, Vec2 pos) {

            return button.GetScreenRectangle().IsContainsPoint(pos);
        }

        public void workbench_Click(Vec2 mousepos) {

            if (isinarea((Button)window.Controls["Resume"],mousepos))
            {
                resumeButton_Click(window.Controls["Resume"]);
            }
            else if (isinarea((Button)window.Controls["ExitToMainMenu"], mousepos))
            {
                exitToMainMenuButton_Click(window.Controls["ExitToMainMenu"]);
            }
            else if (isinarea((Button)window.Controls["Exit"], mousepos))
            {
                exitButton_Click(window.Controls["Exit"]);
            }
            else if (isinarea((Button)window.Controls["Options"], mousepos) || 
                isinarea((Button)window.Controls["ProfilingTool"], mousepos) ||
                isinarea((Button)window.Controls["About"], mousepos))
            {
                StatusMessageHandler.sendMessage("Diese Option ist nur für die Maus verfügbar");
            }
        }
		void exitButton_Click( object sender )
		{
			GameEngineApp.Instance.SetFadeOutScreenAndExit();
		}

		void resumeButton_Click( object sender )
		{
			SetShouldDetach();
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
