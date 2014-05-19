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
	public class BooleanSwitch_0 : Engine.EntitySystem.LogicSystem.LogicEntityObject
	{
		ProjectEntities.BooleanSwitch __ownerEntity;
		
		public BooleanSwitch_0( ProjectEntities.BooleanSwitch ownerEntity )
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
			Engine.EntitySystem.LogicClass __class = Engine.EntitySystem.LogicSystemManager.Instance.MapClassManager.GetByName( "BooleanSwitch_0" );
			Engine.EntitySystem.LogicSystem.LogicDesignerMethod __method = (Engine.EntitySystem.LogicSystem.LogicDesignerMethod)__class.GetMethodByName( "ValueChange" );
			__method.Execute( this, new object[ 0 ]{  } );
		}

	}
}
