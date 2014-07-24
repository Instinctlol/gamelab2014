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
	public class CardReader_0 : Engine.EntitySystem.LogicSystem.LogicEntityObject
	{
		ProjectEntities.Repairable __ownerEntity;
		
		public CardReader_0( ProjectEntities.Repairable ownerEntity )
			: base( ownerEntity )
		{
			this.__ownerEntity = ownerEntity;
		}
		
		public ProjectEntities.Repairable Owner
		{
			get { return __ownerEntity; }
		}
		
		
	}
}