using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.Utils;
using ProjectCommon;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="MySpawner"/> entity type.
    /// </summary>
    public class MySpawnerType : MapObjectType
    {
    }

    /// <summary>
    /// Spawner for creating small aliens
    /// </summary>
    public class MySpawner : MapObject
    {
        [FieldSerialize]
        AIType aiType;

        [FieldSerialize]
        FactionType faction;

        [FieldSerialize]
        UnitType spawnedUnit;

        //[FieldSerialize]
        //float spawnTime;

        [FieldSerialize]
        float spawnRadius;

        [FieldSerialize]
        int popNumber;

        [FieldSerialize]
        float securityRadius;

        //counter for remaining time
        //float spawnCounter;

        //the amount of entities left to spawn
        int popAmount;


        MySpawnerType _type = null; public new MySpawnerType Type { get { return _type; } }

        [Description("Initial Faction or null for neutral")]
        public FactionType Faction
        {
            get { return faction; }
            set { faction = value; }
        }

        //[Description("Time in seconds between spawns")]
        //[DefaultValue(20.0f)]
        //public float SpawnTime
        //{
        //    get { return spawnTime; }
        //    set { spawnTime = value; }
        //}

        [Description("Spawn radius in which small aliens can be spawned. if there's no more space within this radius, no small aliens can be spawned")]
        [DefaultValue(10.0f)]
        public float SpawnRadius
        {
            get { return spawnRadius; }
            set { spawnRadius = value; }
        }

        [Description("UnitType that will be spawned, the small alien")]
        public UnitType SpawnedUnit
        {
            get { return spawnedUnit; }
            set { spawnedUnit = value; }
        }

        [Description("Default AI for this unit or none for empty unit, in general this should be the AI for the small alien")]
        public AIType AIType
        {
            get { return aiType; }
            set { aiType = value; }
        }

        [Description("max amount of entities that will be created")]
        public int PopNumber
        {
            get { return popNumber; }
            set { popNumber = value; }
        }

        [Description("when astronout enters this radius the entities will not spawn")]
        [DefaultValueAttribute(10.0f)]
        public float SecurityRadius
        {
            get { return securityRadius; }
            set { securityRadius = value; }
        }
        
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
                
            //spawnCounter = 0.0f;
            //SubscribeToTickEvent();
        }
        
        /// <summary>
        /// Function for spawning one small alien
        /// </summary>
        //Make function visible in LogicEditor
        [LogicSystemBrowsable(true)]
        public void SpawnSmallAlien()
        {
            //EngineConsole.Instance.Print("Hello Console!");
            if (isCloseToPoint()) return;

            popAmount++;
            // max amount of small aliens is not reached?
            if (popAmount <= PopNumber)
            {
                // create new small alien object
                Unit i = (Unit)Entities.Instance.Create(SpawnedUnit, Parent);

                // add AI to small alien
                if (AIType != null)
                {
                    i.InitialAI = AIType;
                }

                if (i == null) return;

                // calculate position for small alien to be spawned
                i.Position = FindFreePositionForUnit(i, Position);
                // no valid position found?
                if (i.Position.Z == -1)
                {
                    // no small alien is allowed to be spawned
                    return;
                }
                i.Rotation = Rotation;
                    
                // set good or bad faction
                if (Faction != null)
                {
                    i.InitialFaction = Faction;
                }

                // create small alien into map
                i.PostCreate();
            }
        }

        /// <summary>
        /// Calculates the next valid Position for a small alien to be spawned. If there is no more space (1,-1,-1) will be returned.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="center"></param>
        /// <returns></returns>
        Vec3 FindFreePositionForUnit(Unit unit, Vec3 center)
        {
            Vec3 volumeSize = unit.MapBounds.GetSize() + new Vec3(2, 2, 0);
            float zOffset = 0;

            // In einem Kreis mit max. spawnRadius + 3 Abstand zum Spawnpoint Position für kleines Alien berechnen
            // The position for the new small alien will be calculated within a circle and with max spawnRadius+3 distance to the spawnpoint 
            for (float radius = 3; radius < 3 + spawnRadius; radius += .6f)
            {
                for (float angle = 0; angle < MathFunctions.PI * 2; angle += MathFunctions.PI / 32)
                {
                    // new position
                    Vec3 pos = center + new Vec3(MathFunctions.Cos(angle),
                        MathFunctions.Sin(angle), 0) * radius + new Vec3(0, 0, zOffset);
                    // bound with central point pos
                    Bounds volume = new Bounds(pos);
                    volume.Expand(volumeSize * .5f);
                    // select all elements within the bound
                    Body[] bodies = PhysicsWorld.Instance.VolumeCast(
                        volume, (int)ContactGroup.CastOnlyContact);
                    // if there is no other element spawning is possible at this position
                    if (bodies.Length == 0)
                        return pos;
                }
            }
            // no valid position found: return invalid position
            return new Vec3(-1, -1, -1);
        }

        /// <summary>
        /// If the astronout is too near to the spawnpoint, no small aliens are allowed to be spawned.
        /// </summary>
        /// <returns></returns>
        bool isCloseToPoint()
        {

            bool returnValue = false;
            // invalid value for security radius
            if (SecurityRadius <= 0)
            {
                // no spawning allowed
                returnValue = true;
            }
            else
            {
                // find all MapObjects in the sphere around the position of this spawnpoint
                Map.Instance.GetObjects(new Sphere(Position, SecurityRadius), delegate(MapObject mapObject)
                {
                    // cast to PlayerCharacter, so that only player will be examined
                    PlayerCharacter pchar = mapObject as PlayerCharacter;
                        
                    // If a PlayerCharacter is found in this sphere and if it is no small alien then it must be an astronout
                    if (pchar != null && pchar.Type.Name != "Rabbit") // TODO ändern in Alien
                    {
                        // no spawning allowed
                        returnValue = true;
                    }
                });
            }
            return returnValue;
        }

        /* Reste
        private bool createElement()
        {
            //Get point, aber hier muss ein objekt angeklickt worden sein

            //MapObject point = null;
            //int safeCounter = 1;
            //while (point == null)
            //{
            //    point = enemySpawnPoints[World.Instance.Random.Next(enemySpawnPoints.Count)];
            //    safeCounter++;
            //    if (safeCounter > 1000)
            //        break;
            //}

            //if (point == null)
            //    return false;

            //check for free position
            bool freePoint;
            {
                Bounds volume = new Bounds(Position);
                volume.Expand(new Vec3(2, 2, 2));
                Body[] bodies = PhysicsWorld.Instance.VolumeCast(volume,
                    (int)ContactGroup.CastOnlyDynamic);

                freePoint = bodies.Length == 0;
            }
            if (!freePoint)
                return false;

            //Create object
            MapObject newObj = (MapObject)Entities.Instance.Create(SpawnedUnit, Parent);
            newObj.Position = Position + new Vec3(0, 0, 2);
            //if (typeName == "Robot")
            //    newObj.Position += new Vec3(0, 0, 1);
            newObj.PostCreate();

            //if (newObj is Unit)
            //    ((Unit)newObj).ViewRadius = 300;
            //newObj.Destroying += EnemyUnitDestroying;
            //newObj.Tick += EnemyUnitTick;

            return true;
        }

        // Tick geht jede Sekunde
        protected override void OnTick()
        {
            base.OnTick();

            spawnCounter += TickDelta;
            if (spawnCounter >= SpawnTime) //time to do it
            {
                spawnCounter = 0.0f;
                // Kleine Aliens spawnen
                SpawnSmallAlien();
            }
        }*/
    }
}