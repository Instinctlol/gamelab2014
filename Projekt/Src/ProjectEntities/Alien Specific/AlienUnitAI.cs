using Engine.EntitySystem;
using Engine.MathEx;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="AlienUnitAI"/> entity type.
    /// </summary>
    public class AlienUnitAIType : AIType
    {
    }

    /// <summary>
    /// base AI for both, AlienUnitAI and AlienSpawnerAI.
    /// contains the logic which will be used from spawnpoint and small alien.
    /// </summary>
    public abstract class AlienUnitAI : AI
    {
        AlienUnitAIType _type = null; public new AlienUnitAIType Type { get { return _type; } }


		[FieldSerialize]
		Task currentTask = new Task( Task.Types.Stop );

		[FieldSerialize]
		List<Task> tasks = new List<Task>();

        //// Getter ////////////////////////////////
        [Browsable(false)]
        public List<Task> Tasks
        {
            get { return tasks; }
        }

        public Task CurrentTask
        {
            get { return currentTask; }
        }

        [Browsable(false)]
        public new AlienUnit ControlledObject
        {
            get { return (AlienUnit)base.ControlledObject; }
        }

		///////////////////////////////////////////

		public struct Task
		{
			[FieldSerialize]
			[DefaultValue( Types.None )]
			Types type;

			[FieldSerialize]
			[DefaultValue( typeof( Vec3 ), "0 0 0" )]
			Vec3 position;

			[FieldSerialize]
			DynamicType entityType;

			[FieldSerialize]
			Dynamic entity;

            [FieldSerialize]
            [DefaultValue( 1 )]
            int spawnNumber;

			public enum Types
			{
				None,
				Stop,
				BreakableAttack,//for automatic attacks
				Hold,
				Move,
				BreakableMove,//for automatic moves
				Attack,
                Repair,
				BreakableRepair,//for automatic repair
				ProductUnit,
				SelfDestroy,//for cancel build building 
                Active, //for active state of small Alien (walks independently through the world to find enemies and attacks them)
                Passive //for passive state of small Alien (doesn't move as long as there's no enemy in range, but attacks enemies in a certain radius automatically)
			}

			public Task( Types type )
			{
				this.type = type;
				this.position = new Vec3( float.NaN, float.NaN, float.NaN );
				this.entityType = null;
				this.entity = null;
                this.spawnNumber = 1;
			}

			public Task( Types type, Vec3 position )
			{
				this.type = type;
				this.position = position;
				this.entityType = null;
				this.entity = null;
                this.spawnNumber = 1;
			}

			public Task( Types type, DynamicType entityType )
			{
				this.type = type;
				this.position = new Vec3( float.NaN, float.NaN, float.NaN );
				this.entityType = entityType;
                this.entity = null;
                this.spawnNumber = 1;
			}

			public Task( Types type, Vec3 position, DynamicType entityType )
			{
				this.type = type;
				this.position = position;
				this.entityType = entityType;
				this.entity = null;
                this.spawnNumber = 1;
			}

			public Task( Types type, Dynamic entity )
			{
				this.type = type;
				this.position = new Vec3( float.NaN, float.NaN, float.NaN );
				this.entityType = null;
				this.entity = entity;
                this.spawnNumber = 1;
			}

            public Task(Types type, DynamicType entityType, int spawnNumber)
            {
                this.type = type;
                this.position = new Vec3(float.NaN, float.NaN, float.NaN);
                this.entityType = entityType;
                this.entity = null;
                this.spawnNumber = spawnNumber;
            }

			public Types Type
			{
				get { return type; }
			}

			public Vec3 Position
			{
				get { return position; }
			}

			public DynamicType EntityType
			{
				get { return entityType; }
			}

			public Dynamic Entity
			{
				get { return entity; }
			}

            public int SpawnNumber
            {
                get { return spawnNumber;  }
            }

			public override string ToString()
			{
				string s = type.ToString();
				if( !float.IsNaN( position.X ) )
					s += ", Position: " + position.ToString();
				if( entityType != null )
					s += ", EntityType: " + entityType.Name;
				if( entity != null )
					s += ", Entity: " + entity.ToString();
				return s;
			}
		}

		///////////////////////////////////////////

		public struct UserControlPanelTask
		{
			Task task;
			bool active;
			bool enable;

			public UserControlPanelTask( Task task )
			{
				this.task = task;
				this.active = true;
				this.enable = true;
			}

			public UserControlPanelTask( Task task, bool active )
			{
				this.task = task;
				this.active = active;
				this.enable = true;
			}

			public UserControlPanelTask( Task task, bool active, bool enable )
			{
				this.task = task;
				this.active = active;
				this.enable = enable;
			}

			public Task Task
			{
				get { return task; }
			}

			public bool Active
			{
				get { return active; }
			}

			public bool Enable
			{
				get { return enable; }
			}

		}

		///////////////////////////////////////////

		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
		protected override void OnPostCreate( bool loaded )
		{
			base.OnPostCreate( loaded );

            //we want to take items
            AllowTakeItems = true;
           
			SubscribeToTickEvent();
		}

		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDestroy()"/>.</summary>
		protected override void OnDestroy()
		{
            EngineConsole.Instance.Print("ondestroy");
            ClearTaskList();
            base.OnDestroy();
		}

		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnDeleteSubscribedToDeletionEvent(Entity)"/></summary>
		protected override void OnDeleteSubscribedToDeletionEvent( Entity entity )
		{
			base.OnDeleteSubscribedToDeletionEvent( entity );

			for( int n = 0; n < tasks.Count; n++ )
			{
				if( tasks[ n ].Entity == entity )
				{
					tasks.RemoveAt( n );
					n--;
				}
			}

			if( currentTask.Entity == entity )
				DoNextTask();
		}

		/// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
		protected override void OnTick()
		{
			base.OnTick();
            TickTasks();
		}

        protected virtual void TickTasks()      
        {
            AlienUnit controlledObj = ControlledObject;
            if (controlledObj == null)
                return;

            switch (currentTask.Type)
            {
                //Stop
                case Task.Types.Stop:
                    controlledObj.Stop();
                    break;
            }
        }

        /// <summary>
        /// Alle Tasks löschen
        /// </summary>
		void ClearTaskList()
		{
			foreach( Task task in tasks )
				if( task.Entity != null )
					UnsubscribeToDeletionEvent( task.Entity );
			tasks.Clear();
		}

		protected virtual void DoTaskInternal( Task task )
		{
			if( currentTask.Entity != null )
				UnsubscribeToDeletionEvent( currentTask.Entity );

			currentTask = task;

			if( currentTask.Entity != null )
				SubscribeToDeletionEvent( currentTask.Entity );

			//Stop
			if( task.Type == Task.Types.Stop )
			{
				if( ControlledObject != null )
					ControlledObject.Stop();
			}

			//SelfDestroy
			if( task.Type == Task.Types.SelfDestroy )
			{
                EngineConsole.Instance.Print("die");

                ControlledObject.Die();
			}
		}

		public void DoTask( Task task, bool toQueue )
		{
			if( toQueue && currentTask.Type == Task.Types.Stop && tasks.Count == 0 )
				toQueue = false;

			if( !toQueue )
			{
				ClearTaskList();
				DoTaskInternal( task );
			}
			else
			{
				if( task.Entity != null )
					SubscribeToDeletionEvent( task.Entity );
				tasks.Add( task );
			}
		}

		protected void DoNextTask()
		{
			if( currentTask.Entity != null )
				UnsubscribeToDeletionEvent( currentTask.Entity );

			if( tasks.Count != 0 )
			{
				Task task = tasks[ 0 ];
				tasks.RemoveAt( 0 );
				if( task.Entity != null )
					UnsubscribeToDeletionEvent( task.Entity );

				DoTaskInternal( task );
			}
			else
			{
				DoTask( new Task( Task.Types.Stop ), false );
			}
		}

        public abstract List<UserControlPanelTask> GetControlPanelTasks();
    }
}
