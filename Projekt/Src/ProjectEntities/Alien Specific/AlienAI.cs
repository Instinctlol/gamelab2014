using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Engine;
using Engine.MathEx;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.PhysicsSystem;

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
    /// 
    /// bleibt erstmal leer, muss nachher code von AlienUnitAI rüberkopiert werden, der nicht von der AlienSpawnerAI verwendet wird
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

        //TODO
        bool InactiveFindTask()
        {
            if (initialWeapons.Count == 0)
                return false;

            //AlienUnit controlledObj = controlledObject;
            if (ControlledObject == null)
                return false;

            Dynamic newTaskAttack = null;
            float attackObjectPriority = 0;

            Vec3 controlledObjPos = ControlledObject.Position;
            float radius = ControlledObject./*Type.*/ViewRadius;

            Map.Instance.GetObjects(new Sphere(controlledObjPos, radius),
                MapObjectSceneGraphGroups.UnitGroupMask, delegate(MapObject mapObject)
                {
                    Unit obj = (Unit)mapObject;

                    Vec3 objPos = obj.Position;

                    //check distance
                    Vec3 diff = objPos - controlledObjPos;
                    float objDistance = diff.Length();
                    if (objDistance > radius)
                        return;

                    float priority = GetAttackObjectPriority(obj);
                    if (priority != 0 && priority > attackObjectPriority)
                    {
                        attackObjectPriority = priority;
                        newTaskAttack = obj;
                    }
                });

            if (newTaskAttack != null)
            {
                //RTSConstructor specific
                if (ControlledObject.Type.Name == "RTSConstructor")
                    DoTask(new Task(Task.Types.BreakableRepair, newTaskAttack), false);
                else
                    DoTask(new Task(Task.Types.BreakableAttack, newTaskAttack), false);

                return true;
            }

            return false;
        }

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

                //Attack, Repair
                case Task.Types.Attack:
                case Task.Types.BreakableAttack:
                case Task.Types.Repair:
                case Task.Types.BreakableRepair:
                {
                    //healed
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

                        //movement control 
                        if (lineVisibility)
                        {
                            //stop
                            ControlledObject.Stop();

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
            if ((CurrentTask.Type == Task.Types.Stop ||
                CurrentTask.Type == Task.Types.BreakableMove ||
                CurrentTask.Type == Task.Types.BreakableAttack ||
                CurrentTask.Type == Task.Types.BreakableRepair
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

    }
}
