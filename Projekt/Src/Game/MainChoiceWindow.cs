// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Engine;
using Engine.FileSystem;
using Engine.UISystem;
using Engine.Renderer;
using Engine.MathEx;
using Engine.MapSystem;
using Engine.Utils;
using Engine.SoundSystem;
using ProjectCommon;
using ProjectEntities;

namespace Game
{
	/// <summary>
	/// Defines a window of options.
	/// </summary>
	public class MainChoiceWindow : Control
	{
		static int lastPageIndex;

		Control window;
		TabControl tabControl;
		Button[] pageButtons = new Button[ 2 ];

		ComboBox comboBoxResolution;
		ComboBox comboBoxInputDevices;
		CheckBox checkBoxDepthBufferAccess;
		ComboBox comboBoxAntialiasing;

		///////////////////////////////////////////

		class ComboBoxItem
		{
			string identifier;
			string displayName;

			public ComboBoxItem( string identifier, string displayName )
			{
				this.identifier = identifier;
				this.displayName = displayName;
			}

			public string Identifier
			{
				get { return identifier; }
			}

			public string DisplayName
			{
				get { return displayName; }
			}

			public override string ToString()
			{
				return displayName;
			}
		}

		///////////////////////////////////////////

		public class ShadowTechniqueItem
		{
			ShadowTechniques technique;
			string text;

			public ShadowTechniqueItem( ShadowTechniques technique, string text )
			{
				this.technique = technique;
				this.text = text;
			}

			public ShadowTechniques Technique
			{
				get { return technique; }
			}

			public override string ToString()
			{
				return text;
			}
		}

		///////////////////////////////////////////

		protected override void OnAttach()
		{
			base.OnAttach();

			window = ControlDeclarationManager.Instance.CreateControl( "Gui\\MainChoiceWindow.gui" );
			Controls.Add( window );

			BackColor = new ColorValue( 0, 0, 0, .5f );
			MouseCover = true;

			//load Engine.config
			TextBlock engineConfigBlock = LoadEngineConfig();
			TextBlock rendererBlock = null;
			if( engineConfigBlock != null )
				rendererBlock = engineConfigBlock.FindChild( "Renderer" );

			//page buttons
            ((Button)window.Controls["ButtonAlien"]).Click += RunAlien_Click;
            ((Button)window.Controls[ "ButtonAstronaut" ]).Click += RunAstronaut_Click;
            ((Button)window.Controls["Close"]).Click += delegate(Button sender)
            {
                SetShouldDetach();
            };
            ;
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

        void RunAlien_Click(Button sender)
        {
            GameEngineApp.Instance.SetNeedMapLoad("Maps\\GameLab_v01\\Map.map");
        }

        void RunAstronaut_Click(Button sender)
        {
            GameEngineApp.Instance.SetNeedMapLoad("Maps\\GameLab_v01\\Map.map");
        }

        TextBlock LoadEngineConfig()
		{
			string fileName = VirtualFileSystem.GetRealPathByVirtual( "user:Configs/Engine.config" );
			string error;
			return TextBlockUtils.LoadFromRealFile( fileName, out error );
		}

		void SaveEngineConfig( TextBlock engineConfigBlock )
		{
			string fileName = VirtualFileSystem.GetRealPathByVirtual( "user:Configs/Engine.config" );
			try
			{
				string directoryName = Path.GetDirectoryName( fileName );
				if( directoryName != "" && !Directory.Exists( directoryName ) )
					Directory.CreateDirectory( directoryName );
				using( StreamWriter writer = new StreamWriter( fileName ) )
				{
					writer.Write( engineConfigBlock.DumpToString() );
				}
			}
			catch( Exception e )
			{
				Log.Warning( "Unable to save file \"{0}\". {1}", fileName, e.Message );
			}
		}
	}
}
