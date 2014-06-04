using System;
using System.Collections.Generic;
using System.Text;
using Engine;
using Engine.FileSystem;
using Engine.MathEx;
using Engine.Utils;
using Engine.Renderer;
using Engine.PhysicsSystem;
using Engine.SoundSystem;
using Engine.UISystem;
using Engine.EntitySystem;
using Engine.MapSystem;
using ProjectCommon;
using ProjectEntities;

namespace Maps_GameLab_v___LogicSystem_LogicSystemScripts
{
	public class Terminal_0 : Engine.EntitySystem.LogicSystem.LogicEntityObject
	{
		ProjectEntities.Terminal __ownerEntity;
		
		public Terminal_0( ProjectEntities.Terminal ownerEntity )
			: base( ownerEntity )
		{
			this.__ownerEntity = ownerEntity;
			ownerEntity.TerminalAction += delegate( ProjectEntities.Terminal __entity ) { if( Engine.EntitySystem.LogicSystemManager.Instance != null )TerminalAction(  ); };
		}
		
		public ProjectEntities.Terminal Owner
		{
			get { return __ownerEntity; }
		}
		
		
		public void TerminalAction()
		{
			Engine.EntitySystem.LogicClass __class = Engine.EntitySystem.LogicSystemManager.Instance.MapClassManager.GetByName( "Terminal_0" );
			Engine.EntitySystem.LogicSystem.LogicDesignerMethod __method = (Engine.EntitySystem.LogicSystem.LogicDesignerMethod)__class.GetMethodByName( "TerminalAction" );
			__method.Execute( this, new object[ 0 ]{  } );
		}

	}
}
