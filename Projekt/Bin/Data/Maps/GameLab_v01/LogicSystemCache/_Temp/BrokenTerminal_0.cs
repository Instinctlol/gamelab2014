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
	public class BrokenTerminal_0 : Engine.EntitySystem.LogicSystem.LogicEntityObject
	{
		ProjectEntities.Repairable __ownerEntity;
		
		public BrokenTerminal_0( ProjectEntities.Repairable ownerEntity )
			: base( ownerEntity )
		{
			this.__ownerEntity = ownerEntity;
			ownerEntity.Repair += delegate( ProjectEntities.Repairable __entity ) { if( Engine.EntitySystem.LogicSystemManager.Instance != null )Repair(  ); };
		}
		
		public ProjectEntities.Repairable Owner
		{
			get { return __ownerEntity; }
		}
		
		
		public void Repair()
		{
			Owner.Die();
			((Repairable)Entitiese.Instance.GetByName("BrokenTerminal_0")).Visibel = true;
			
		}

	}
}
