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
	public class BooleanSwitch2_InnerRing : Engine.EntitySystem.LogicSystem.LogicEntityObject
	{
		ProjectEntities.BooleanSwitch __ownerEntity;
		
		public BooleanSwitch2_InnerRing( ProjectEntities.BooleanSwitch ownerEntity )
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
			if(((BooleanSwitch)Entities.Instance.GetByName("BooleanSwitch1_InnerRing")).Value && ((BooleanSwitch)Entities.Instance.GetByName("BooleanSwitch2_InnerRing")).Value && 
			((BooleanSwitch)Entities.Instance.GetByName("BooleanSwitch3_InnerRing")).Value )
			{
				((Ring)Entities.Instance.GetByName("F3_Ring")).Rotatable=true;
			}
			else
			{
				((Ring)Entities.Instance.GetByName("F3_Ring")).Rotatable=false;
			}
		}

	}
}
