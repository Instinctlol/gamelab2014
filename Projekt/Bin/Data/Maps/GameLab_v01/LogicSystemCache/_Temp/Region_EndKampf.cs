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
	public class Region_EndKampf : Engine.EntitySystem.LogicSystem.LogicEntityObject
	{
		Engine.MapSystem.Region __ownerEntity;
		
		public Region_EndKampf( Engine.MapSystem.Region ownerEntity )
			: base( ownerEntity )
		{
			this.__ownerEntity = ownerEntity;
			ownerEntity.ObjectIn += delegate( Engine.EntitySystem.Entity __entity, Engine.MapSystem.MapObject obj ) { if( Engine.EntitySystem.LogicSystemManager.Instance != null )ObjectIn( obj ); };
		}
		
		public Engine.MapSystem.Region Owner
		{
			get { return __ownerEntity; }
		}
		
		public bool firstIn;
		
		public void ObjectIn( Engine.MapSystem.MapObject obj )
		{
			if(!firstIn && obj is PlayerCharacter)
			{
				GameMap.Instance.GameMusic = "Sounds\\Music\\ava_bossfight_looping.ogg";
				firstIn = true;
			}
		}

	}
}
