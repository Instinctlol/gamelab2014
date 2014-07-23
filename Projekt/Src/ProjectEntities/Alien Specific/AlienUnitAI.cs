using Engine.EntitySystem;
using Engine.MathEx;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Engine.FileSystem;
using System.Collections;
using Engine.MapSystem;


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
        /*************/
        /* Attribute */
        /*************/
        AlienUnitAIType _type = null; public new AlienUnitAIType Type { get { return _type; } }

		[FieldSerialize]
		public Task currentTask = new Task( Task.Types.Stop );

		[FieldSerialize]
		List<Task> tasks = new List<Task>();


        ////begin patrol
        //ArrayList route; //new variable for the route
        //int routeIndex = 0; //index for route points
        //PathController pathController = new PathController();
        ////end patrol    



        /*******************/
        /* Getter / Setter */
        /*******************/
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



        /**************/
        /* Funktionen */
        /**************/
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

        /// <summary>
        /// Wird bei jedem Tick-Event ausgeführt. Wird in Alien und AlienSpawner Klassen erweitert. Deshalb nur Stop hier, weil gemeinsamer Task
        /// </summary>
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

         ///<summary>
         ///Task ausführen
         ///</summary>
         ///<param name="task"></param>
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
                ControlledObject.Die();
            }

            //Patrol
            if (task.Type == Task.Types.Patrol)
            {
                if (ControlledObject != null)
                    ((Alien)ControlledObject).Patrol();
            }


        }

        /// <summary>
        /// Tasks in Queue verwalten
        /// </summary>
        /// <param name="task"></param>
        /// <param name="toQueue"></param>
		public void DoTask( Task task, bool toQueue )
		{
			if( toQueue && currentTask.Type == Task.Types.Stop && tasks.Count == 0 )
				toQueue = false;

            if (this.ControlledObject.patrolEnabled && task.Type == Task.Types.Move)
            {
                DoTaskInternal(new AlienUnitAI.Task(Task.Types.Stop));
            }

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

        /// <summary>
        /// Nächste Task ausführen oder Stop
        /// </summary>
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

        
        
        /***************/
        /* Struct Task */
        /***************/
        /// <summary>
        /// Taskverwaltung für die Aufgabenzuteilung an Alien und AlienSpawner
        /// </summary>
        public struct Task
        {
            [FieldSerialize]
            [DefaultValue(Types.None)]
            Types type;

            [FieldSerialize]
            [DefaultValue(typeof(Vec3), "0 0 0")]
            Vec3 position;

            [FieldSerialize]
            DynamicType entityType;

            [FieldSerialize]
            Dynamic entity;

            [FieldSerialize]
            [DefaultValue(1)]
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
                Patrol
                
            }

            public Task(Types type)
            {
                this.type = type;
                this.position = new Vec3(float.NaN, float.NaN, float.NaN);
                this.entityType = null;
                this.entity = null;
                this.spawnNumber = 1;
            }

            public Task(Types type, Vec3 position)
            {
                this.type = type;
                this.position = position;
                this.entityType = null;
                this.entity = null;
                this.spawnNumber = 1;
            }

            public Task(Types type, DynamicType entityType)
            {
                this.type = type;
                this.position = new Vec3(float.NaN, float.NaN, float.NaN);
                this.entityType = entityType;
                this.entity = null;
                this.spawnNumber = 1;
            }

            public Task(Types type, Vec3 position, DynamicType entityType)
            {
                this.type = type;
                this.position = position;
                this.entityType = entityType;
                this.entity = null;
                this.spawnNumber = 1;
            }

            public Task(Types type, Dynamic entity)
            {
                this.type = type;
                this.position = new Vec3(float.NaN, float.NaN, float.NaN);
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
                get { return spawnNumber; }
            }

            public override string ToString()
            {
                string s = type.ToString();
                if (!float.IsNaN(position.X))
                    s += ", Position: " + position.ToString();
                if (entityType != null)
                    s += ", EntityType: " + entityType.Name;
                if (entity != null)
                    s += ", Entity: " + entity.ToString();
                return s;
            }
        }



        /***************/
        /* Struct UserControlPanelTask */
        /***************/
        /// <summary>
        /// Taskverwaltung für die Buttons für Alien und AlienSpawner
        /// </summary>
        public struct UserControlPanelTask
        {
            Task task;
            bool active;
            bool enable;

            public UserControlPanelTask(Task task)
            {
                this.task = task;
                this.active = true;
                this.enable = true;
            }

            public UserControlPanelTask(Task task, bool active)
            {
                this.task = task;
                this.active = active;
                this.enable = true;
            }

            public UserControlPanelTask(Task task, bool active, bool enable)
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





        ////begin patrol
        //public bool Patrol()
        //{

        //    //MapCurve es ausgewählten Aliens:
        //    AlienUnit controlledObj = ControlledObject;

        //    MapCurve mapCurve = controlledObj.MovementRoute as MapCurve; //get the MapCurve from the object this AI controls (Alien on the map)

        //    if (mapCurve != null) //was there one set for this Alien?
        //    {
        //        if (route == null) //initialize patrol route, if not already done
        //        {
        //            route = new ArrayList();

        //            foreach (MapCurvePoint point in mapCurve.Points) //add every MapCurvePoint as a waypoint in our route
        //            {
        //                route.Add(point);
        //            }
        //        }


        //        //create a movement task for the next point
        //        MapCurvePoint pt = route[routeIndex] as MapCurvePoint;

        //        //this.AutomaticTasks = GameCharacterAI.AutomaticTasksEnum.EnabledOnlyWhenNoTasks; //do this only if there are no other tasks
        //        new MoveTask(this, pt.Position, .5f); //create a new move task and use current CurvePoint as destination
        //        routeIndex++; //next route waypoint

        //        //reverse the route if we are at the end
        //        if (routeIndex >= route.Count)
        //        {
        //            routeIndex = 0;
        //            route.Reverse();
        //        }

        //        return true; //we found something to do!
        //    }
        //    else // we are not patrolling. Select a random destination from the curve
        //    {
        //        Random rnd = new Random();
        //        MapCurvePoint pt = route[rnd.Next(route.Count)] as MapCurvePoint;
        //        //this.AutomaticTasks = GameCharacterAI.AutomaticTasksEnum.EnabledOnlyWhenNoTasks;
        //        new MoveTask(this, pt.Position, .5f);
        //        return true;
        //    }

        //}
        ////end Patrol




        ////begin Mission
        //public abstract class Mission
        //{
        //    AlienAI owner;

        //    Vec3 taskPosition;
        //    MapObject taskEntity;

        //    //

        //    protected Mission(AlienAI owner, Vec3 position, MapObject entity)
        //    {
        //        this.owner = owner;
        //        this.taskPosition = position;
        //        this.taskEntity = entity;
        //    }

        //    public AlienAI Owner
        //    {
        //        get { return owner; }
        //    }

        //    public Vec3 TaskPosition
        //    {
        //        get { return taskPosition; }
        //    }

        //    public MapObject TaskEntity
        //    {
        //        get { return taskEntity; }
        //    }

        //    protected virtual bool OnLoad(TextBlock block)
        //    {
        //        if (block.IsAttributeExist("taskPosition"))
        //            taskPosition = Vec3.Parse(block.GetAttribute("taskPosition"));
        //        if (block.IsAttributeExist("taskEntity"))
        //        {
        //            taskEntity = Entities.Instance.GetLoadingEntityBySerializedUIN(
        //                uint.Parse(block.GetAttribute("taskEntity"))) as MapObject;
        //            if (taskEntity == null)
        //                return false;
        //        }
        //        return true;
        //    }
        //    internal bool _Load(TextBlock block) { return OnLoad(block); }

        //    protected virtual void OnSave(TextBlock block)
        //    {
        //        if (taskPosition != Vec3.Zero)
        //            block.SetAttribute("taskPosition", taskPosition.ToString());
        //        if (taskEntity != null)
        //            block.SetAttribute("taskEntity", taskEntity.UIN.ToString());
        //    }
        //    internal void _Save(TextBlock block) { OnSave(block); }

        //    protected virtual void OnBegin() { }
        //    internal void _Begin() { OnBegin(); }

        //    protected virtual void OnTick() { }
        //    internal void _Tick() { OnTick(); }

        //    protected abstract bool OnIsFinished();
        //    public bool IsFinished() { return OnIsFinished(); }

        //    public virtual Vec3 GetTargetPosition()
        //    {
        //        if (TaskEntity != null)
        //            return TaskEntity.Position;
        //        else
        //            return TaskPosition;
        //    }
        //}
        ////end Mission

        ////begin MoveTask
        //public class MoveTask : Mission
        //{
        //    float reachDistance;

        //    //

        //    public MoveTask(AlienAI owner, Vec3 position, float reachDistance)
        //        : base(owner, position, null)
        //    {
        //        this.reachDistance = reachDistance;
        //    }

        //    public MoveTask(AlienAI owner, MapObject entity, float reachDistance)
        //        : base(owner, Vec3.Zero, entity)
        //    {
        //        this.reachDistance = reachDistance;
        //    }

        //    public float ReachDistance
        //    {
        //        get { return reachDistance; }
        //    }

        //    public override string ToString()
        //    {
        //        return "Move: " + (TaskEntity != null ? TaskEntity.ToString() : TaskPosition.ToString(1));
        //    }

        //    protected override bool OnLoad(TextBlock block)
        //    {
        //        if (!base.OnLoad(block))
        //            return false;
        //        if (block.IsAttributeExist("reachDistance"))
        //            reachDistance = float.Parse(block.GetAttribute("reachDistance"));
        //        return true;
        //    }

        //    protected override void OnSave(TextBlock block)
        //    {
        //        base.OnSave(block);
        //        block.SetAttribute("reachDistance", reachDistance.ToString());
        //    }

        //    protected override void OnBegin()
        //    {
        //        base.OnBegin();

        //        Owner.pathController.Reset();
        //    }

        //    //bool IsAllowUpdateControlledObject()
        //    //{
        //    //    //bad for system with disabled renderer, because here game logic depends animation.
        //    //    AnimationTree tree = Owner.ControlledObject.GetFirstAnimationTree();
        //    //    if (tree != null && tree.GetActiveTriggers().Count != 0)
        //    //        return false;
        //    //    return true;
        //    //}

        //    //protected override void OnTick()
        //    //{
        //    //    base.OnTick();

        //    //    Alien controlledObj = Owner.ControlledObject;

        //    //    if (IsAllowUpdateControlledObject() && controlledObj.GetElapsedTimeSinceLastGroundContact() < .3f)//IsOnGround() )
        //    //    {
        //    //        //update path controller
        //    //        Owner.pathController.Update(Entity.TickDelta, controlledObj.Position,
        //    //            GetTargetPosition(), false);

        //    //        //update character
        //    //        Vec3 nextPointPosition;
        //    //        if (Owner.pathController.GetNextPointPosition(out nextPointPosition))
        //    //        {
        //    //            Vec2 vector = nextPointPosition.ToVec2() - controlledObj.Position.ToVec2();
        //    //            if (vector != Vec2.Zero)
        //    //                vector.Normalize();
        //    //            controlledObj.SetTurnToPosition(nextPointPosition);
        //    //            controlledObj.SetForceMoveVector(vector);
        //    //        }
        //    //        else
        //    //            controlledObj.SetForceMoveVector(Vec2.Zero);
        //    //    }
        //    //    else
        //    //        controlledObj.SetForceMoveVector(Vec2.Zero);
        //    //}

        //    protected override bool OnIsFinished()
        //    {
        //        Vec3 targetPosition = GetTargetPosition();
        //        Vec3 objectPosition = Owner.ControlledObject.Position;
        //        if ((GetTargetPosition().ToVec2() - objectPosition.ToVec2()).Length() < reachDistance &&
        //            Math.Abs(objectPosition.Z - targetPosition.Z) < 1.5f)
        //        {
        //            return true;
        //        }
        //        return false;
        //    }
        //}

        ////end MoveTask


        ////begin PathController
        //class PathController
        //{
        //    readonly float reachDestinationPointDistance = .5f;
        //    readonly float reachDestinationPointZDifference = 1.5f;
        //    readonly float maxAllowableDeviationFromPath = .5f;
        //    readonly float updatePathWhenTargetPositionHasChangedMoreThanDistance = 2;
        //    readonly float stepSize = 1;
        //    readonly Vec3 polygonPickExtents = new Vec3(2, 2, 2);
        //    readonly int maxPolygonPath = 512;
        //    readonly int maxSmoothPath = 4096;
        //    readonly int maxSteerPoints = 16;

        //    Vec3 foundPathForTargetPosition = new Vec3(float.NaN, float.NaN, float.NaN);
        //    Vec3[] path;
        //    float pathFindWaitTime;
        //    int currentIndex;

        //    //

        //    RecastNavigationSystem GetNavigationSystem()
        //    {
        //        //use first instance on the map
        //        if (RecastNavigationSystem.Instances.Count != 0)
        //            return RecastNavigationSystem.Instances[0];
        //        return null;
        //    }

        //    public void DropPath()
        //    {
        //        foundPathForTargetPosition = new Vec3(float.NaN, float.NaN, float.NaN);
        //        path = null;
        //        currentIndex = 0;
        //    }

        //    public void Reset()
        //    {
        //        DropPath();
        //    }

        //    public void Update(float delta, Vec3 unitPosition, Vec3 targetPosition, bool dropPath)
        //    {
        //        if (dropPath)
        //            DropPath();

        //        //wait before last path find
        //        if (pathFindWaitTime > 0)
        //        {
        //            pathFindWaitTime -= delta;
        //            if (pathFindWaitTime < 0)
        //                pathFindWaitTime = 0;
        //        }

        //        //already on target position?
        //        if ((unitPosition.ToVec2() - targetPosition.ToVec2()).LengthSqr() <
        //            reachDestinationPointDistance * reachDestinationPointDistance &&
        //            Math.Abs(unitPosition.Z - targetPosition.Z) < reachDestinationPointZDifference)
        //        {
        //            DropPath();
        //            return;
        //        }

        //        //drop path when target position was updated
        //        if (path != null && (foundPathForTargetPosition - targetPosition).Length() >
        //            updatePathWhenTargetPositionHasChangedMoreThanDistance)
        //        {
        //            DropPath();
        //        }

        //        //drop path when unit goaway from path
        //        if (path != null && currentIndex > 0)
        //        {
        //            Vec3 previous = path[currentIndex - 1];
        //            Vec3 next = path[currentIndex];

        //            float min = Math.Min(previous.Z, next.Z);
        //            float max = Math.Max(previous.Z, next.Z);

        //            Vec2 projectedPoint = MathUtils.ProjectPointToLine(
        //                previous.ToVec2(), next.ToVec2(), unitPosition.ToVec2());
        //            float distance2D = (unitPosition.ToVec2() - projectedPoint).Length();

        //            if (distance2D > maxAllowableDeviationFromPath ||
        //                unitPosition.Z + reachDestinationPointZDifference < min ||
        //                unitPosition.Z - reachDestinationPointZDifference > max)
        //            {
        //                DropPath();
        //            }
        //        }

        //        //check if need update path
        //        if (path == null && pathFindWaitTime == 0)
        //        {
        //            bool found;

        //            RecastNavigationSystem system = GetNavigationSystem();
        //            if (system != null)
        //            {
        //                found = system.FindPath(unitPosition, targetPosition, stepSize, polygonPickExtents,
        //                    maxPolygonPath, maxSmoothPath, maxSteerPoints, out path);
        //            }
        //            else
        //            {
        //                found = true;
        //                path = new Vec3[] { targetPosition };
        //            }

        //            currentIndex = 0;

        //            if (found)
        //            {
        //                foundPathForTargetPosition = targetPosition;
        //                //can't find new path during specified time.
        //                pathFindWaitTime = .3f;
        //            }
        //            else
        //            {
        //                foundPathForTargetPosition = new Vec3(float.NaN, float.NaN, float.NaN);
        //                //can't find new path during specified time.
        //                pathFindWaitTime = 1.0f;
        //            }
        //        }

        //        //progress
        //        if (path != null)
        //        {
        //            Vec3 point;
        //            while (true)
        //            {
        //                point = path[currentIndex];

        //                if ((unitPosition.ToVec2() - point.ToVec2()).LengthSqr() <
        //                    reachDestinationPointDistance * reachDestinationPointDistance &&
        //                    Math.Abs(unitPosition.Z - point.Z) < reachDestinationPointZDifference)
        //                {
        //                    //reach point
        //                    currentIndex++;
        //                    if (currentIndex == path.Length)
        //                    {
        //                        //path is ended
        //                        DropPath();
        //                        break;
        //                    }
        //                }
        //                else
        //                    break;
        //            }
        //        }
        //    }

        //    public bool GetNextPointPosition(out Vec3 position)
        //    {
        //        if (path != null)
        //        {
        //            position = path[currentIndex];
        //            return true;
        //        }
        //        position = Vec3.Zero;
        //        return false;
        //    }

        //    //public void DebugDrawPath(Camera camera)
        //    //{
        //    //    if (path != null)
        //    //    {
        //    //        Vec3 offset = new Vec3(0, 0, .2f);

        //    //        camera.DebugGeometry.Color = new ColorValue(0, 1, 0);
        //    //        for (int n = 1; n < path.Length; n++)
        //    //        {
        //    //            Vec3 from = path[n - 1] + offset;
        //    //            Vec3 to = path[n] + offset;
        //    //            AddThicknessLine(camera, from, to, .07f);
        //    //            camera.DebugGeometry.AddLine(from, to);
        //    //        }

        //    //        camera.DebugGeometry.Color = new ColorValue(1, 1, 0);
        //    //        foreach (Vec3 point in path)
        //    //            AddSphere(camera, new Sphere(point + offset, .15f));
        //    //    }
        //    //}
        //}

        ////end PathController





    }
}
