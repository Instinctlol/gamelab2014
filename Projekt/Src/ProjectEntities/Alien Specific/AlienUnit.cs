using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using System.Collections;
using Engine.FileSystem;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="AlienUnit"/> entity type.
    /// </summary>
    public class AlienUnitType : UnitType
    {
        /*************/
        /* Attribute */
        /*************/
        [FieldSerialize]
        [DefaultValue(typeof(Range), "0 0")]
        Range optimalAttackDistanceRange;

        [FieldSerialize]
        [DefaultValue(0.0f)]
        float buildCost;

        [FieldSerialize]
        [DefaultValue(10.0f)]
        float buildTime = 10;



        /*******************/
        /* Getter / Setter */
        /*******************/
        [DefaultValue(typeof(Range), "0 0")]
        public Range OptimalAttackDistanceRange
        {
            get { return optimalAttackDistanceRange; }
            set { optimalAttackDistanceRange = value; }
        }

        [DefaultValue(0.0f)]
        public float BuildCost
        {
            get { return buildCost; }
            set { buildCost = value; }
        }

        [DefaultValue(10.0f)]
        public float BuildTime
        {
            get { return buildTime; }
            set { buildTime = value; }
        }
    }

    public class AlienUnit : Unit
    {
        /*************/
        /* Attribute */
        /*************/
        [FieldSerialize]
        [DefaultValue(false)]
        bool moveEnabled;

        [FieldSerialize]
        [DefaultValue(typeof(Vec3), "0 0 0")]
        Vec3 movePosition;

        AlienUnitType _type = null; public new AlienUnitType Type { get { return _type; } }



        //begin patrol
        protected ArrayList route; //new variable for the route
        protected int routeIndex = 0; //index for route points
        //PathController pathController = new PathController();
        //end patrol


        

        /*******************/
        /* Getter / Setter */
        /*******************/
        [Browsable(false)]
        protected bool MoveEnabled
        {
            get { return moveEnabled; }
        }

        [Browsable(false)]
        protected Vec3 MovePosition
        {
            get { return movePosition; }
        }

        public bool patrolEnabled = false;

       


        /**************/
        /* Funktionen */
        /**************/
        public void Stop()
        {
            moveEnabled = false;
            movePosition = Vec3.Zero;
            patrolEnabled = false;
        }

        public void Move(Vec3 pos)
        {
            moveEnabled = true;
            movePosition = pos;
        }

       
        

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
        }

        public override MapObjectCreateObjectCollection.CreateObjectsResultItem[] DieObjects_Create()
        {
            MapObjectCreateObjectCollection.CreateObjectsResultItem[] result = base.DieObjects_Create();

            //paint created corpses of units to faction color
            foreach (MapObjectCreateObjectCollection.CreateObjectsResultItem item in result)
            {
                MapObjectCreateMapObject createMapObject = item.Source as MapObjectCreateMapObject;
                if (createMapObject != null)
                {
                    foreach (MapObject mapObject in item.CreatedObjects)
                    {
                        //Corpse copy forceMaterial to meshes
                        if (mapObject is Corpse && InitialFaction != null)
                        {
                            (mapObject.AttachedObjects[0] as MapObjectAttachedMesh).MeshObject.
                                    SubObjects[0].MaterialName = "Alien";
                        }
                    }
                }
            }

            return result;
        }

        public override FactionType InitialFaction
        {
            get { return base.InitialFaction; }
            set
            {
                base.InitialFaction = value;
            }
        }



        //begin patrol
        //public bool Patrol()
        //{

        //    //MapCurve des ausgewählten Aliens:
            
        //    MapCurve mapCurve = AlienUnit.MovementRoute as MapCurve; //get the MapCurve from the object this AI controls (Alien on the map)

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
        //end Patrol




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
