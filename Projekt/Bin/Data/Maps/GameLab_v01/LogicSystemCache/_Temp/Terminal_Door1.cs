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
	public class Terminal_Door1 : Engine.EntitySystem.LogicSystem.LogicEntityObject
	{
		ProjectEntities.Terminal __ownerEntity;
		
		public Terminal_Door1( ProjectEntities.Terminal ownerEntity )
			: base( ownerEntity )
		{
			this.__ownerEntity = ownerEntity;
			ownerEntity.TerminalRotateLeftAction += delegate( ProjectEntities.Terminal __entity ) { if( Engine.EntitySystem.LogicSystemManager.Instance != null )TerminalRotateLeftAction(  ); };
			ownerEntity.TerminalRotateRightAction += delegate( ProjectEntities.Terminal __entity ) { if( Engine.EntitySystem.LogicSystemManager.Instance != null )TerminalRotateRightAction(  ); };
		}
		
		public ProjectEntities.Terminal Owner
		{
			get { return __ownerEntity; }
		}
		
		
		public void TerminalRotateLeftAction()
		{
			((Ring)Entities.Instance.GetByName("F2_Ring")).RotateLeft();
		}

		public void TerminalRotateRightAction()
		{
			((Ring)Entities.Instance.GetByName("F2_Ring")).RotateRight();
		}

	}
}