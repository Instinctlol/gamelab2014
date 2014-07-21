using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Engine;
using Engine.MathEx;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.PhysicsSystem;
using Engine.FileSystem;
using System.Collections;
using ProjectCommon; 


namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="AlienAI"/> entity type.
    /// </summary>
    public class AlienAIType : AlienUnitAIType
    {
   
    }

    /// <summary>
    /// AI for small aliens
    /// </summary>
    public class AlienAI : AlienUnitAI
    {
        AlienAIType _type = null; public new AlienAIType Type { get { return _type; } }

        float inactiveFindTaskTimer;

        //optimization
        List<Weapon> initialWeapons;

              

        /// <summary>
        /// Konstruktor
        /// </summary>
        public AlienAI()
        {
            inactiveFindTaskTimer = World.Instance.Random.NextFloat() * 2;
        }

        /// <summary>
        /// Getter for Alien Object
        /// </summary>
        [Browsable(false)]
        public new Alien ControlledObject
        {
            get { return (Alien)base.ControlledObject; }
        }

        public override List<UserControlPanelTask> GetControlPanelTasks()
        {
            List<UserControlPanelTask> list = new List<UserControlPanelTask>();

            list.Add(new UserControlPanelTask(new Task(Task.Types.Stop), CurrentTask.Type == Task.Types.Stop));

            list.Add(new UserControlPanelTask(new Task(Task.Types.Move),
                CurrentTask.Type == Task.Types.Move || CurrentTask.Type == Task.Types.BreakableMove));

            list.Add(new UserControlPanelTask(new Task(Task.Types.Attack),
                CurrentTask.Type == Task.Types.Attack || CurrentTask.Type == Task.Types.BreakableAttack));

            list.Add(new UserControlPanelTask(new Task(Task.Types.Patrol),
                CurrentTask.Type == Task.Types.Patrol));

           

            return list;
        }

        protected float GetAttackObjectPriority(Unit obj)
        {
            // if the object to attack is me, do not attack
            if (ControlledObject == obj)
                return 0;

            // if the object to attack has no intellect, do not attack
            if (obj.Intellect == null)
                return 0;

            // if the object to attack has a different faction than me
            if (Faction != null && obj.Intellect.Faction != null && Faction != obj.Intellect.Faction)
            {
                // calculate a value for priority to attack this object
                Vec3 distance = obj.Position - ControlledObject.Position;
                float len = distance.Length();
                // if distance is very small the priority is very high
                return 1.0f / len + 1.0f;
            }

            //0 means no priority to attacks
            return 0;
        }





        //Alien überprüft selbstständig, ob Gegner in der Nähe sind und greift dann an
        bool InactiveFindTask()
        {
                //wenn das Alien keine Waffe hat -> tue nichts
                if (initialWeapons.Count == 0)
                    return false;

                Alien controlledObj = ControlledObject;
                if (controlledObj == null)
                    return false;

                Dynamic newTaskAttack = null;
                float attackObjectPriority = 0;

                Vec3 controlledObjPos = controlledObj.Position;
                float radius = controlledObj./*Type.*/ViewRadius;

                //suche Objekte in einem bestimmten Radius
                Map.Instance.GetObjects(new Sphere(controlledObjPos, radius),
                    MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                    {
                        Unit obj = (Unit)mapObject;

                        Vec3 objPos = obj.Position;

                        //überprüfe Abstand des Objektes
                        Vec3 diff = objPos - controlledObjPos;
                        float objDistance = diff.Length();
                        if (objDistance > radius)
                            return;

                        //falls "attack-Priorität" größer -> neues Objekt für attack gefunden
                        float priority = GetAttackObjectPriority(obj);
                        if (priority != 0 && priority > attackObjectPriority)
                        {
                            attackObjectPriority = priority;
                            newTaskAttack = obj;
                        }
                    });

                //falls Objekt für attack vorhanden, greife automatisch an!
                if (newTaskAttack != null)
                {
                    DoTask(new Task(Task.Types.BreakableAttack, newTaskAttack), false);
                    return true;
                }
                
                 

                
                return false;
        }
    

 /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        
        //überprüfe, ob Alien eine Waffe besitzt
        void UpdateInitialWeapons()
        {
            initialWeapons = new List<Weapon>();

            foreach (MapObjectAttachedObject attachedObject in ControlledObject.AttachedObjects)
            {
                MapObjectAttachedMapObject attachedMapObject = attachedObject as MapObjectAttachedMapObject;
                if (attachedMapObject != null)
                {
                    Weapon weapon = attachedMapObject.MapObject as Weapon;
                    if (weapon != null)
                    {
                        initialWeapons.Add(weapon);
                    }
                }
            }
        }

        protected override void TickTasks()
        {
            if (initialWeapons == null)
                UpdateInitialWeapons();

            base.TickTasks();

            Alien controlledObj = ControlledObject;
            if (controlledObj == null)
                return;

            switch (CurrentTask.Type)
            {
                //Move
                case Task.Types.Move:
                case Task.Types.BreakableMove:
                    if (CurrentTask.Entity != null)
                    {
                        controlledObj.Move(CurrentTask.Entity.Position);
                    }
                    else
                    {
                        Vec3 pos = CurrentTask.Position;

                        if ((controlledObj.Position.ToVec2() - pos.ToVec2()).Length() < 1.5f &&
                            Math.Abs(controlledObj.Position.Z - pos.Z) < 3.0f)
                        {
                            //get to
                            DoNextTask();
                        }
                        else
                            controlledObj.Move(pos);
                    }
                    break;

                // Patrollieren
                case Task.Types.Patrol:
                if (CurrentTask.Entity != null)
                {
                    controlledObj.Patrol();
                }
                 break;

                //Attack, Repair
                case Task.Types.Attack:
                case Task.Types.BreakableAttack:
                case Task.Types.Repair:
                case Task.Types.BreakableRepair:
                {
                    /*//healed
                    if ((CurrentTask.Type == Task.Types.Repair ||
                        CurrentTask.Type == Task.Types.BreakableRepair)
                        && CurrentTask.Entity != null)
                    {
                        if (CurrentTask.Entity.Health == CurrentTask.Entity.Type.HealthMax)
                        {
                            DoNextTask();
                            break;
                        }
                    }
                    */

                    float needDistance = controlledObj.Type.OptimalAttackDistanceRange.Maximum;

                    Vec3 targetPos;
                    if (CurrentTask.Entity != null)
                        targetPos = CurrentTask.Entity.Position;
                    else
                        targetPos = CurrentTask.Position;

                    float distance = (controlledObj.Position - targetPos).Length();

                    if (distance != 0)
                    {
                        bool lineVisibility = false;
                        {
                            if (distance < needDistance)
                            {
                                lineVisibility = true;

                                //direct line visibility check 

                                Vec3 start = initialWeapons[0].Position;
                                Ray ray = new Ray(start, targetPos - start);

                                RayCastResult[] piercingResult = PhysicsWorld.Instance.RayCastPiercing(
                                    ray, (int)ContactGroup.CastOnlyContact);

                                foreach (RayCastResult result in piercingResult)
                                {
                                    MapObject obj = MapSystemWorld.GetMapObjectByBody(result.Shape.Body);

                                    if (obj != null && obj == CurrentTask.Entity)
                                        break;

                                    if (obj != controlledObj)
                                    {
                                        lineVisibility = false;
                                        break;
                                    }
                                }
                            }
                        }

                        //movement control: falls Gegner sichtbar, stopt das Alien und richtet sich in Richtung seines Gegners aus
                        if (lineVisibility)
                        {
                            //stop
                            controlledObj.Stop();

                            Alien character = controlledObj as Alien;
                            if (character != null)
                                character.SetLookDirection(targetPos);
                        }
                        else
                        {
                            //move to target
                            controlledObj.Move(targetPos);
                        }

                        //weapons control
                        if (lineVisibility)
                        {
                            foreach (Weapon weapon in initialWeapons)
                            {
                                Vec3 pos = targetPos;
                                Gun gun = weapon as Gun;
                                if (gun != null && base.CurrentTask.Entity != null)
                                    gun.GetAdvanceAttackTargetPosition(false, base.CurrentTask.Entity, false, out pos);
                                weapon.SetForceFireRotationLookTo(pos);

                                if (weapon.Ready)
                                {
                                    // Attackieren
                                    // Sound abspielen
                                    controlledObj.PlaySound("attack");
                                    Range range;

                                    range = weapon.Type.WeaponNormalMode.UseDistanceRange;
                                    if (distance >= range.Minimum && distance <= range.Maximum)
                                        weapon.TryFire(false);

                                    range = weapon.Type.WeaponAlternativeMode.UseDistanceRange;
                                    if (distance >= range.Minimum && distance <= range.Maximum)
                                        weapon.TryFire(true);
                                }
                            }
                        }
                    }

                }
                break;
            }
            if ((CurrentTask.Type == Task.Types.Patrol ||
                CurrentTask.Type == Task.Types.Stop ||
                CurrentTask.Type == Task.Types.BreakableMove ||
                CurrentTask.Type == Task.Types.BreakableAttack //||
                //CurrentTask.Type == Task.Types.BreakableRepair
                ) && Tasks.Count == 0)
            {
                inactiveFindTaskTimer -= TickDelta;
                if (inactiveFindTaskTimer <= 0)
                {
                    inactiveFindTaskTimer += 1.0f;
                    if (!InactiveFindTask())
                        inactiveFindTaskTimer += .5f;
                }
            }
        }

        

        //begin ClearTaskQueue
        //void ClearTaskQueue()
        //{
        //    foreach (Mission mission in missions)
        //    {
        //        if (mission.TaskEntity != null)
        //            UnsubscribeToDeletionEvent(mission.TaskEntity);
        //    }
        //    missions.Clear();
        //}
        //end ClearTaskQueue
        
        //begin DoMissionInternal
        //void DoMissionInternal(Mission mission)
        //{
        //    if (currentMission.TaskEntity != null)
        //        UnsubscribeToDeletionEvent(currentMission.TaskEntity);

        //    currentMission = mission;

        //    if (currentMission.TaskEntity != null)
        //        SubscribeToDeletionEvent(currentMission.TaskEntity);
        //    currentMission._Begin();
        //}
        //end DoMissionInternal

        //begin doMission
        //public void DoMission(Mission mission, bool toQueue)
        //{
        //    if (ControlledObject == null)
        //        return;

        //    //if (toQueue && mission.Count == 0 && currentTask is IdleTask)
        //    //    toQueue = false;

        //    if (!toQueue)
        //    {
        //        ClearTaskQueue();
        //        DoMissionInternal(mission);
        //    }
        //    else
        //    {
        //        //add task to queue
        //        if (mission.TaskEntity != null)
        //            SubscribeToDeletionEvent(mission.TaskEntity);
        //        missions.Enqueue(mission);
        //    }
        //}
        //end doMission


        //begin Mission
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
        //end Mission

        //begin MoveTask

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
        
        //end MoveTask




        //begin PathController
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

        //end PathController
  
        

      
        





    }
}
