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
	public class CutScene_Region : Engine.EntitySystem.LogicSystem.LogicEntityObject
	{
		Engine.MapSystem.Region __ownerEntity;
		
		public CutScene_Region( Engine.MapSystem.Region ownerEntity )
			: base( ownerEntity )
		{
			this.__ownerEntity = ownerEntity;
			ownerEntity.ObjectIn += delegate( Engine.EntitySystem.Entity __entity, Engine.MapSystem.MapObject obj ) { if( Engine.EntitySystem.LogicSystemManager.Instance != null )ObjectIn( obj ); };
		}
		
		public Engine.MapSystem.Region Owner
		{
			get { return __ownerEntity; }
		}
		
		
		public void ObjectIn( Engine.MapSystem.MapObject obj )
		{
			Engine.EntitySystem.LogicClass __class = Engine.EntitySystem.LogicSystemManager.Instance.MapClassManager.GetByName( "CutScene_Region" );
			Engine.EntitySystem.LogicSystem.LogicDesignerMethod __method = (Engine.EntitySystem.LogicSystem.LogicDesignerMethod)__class.GetMethodByName( "ObjectIn" );
			__method.Execute( this, new object[ 1 ]{ obj } );
		}

	}
}
