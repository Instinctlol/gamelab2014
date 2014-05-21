// Copyright (C) NeoAxis Group Ltd. This is part of NeoAxis 3D Engine SDK.
using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Engine.UISystem;
using Engine.MathEx;
using Engine.Renderer;
using ProjectEntities;

namespace Game
{
	/// <summary>
	/// Defines a about us window.
	/// </summary>
	public class AboutWindow : Control
	{
		protected override void OnAttach()
		{
			base.OnAttach();

			Control window = ControlDeclarationManager.Instance.CreateControl( "Gui\\AboutWindow.gui" );
			Controls.Add( window );

			window.Controls[ "Version" ].Text = EngineVersionInformation.Version;

			( (Button)window.Controls[ "Quit" ] ).Click += delegate( Button sender )
			{
				SetShouldDetach();
			};

			SDKInfo.SDKs sdk = SDKInfo.GetSDK();
			float alpha = .08f;

			if( sdk > SDKInfo.SDKs.Free )
			{
				window.Controls[ "FreeSDK" ].ColorMultiplier = new ColorValue( 1, 1, 1, alpha );
				window.Controls[ "FreeSDKBackground" ].ColorMultiplier = new ColorValue( 1, 1, 1, alpha );
			}
			if( sdk > SDKInfo.SDKs.Professional )
			{
				window.Controls[ "ProfessionalSDK" ].ColorMultiplier = new ColorValue( 1, 1, 1, alpha );
				window.Controls[ "ProfessionalSDKBackground" ].ColorMultiplier = new ColorValue( 1, 1, 1, alpha );
			}
			if( sdk > SDKInfo.SDKs.Unlimited )
			{
				window.Controls[ "UnlimitedSDK" ].ColorMultiplier = new ColorValue( 1, 1, 1, alpha );
				window.Controls[ "UnlimitedSDKBackground" ].ColorMultiplier = new ColorValue( 1, 1, 1, alpha );
			}

			BackColor = new ColorValue( 0, 0, 0, .5f );
			MouseCover = true;
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
