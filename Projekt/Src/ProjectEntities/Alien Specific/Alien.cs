using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.ComponentModel;
using Engine.EntitySystem;
using Engine;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.MapSystem;
using Engine.Renderer;
using Engine.Utils;
using System.Reflection;
using System.Drawing.Design;
using System.Diagnostics;
using Engine.SoundSystem;
using Engine.FileSystem;
using ProjectCommon;
using System.Collections;


namespace ProjectEntities
{
    /// <summary>
	/// Defines the <see cref="Alien"/> entity type.
	/// </summary>
	public class AlienType : AlienUnitType
	{
        const float heightDefault = 1.8f;

        [FieldSerialize]
        float height = heightDefault;

        const float radiusDefault = .1f;
        [FieldSerialize]
        float radius = radiusDefault;

        const float maxVelocityDefault = 5;
        [FieldSerialize]
        float maxVelocity = maxVelocityDefault;
        
        [DefaultValue(heightDefault)]
        public float Height
        {
            get { return height; }
            set { height = value; }
        }

        [DefaultValue(radiusDefault)]
        public float Radius
        {
            get { return radius; }
            set { radius = value; }
        }

        [DefaultValue(maxVelocityDefault)]
        public float MaxVelocity
        {
            get { return maxVelocity; }
            set { maxVelocity = value; }
        }

    }

    /// <summary>
    /// The Alien character.
    /// </summary>
    public class Alien : AlienUnit, GridBasedNavigationSystem.IOverrideObjectBehavior
    {
        Body mainBody;

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        Vec2 pathFoundedToPosition = new Vec2(float.NaN, float.NaN);

        [FieldSerialize(FieldSerializeSerializationTypes.World)]
        List<Vec2> path = new List<Vec2>();


        [Description("The AlienSpawner of the spawned Alien")]
        [Browsable(false)]
        public AlienSpawner spawner
        {
            get { return spawner; }
            set { spawner = value; }
        }

        
        float pathFindWaitTime;

        
        float patrolTickTime;

        
        int counterPatrolCosts = 0; //Zähler für das Abziehen von ExperiencePoints beim Patrollieren

        Vec3 oldMainBodyPosition;
        Vec3 mainBodyVelocity;
        // Channel zum abspielen des Default-Sounds für die kleinen Aliens
        Sound alienSound;
        VirtualChannel alienChannel;
        
        AlienType _type = null; public new AlienType Type { get { return _type; } }

        float timeTilNextSound = 0;
        String currentSoundName;

        // Waffe stärker machen
        float timeForStrongerWeapon = 300;
        int weaponStrength = 3;


      
        protected ArrayList route; //Variable für die Patrolroute
        protected int routeIndex = 0; //Index für die Route-Points
                 

        [FieldSerialize]
        private MapObject movRoute; //beinhaltet eine MapCurve, die in der Map als Patrolroute definiert wird 

        
        public MapObject MovementRoute //Zugriff auf die MapCurve
        {
            get { return movRoute; }
            set
            {
                if (value is MapCurve) //akzeptiert nur bestimmte MapObjects 
                {
                    movRoute = value;
                }
                else movRoute = null;
            }
        }

       

        public void Patrol()
        {
            //EngineConsole.Instance.Print("Ich patroulliere");
            patrolEnabled = true;
            this.MovementRoute = null;
            MapCurve mapCurve = null;
            IEnumerable<MapCurve> allPossibleCurves = Entities.Instance.EntitiesCollection.OfType<MapCurve>();
            IEnumerable<Sector> allPossibleSectors = Entities.Instance.EntitiesCollection.OfType<Sector>();
            IEnumerable<MapCurvePoint> allPossibleCurvePoints = Entities.Instance.EntitiesCollection.OfType<MapCurvePoint>();
            Vec3 myPosition = this.Position;
            MapCurve minCurve = null;
            float minDistance = 10000f;
            
            //////////////////////////////////////////

            Vec3 source = this.Position;
            source.Z = 100;
            Vec3 direction = new Vec3(0, 0, -1000000000000000000);
            Ray ray = new Ray(source, direction);
            Map.Instance.GetObjects(ray, delegate(MapObject mObj, float scale)
            {
                Sector sec = mObj as Sector;
                //EngineConsole.Instance.Print("Sektor:" + sec.Name);
                //EngineConsole.Instance.Print("Strahl");

                if (sec != null)
                {
                    
                    foreach (MapCurve curve in allPossibleCurves)
                    {
                        if (sec.Name == "F1R1-S" && curve.Name == "CurveF1R2")
                        {
                            //EngineConsole.Instance.Print("Ich bin im Sektor F1R1");
                            minCurve = curve;
                        }
                        if (sec.Name == "F1S45" && curve.Name == "CurveF1R4")
                        {
                            minCurve = curve;
                            //EngineConsole.Instance.Print("Ich bin im Sektor F1S45");
                        }

                        if (sec.Name == "F2S23" && curve.Name == "CurveF2R2")
                        {
                            minCurve = curve;
                            //EngineConsole.Instance.Print("Ich bin im Sektor F1S23");
                        }
                        //Probleme mit Raum F1R5 -> aktuell wird gemeldet, dass hier Patrouillieren nicht möglich ist!!!!!!!!!!!!!!!


                        if (curve.Name.Substring(5, 4) == sec.Name.Substring(0, 4) && sec.Name != "F1R1-S")
                        {
                            minCurve = curve;
                        }
                        else if (curve.Name.Substring(6, 1) == sec.Name.Substring(1, 1) && (sec.Name.Substring(3, 1) == curve.Name.Substring(8, 1) || sec.Name.Substring(4, 1) == curve.Name.Substring(8, 1)) && sec.Name != "F1S45" && sec.Name != "F2S23")
                        {
                            //EngineConsole.Instance.Print("Ich stehe im Gang");

                            foreach (MapCurvePoint curvePoint in allPossibleCurvePoints)
                            {
                                if (curvePoint.Owner == curve)
                                {
                                    Vec3 distance = curvePoint.Position - this.Position;
                                    if (distance.Length() < minDistance)
                                    {
                                        minDistance = distance.Length();
                                        minCurve = curve;
                                        //EngineConsole.Instance.Print("Minimale Kurve " + minCurve.Name);
                                    }
                                }
                            }
                            //minCurve = curve;
                        }
                        else
                        {
                            //EngineConsole.Instance.Print("Kurvenname != Sektorname ");
                        }
                    }
                }
                else
                {
                    //EngineConsole.Instance.Print("Keinen Sektor gefunden");
                }

                return false;
            });


            //Vec3 source = this.Position;
            //source.Z = 100;
            //Vec3 direction = new Vec3(0, 0, -1000);
            //Ray ray = new Ray(source, direction);
            //Map.Instance.GetObjects(ray, delegate(MapObject mObj, float scale)
            //{
            //    Sector sec = mObj as Sector;

            //    if (sec != null)
            //    {
            //        foreach (MapCurve curve in allPossibleCurves)
            //        {
            //            // suche die am nächsten liegende MapCurve
            //            Vec3 distance = sec.Position - curve.Position;
            //            if (distance.Length() < minDistance)
            //            {
            //                minDistance = distance.Length();
            //                minCurve = curve;
            //                EngineConsole.Instance.Print("MinCurveName: " + minCurve.Name);
            //            }
            //        }
            //    }

            //    return false;
            //});
                    
            ////////////////////////////////////////////////////////


          

            if (minCurve == null)
            {
                //EngineConsole.Instance.Print("Patroullieren ist hier nicht möglich!");
                StatusMessageHandler.sendMessage("Patrouillieren ist hier nicht möglich!");
                patrolEnabled = false;
                Stop();
            }
            
            this.MovementRoute = minCurve;


            mapCurve = this.MovementRoute as MapCurve; //nehme die MapCurve des ausgewählten Aliens in der Map

            if (mapCurve != null) //hat das Alien eine MapCurve?
            {

                route = new ArrayList();

                foreach (MapCurvePoint point in mapCurve.Points) //füge jeden MapCurvePoint als einen Waypoint in die Route ein 
                {
                    route.Add(point);
                }

            }
        }
       

        private void TickPatrol()
        {
            
            patrolTickTime -= TickDelta;
            if (patrolTickTime <= 0)
            {

                if (Computer.Instance.ExperiencePoints > 0)
                    {
                        //this._type.
                        if (counterPatrolCosts == 5)
                        {
                            Computer.Instance.DecrementExperiencePoints();
                            EngineConsole.Instance.Print("XP: " + Computer.Instance.ExperiencePoints);
                            counterPatrolCosts = 0;
                        }
                        counterPatrolCosts++;

                       
                        //laufe zum nächsten Punkt
                        MapCurvePoint pt = route[routeIndex] as MapCurvePoint;
                        Move(pt.Position);
                        routeIndex++; //nächster Route-Waypoint
                                

                        //das Alien läuft die Route zurück, wenn es am Ende der Route angekommen ist.
                        if (routeIndex >= route.Count)
                        {
                            routeIndex = 0;
                            route.Reverse();
                        }

                            
                        patrolTickTime = 0.63f;
                        



                    }
                    else
                    {
                        Stop();
                    }
                

            }
         }
                      
        enum NetworkMessages
        {
            MainBodyVelocityToClient
        }

        /// <summary>
        /// Wenn ein Alien stirbt, dann muss das dem Computer mitgeteilt werden
        /// </summary>
        /// <param name="prejudicial"></param>
        protected override void OnDie(MapObject prejudicial)
        {
            PlaySound("die");
            Computer.Instance.DecrementUsedAliens();
            base.OnDie(prejudicial);
        }

        protected override void OnDamage(MapObject prejudicial, Vec3 pos, Shape shape, float damage, bool allowMoveDamageToParent)
        {
            // Aliens sollen sich nicht mehr gegenseitig töten können. Aliens benutzen ShotgunBullet2
            if (prejudicial.Type.ToString() != "ShotgunBullet2 (Bullet)")
            {
                base.OnDamage(prejudicial, pos, shape, damage, allowMoveDamageToParent);
            }
        }

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            pathFindWaitTime = World.Instance.Random.NextFloat();

            CreatePhysicsModel();

            Body body = PhysicsModel.CreateBody();
            mainBody = body;
            body.Name = "main";
            body.Static = true;
            body.Position = Position;
            body.Rotation = Rotation;

            float length = Type.Height - Type.Radius * 2;
            if (length < 0)
            {
                Log.Error("Alien Length < 0");
                return;
            }
            CapsuleShape shape = body.CreateCapsuleShape();
            shape.Length = length;
            shape.Radius = Type.Radius;
            shape.ContactGroup = (int)ContactGroup.Dynamic;

            SubscribeToTickEvent();

            PhysicsModel.PushToWorld();

            if (mainBody != null)
                oldMainBodyPosition = mainBody.Position;
            
            foreach(MapObjectAttachedObject attachedObject in this.AttachedObjects)
            {
                MapObjectAttachedMapObject mapObject = attachedObject as MapObjectAttachedMapObject;
                if (mapObject != null && mapObject.Alias == "weapon")
                {
                    Gun g = mapObject.MapObject as Gun;
                    if (g != null)
                    {
                        g.PreFire += ChangeBullet;
                    }
                }
            }
        }

        /// <summary>
        /// Vor dem Abfeuern der Bullet, die Stärke aktualisieren, da wir diese alle fünf Min um Eins erhöhen.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="alternative"></param>
        private void ChangeBullet(Weapon entity, bool alternative)
        {
            Gun g = entity as Gun;
            g.NormalMode.typeMode.BulletType.Damage = this.weaponStrength;
        }


        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();

            if (patrolEnabled)
                TickPatrol();

            if (MoveEnabled)
                TickMove();
            else
            {
                Console.WriteLine("sind in OnTick");
                path.Clear();
            }
                

            if (timeTilNextSound > 0)
            {
                timeTilNextSound -= TickDelta;
            }

            CalculateMainBodyVelocity();

            oldMainBodyPosition = mainBody.Position;

            timeForStrongerWeapon -= TickDelta;
            if (timeForStrongerWeapon < 0)
            {
                this.weaponStrength++;
                timeForStrongerWeapon = 300;
            }
        }

        private void CalculateMainBodyVelocity()
        {
            mainBodyVelocity = (mainBody.Position - oldMainBodyPosition) * EntitySystemWorld.Instance.GameFPS;

            if (EntitySystemWorld.Instance.IsServer())
                Server_SendMainBodyVelocityToAllClients();
        }

        private void Server_SendMainBodyVelocityToAllClients()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Alien), (ushort) NetworkMessages.MainBodyVelocityToClient);
            writer.Write(mainBodyVelocity);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.MainBodyVelocityToClient)]
        void Client_ReceiveMainBodyVelocity(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            Vec3 velocity = reader.ReadVec3();
            if (!reader.Complete())
                return;

            mainBodyVelocity = velocity;
        }

        public void SetLookDirection(Vec3 pos)
        {
            Vec2 diff = pos.ToVec2() - Position.ToVec2();

            if (diff == Vec2.Zero)
                return;

            Radian dir = MathFunctions.ATan(diff.Y, diff.X);

            float halfAngle = dir * 0.5f;
            Quat rot = new Quat(new Vec3(0, 0, MathFunctions.Sin(halfAngle)), MathFunctions.Cos(halfAngle));
            Rotation = rot;
        }

        bool DoPathFind()
        {
            Dynamic targetObj = null;
            {
                AlienAI ai = Intellect as AlienAI;
                if (ai != null)
                    targetObj = ai.CurrentTask.Entity;
            }

            //remove this unit from the pathfinding grid
            GetNavigationSystem().RemoveObjectFromMotionMap(this);

            float radius = Type.Radius;
            Rect targetRect = new Rect(
                MovePosition.ToVec2() - new Vec2(radius, radius),
                MovePosition.ToVec2() + new Vec2(radius, radius));

            //remove target unit from the pathfinding grid
            if (targetObj != null && targetObj != this)
                GetNavigationSystem().RemoveObjectFromMotionMap(targetObj);

            //TO DO: really need this call?
            GetNavigationSystem().AddTempClearMotionMap(targetRect);

            const int maxFieldsDistance = 1000;
            const int maxFieldsToCheck = 100000;
            bool found = GetNavigationSystem().FindPath(
                Type.Radius * 2 * 1.1f,
                Position.ToVec2(),
                MovePosition.ToVec2(),
                maxFieldsDistance,
                maxFieldsToCheck,
                true,
                false,
                path);

            GetNavigationSystem().DeleteAllTempClearedMotionMap();

            //add target unit to the pathfinding grid
            if (targetObj != null && targetObj != this)
                GetNavigationSystem().AddObjectToMotionMap(targetObj);

            //add this unit the the pathfinding grid
            GetNavigationSystem().AddObjectToMotionMap(this);

            return found;
        }

        /// <summary>Overridden from <see cref="Engine.MapSystem.MapObject.OnRender(Camera)"/>.</summary>
        protected override void OnRender(Camera camera)
        {
            base.OnRender(camera);

            if (path.Count != 0 && GetNavigationSystem().AlwaysDrawGrid)
                GetNavigationSystem().DebugDrawPath(camera, Position.ToVec2(), path);
        }

        void TickMove()
        {
           
            //path find control
            {
                if (pathFindWaitTime != 0)
                {
                    pathFindWaitTime -= TickDelta;
                    if (pathFindWaitTime < 0)
                        pathFindWaitTime = 0;
                }

                if (pathFoundedToPosition != MovePosition.ToVec2() && pathFindWaitTime == 0)
                {
                    //if (patrolEnabled)
                    //{
                    //    patrolEnabled = false;
                    //    ((AlienUnitAI)this.Intellect).DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Stop), false);

                    //    StatusMessageHandler.sendMessage("Kein Weg gefunden");
                    //}

                    path.Clear();
                }
                    

                if (path.Count == 0)
                {
                    if (pathFindWaitTime == 0)
                    {
                        if (DoPathFind())
                        {
                            pathFoundedToPosition = MovePosition.ToVec2();
                            pathFindWaitTime = .5f;
                        }
                        else
                        {
                            pathFindWaitTime = 1.0f;
                        }
                    }
                    //else
                    //{
                    //    if (patrolEnabled)
                    //    {
                    //        patrolEnabled = false;
                    //        ((AlienUnitAI)this.Intellect).DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Stop), false);

                    //        StatusMessageHandler.sendMessage("Keinen Weg gefunden");
                    //    }
                    //}
                }
            }

            if (path.Count == 0)
            {
                //if(patrolEnabled)
                //{
                //    patrolEnabled = false;
                //    ((AlienUnitAI)this.Intellect).DoTask(new AlienUnitAI.Task(AlienUnitAI.Task.Types.Stop), false);

                //    StatusMessageHandler.sendMessage("Kein Weg gefunden");
                //}
                return;
            }

            //line movement to path[ 0 ]
            {
                Vec2 destPoint = path[0];

                Vec2 diff = destPoint - Position.ToVec2();

                if (diff == Vec2.Zero)
                {
                    path.RemoveAt(0);
                    return;
                }

                Radian dir = MathFunctions.ATan(diff.Y, diff.X);

                float halfAngle = dir * 0.5f;
                Quat rot = new Quat(new Vec3(0, 0, MathFunctions.Sin(halfAngle)),
                    MathFunctions.Cos(halfAngle));
                Rotation = rot;

                Vec2 dirVector = diff.GetNormalize();
                Vec2 dirStep = dirVector * (Type.MaxVelocity * TickDelta);

                Vec2 newPos = Position.ToVec2() + dirStep;

                if (Math.Abs(diff.X) <= Math.Abs(dirStep.X) && Math.Abs(diff.Y) <= Math.Abs(dirStep.Y))
                {
                    //unit at point
                    newPos = path[0];
                    path.RemoveAt(0);
                }

                GetNavigationSystem().RemoveObjectFromMotionMap(this);

                bool free;
                {
                    float radius = Type.Radius;
                    Rect targetRect = new Rect(newPos - new Vec2(radius, radius), newPos + new Vec2(radius, radius));
                    free = GetNavigationSystem().IsFreeInMapMotion(targetRect);
                }

                GetNavigationSystem().AddObjectToMotionMap(this);

                if (free)
                {
                    float newZ = GetNavigationSystem().GetMotionMapHeight(newPos) + Type.Height * .5f;
                    Position = new Vec3(newPos.X, newPos.Y, newZ);
                }
                else
                    path.Clear();
            }
            
        }

        protected override void OnRenderFrame()
        {
            //update animation tree
            if (EntitySystemWorld.Instance.Simulation && !EntitySystemWorld.Instance.SystemPauseOfSimulation)
            {
                AnimationTree tree = GetFirstAnimationTree();

                if (tree != null)
                    UpdateAnimationTree(tree);
            }

            base.OnRenderFrame();
        }

        // TODO kann gelöscht werden
        public void testDeath()
        {
            AnimationTree tree = GetFirstAnimationTree();
            if (tree != null)
            {
                tree.ActivateTrigger("death");
            }
        }

        void UpdateAnimationTree(AnimationTree tree)
        {
            bool move = false;
            //Degree moveAngle = 0;
            float moveSpeed = 0;

            if (mainBodyVelocity.ToVec2().Length() > .1f)
            {
                move = true;
                moveSpeed = (Rotation.GetInverse() * mainBodyVelocity).X;
                // Play sound
                PlaySound("walk");
            }

            tree.SetParameterValue("move", move ? 1 : 0);
            //tree.SetParameterValue( "moveAngle", moveAngle );
            tree.SetParameterValue("moveSpeed", moveSpeed);
        }

        /// <summary>
        /// Spielt den Sound mit entsprechendem Alias ab
        /// </summary>
        /// <param name="name"></param>
        public void PlaySound(String name)
        {
            if (timeTilNextSound <= 0 || currentSoundName != name)
            {
                if (alienChannel != null)
                {
                    alienChannel.Stop();
                }
                // Play sound
                IEnumerable<MapObjectTypeAttachedSound> sounds = this._type.AttachedObjects.OfType<MapObjectTypeAttachedSound>();
                foreach (MapObjectTypeAttachedSound s in sounds)
                {
                    if (s.Alias == name)
                    {
                        alienSound = SoundWorld.Instance.SoundCreate(s.SoundName, SoundMode.Record);
                        currentSoundName = name;
                        break;
                    }
                }
                // Sound erstellen
                if (alienSound != null)
                {
                    this.alienChannel = SoundWorld.Instance.SoundPlay(alienSound, EngineApp.Instance.DefaultSoundChannelGroup, 1f, false);
                }
                timeTilNextSound = 2;
            }
        }

        public override bool IsAllowToChangeScale(out string reason)
        {
            reason = ToolsLocalization.Translate("Various", "Characters do not support scaling.");
            return false;
        }

        public GridBasedNavigationSystem GetNavigationSystem()
        {
            //get the first instance on the map
            return GridBasedNavigationSystem.Instances[0];
        }

        /// <summary>
        /// Implements GridBasedNavigationSystem.IOverrideObjectBehavior
        /// </summary>
        /// <param name="navigationSystem"></param>
        /// <param name="obj"></param>
        /// <param name="rectangles"></param>
        public void GetMotionMapRectanglesForObject(GridBasedNavigationSystem navigationSystem, MapObject obj, List<Rect> rectangles)
        {
            rectangles.Add(new Rect(
                obj.Position.ToVec2() - new Vec2(Type.Radius, Type.Radius),
                obj.Position.ToVec2() + new Vec2(Type.Radius, Type.Radius)));
        }

        /// <summary>
        /// Selbstmord behen, d.h. Explosion erstellen und Die.
        /// </summary>
        public void DoSuicide()
        {
            Console.WriteLine("Dosuicide");


            // Alien soll sterben
            this.Die();

            // ExplosionBarrel2 erstellen
            Dynamic explosionBarrel = Entities.Instance.Create("ExplosionBarrel2", Map.Instance) as Dynamic;


            Vec3 p = this.Position + this.Rotation.GetForward();

            explosionBarrel.Position = p;
            explosionBarrel.PostCreate();

        }

        
    }
}
