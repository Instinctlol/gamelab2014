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

namespace ProjectEntities.Alien_Specific
{
    /// <summary>
    /// Defines the <see cref="MySpawner"/> entity type.
    /// </summary>
    public class AlienSpawnerType : AlienUnitType
    {
    }

    /// <summary>
    /// Spawner for creating small aliens
    /// </summary>
    class AlienSpawner : AlienUnit  // ableiten von AlienUnit (neu zu erstellen, wie RTSBUILDING von RTSUnit) oder von Unit
    {
        /*von RTSBuilding.cs*/
        [FieldSerialize]
        AlienUnitType productUnitType;

        [FieldSerialize]
        [DefaultValue(0.0f)]
        float productUnitProgress;

        MapObjectAttachedMesh productUnitAttachedMesh;

        [DefaultValue(1.0f)]
        [FieldSerialize]
        float buildedProgress = 1;


        /*Ende RTSBuilding.cs*/

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


        AlienSpawnerType _type = null; public new AlienSpawnerType Type { get { return _type; } }

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





        /* von RTSBuilding.cs */
        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.</summary>
        //protected override void OnPostCreate(bool loaded)
        //{
        //    base.OnPostCreate(loaded);
        //    SubscribeToTickEvent();

        //    //for world load/save
        //    if (productUnitType != null)
        //        CreateProductUnitAttachedMesh();

        //    UpdateAttachedObjectsVisibility();
        //}

        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();
            TickProductUnit();
        }

        void TickProductUnit()
        {
            if (productUnitType == null)
                return;

            productUnitProgress += TickDelta / productUnitType.BuildTime;

            Degree angleDelta = TickDelta * 20;

            if (productUnitAttachedMesh != null)
                productUnitAttachedMesh.RotationOffset *= new Angles(0, 0, angleDelta).ToQuat();

            if (BuildUnitProgress >= 1)
            {
                CreateProductedUnit();
                StopProductUnit();
            }

            MapObjectAttachedObject buildPlatformMesh = GetFirstAttachedObjectByAlias("buildPlatform");
            if (buildPlatformMesh != null)
                buildPlatformMesh.RotationOffset *= new Angles(0, 0, angleDelta).ToQuat();
        }

        public void StartProductUnit(AlienUnitType unitType)
        {
            //StopProductUnit();

            ////check cost
            //RTSFactionManager.FactionItem factionItem = RTSFactionManager.Instance.
            //    GetFactionItemByType(Intellect.Faction);
            //if (factionItem != null)
            //{
            //    float cost = unitType.BuildCost;

            //    if (factionItem.Money - cost < 0)
            //        return;

            //    factionItem.Money -= cost;
            //}

            //productUnitType = unitType;
            //productUnitProgress = 0;

            //CreateProductUnitAttachedMesh();

            //UpdateAttachedObjectsVisibility();
        }

        public void StopProductUnit()
        {
            DestroyProductUnitAttachedMesh();

            productUnitType = null;
            productUnitProgress = 0;

            UpdateAttachedObjectsVisibility();
        }

        void CreateProductUnitAttachedMesh()
        {
            productUnitAttachedMesh = new MapObjectAttachedMesh();
            Attach(productUnitAttachedMesh);

            string meshName = null;
            Vec3 meshOffset = Vec3.Zero;
            Vec3 meshScale = new Vec3(1, 1, 1);
            {
                foreach (MapObjectTypeAttachedObject typeAttachedObject in
                    productUnitType.AttachedObjects)
                {
                    MapObjectTypeAttachedMesh typeAttachedMesh =
                        typeAttachedObject as MapObjectTypeAttachedMesh;
                    if (typeAttachedMesh == null)
                        continue;

                    meshName = typeAttachedMesh.GetMeshNameFullPath();
                    meshOffset = typeAttachedMesh.Position;
                    meshScale = typeAttachedMesh.Scale;
                    break;
                }
            }

            productUnitAttachedMesh.MeshName = meshName;

            Vec3 pos = meshOffset;
            {
                MapObjectAttachedObject buildPointAttachedHelper = GetFirstAttachedObjectByAlias("productUnitPoint");
                if (buildPointAttachedHelper != null)
                    pos += buildPointAttachedHelper.PositionOffset;
            }
            productUnitAttachedMesh.PositionOffset = pos;

            productUnitAttachedMesh.ScaleOffset = meshScale;

            if (Type.Name == "RTSHeadquaters")
            {
                foreach (MeshObject.SubObject subMesh in productUnitAttachedMesh.MeshObject.SubObjects)
                    subMesh.MaterialName = "RTSBuildMaterial";
            }
        }

        void DestroyProductUnitAttachedMesh()
        {
            if (productUnitAttachedMesh != null)
            {
                Detach(productUnitAttachedMesh);
                productUnitAttachedMesh = null;
            }
        }

        [Browsable(false)]
        public AlienUnitType BuildUnitType
        {
            get { return productUnitType; }
        }

        [Browsable(false)]
        public float BuildUnitProgress
        {
            get { return productUnitProgress; }
        }

        void CreateProductedUnit()
        {
            AlienUnit unit = (AlienUnit)Entities.Instance.Create(productUnitType, Map.Instance);

            Alien character = unit as Alien;
            if (character == null)
                Log.Fatal("RTSBuilding: CreateProductedUnit: character == null");

            GridBasedNavigationSystem navigationSystem = GridBasedNavigationSystem.Instances[0];
            Vec2 p = navigationSystem.GetNearestFreePosition(Position.ToVec2(), character.Type.Radius * 2);
            unit.Position = new Vec3(p.X, p.Y, navigationSystem.GetMotionMapHeight(p) + character.Type.Height * .5f);

            if (Intellect != null)
                unit.InitialFaction = Intellect.Faction;

            unit.PostCreate();
        }

        [DefaultValue(1.0f)]
        public float BuildedProgress
        {
            get { return buildedProgress; }
            set
            {
                buildedProgress = value;

                UpdateAttachedObjectsVisibility();
            }
        }

        protected override void OnDamage(MapObject prejudicial, Vec3 pos, Shape shape, float damage,
            bool allowMoveDamageToParent)
        {
            float oldLife = Health;

            base.OnDamage(prejudicial, pos, shape, damage, allowMoveDamageToParent);

            if (damage < 0 && BuildedProgress != 1)
            {
                BuildedProgress += (-damage) / Type.HealthMax;
                if (BuildedProgress > 1)
                    BuildedProgress = 1;

                if (BuildedProgress != 1 && Health == Type.HealthMax)
                    Health = Type.HealthMax - .01f;
            }

            float halfLife = Type.HealthMax * .5f;
            if (Health > halfLife && oldLife <= halfLife)
                UpdateAttachedObjectsVisibility();
            else if (Health < halfLife && oldLife >= halfLife)
                UpdateAttachedObjectsVisibility();

            float quarterLife = Type.HealthMax * .25f;
            if (Health > quarterLife && oldLife <= quarterLife)
                UpdateAttachedObjectsVisibility();
            else if (Health < quarterLife && oldLife >= quarterLife)
                UpdateAttachedObjectsVisibility();
        }

        void UpdateAttachedObjectsVisibility()
        {
            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                //lessHalfLife
                if (attachedObject.Alias == "lessHalfLife")
                {
                    attachedObject.Visible = (Health < Type.HealthMax * .5f && buildedProgress == 1);
                    continue;
                }

                //lessQuarterLife
                if (attachedObject.Alias == "lessQuarterLife")
                {
                    attachedObject.Visible = (Health < Type.HealthMax * .25f && buildedProgress == 1);
                    continue;
                }

                //productUnit
                if (attachedObject.Alias == "productUnit")
                {
                    attachedObject.Visible = productUnitType != null;
                    continue;
                }

                //building
                {
                    string showAlias = null;

                    if (buildedProgress < .25f)
                        showAlias = "building0";
                    else if (buildedProgress < .5f)
                        showAlias = "building1";
                    else if (buildedProgress < 1)
                        showAlias = "building2";

                    if (showAlias != null)
                        attachedObject.Visible = (attachedObject.Alias == showAlias);
                    else
                        attachedObject.Visible = !attachedObject.Alias.Contains("building");
                }

            }
        }

        /* Ende RTSBuilding.cs*/
    }
}