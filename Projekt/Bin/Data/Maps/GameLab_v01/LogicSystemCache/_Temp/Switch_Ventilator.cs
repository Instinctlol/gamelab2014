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
	public class Switch_Ventilator : Engine.EntitySystem.LogicSystem.LogicEntityObject
	{
		ProjectEntities.BooleanSwitch __ownerEntity;
		
		public Switch_Ventilator( ProjectEntities.BooleanSwitch ownerEntity )
			: base( ownerEntity )
		{
			this.__ownerEntity = ownerEntity;
			ownerEntity.ValueChange += delegate( ProjectEntities.Switch __entity ) { if( Engine.EntitySystem.LogicSystemManager.Instance != null )ValueChange(  ); };
		}
		
		public ProjectEntities.BooleanSwitch Owner
		{
			get { return __ownerEntity; }
		}
		
		
		public void ValueChange()
		{
			if(Owner.Value)
				((Fan)Entities.Instance.GetByName("Fan_0")).Throttle = 1;
			else
				((Fan)Entities.Instance.GetByName("Fan_0")).Throttle = 0;
		}

	}
}
