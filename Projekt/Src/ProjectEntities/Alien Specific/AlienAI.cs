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
                //case Task.Types.Repair:
                //case Task.Types.BreakableRepair:
                {
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
                            //if (CurrentTask.Type == Task.Types.Attack)
                            //{
                                //move to target
                                controlledObj.Move(targetPos);
                            //}
                        }

                        //weapons control
                        if (lineVisibility)
                        {
                            Computer.AddRadarElement(controlledObj.MapBounds.Minimum.ToVec2(), controlledObj.MapBounds.Maximum.ToVec2());
                            
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
                CurrentTask.Type == Task.Types.BreakableAttack                 
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
