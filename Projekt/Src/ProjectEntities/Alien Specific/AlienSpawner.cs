using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using Engine.UISystem;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.SoundSystem;
using Engine.Utils;
using ProjectCommon;

using Engine.FileSystem;
using ProjectEntities;

namespace ProjectEntities
{
    /// <summary>
    /// Defines the <see cref="AlienSpawner"/> entity type.
    /// </summary>
    public class AlienSpawnerType : AlienUnitType
    {
    }

    /// <summary>
    /// Spawner for creating small aliens
    /// </summary>
    public class AlienSpawner : AlienUnit
    {
        /*************/
        /* Attribute */
        /*************/
        [FieldSerialize]
        [DefaultValue(0.0f)]
        float productUnitProgress;
        MapObjectAttachedMesh productUnitAttachedMesh;

        [FieldSerialize]
        AlienType spawnedUnit;

        [DefaultValue(1.0f)]
        [FieldSerialize]
        float buildedProgress = 1;

        [FieldSerialize]
        AIType aiType;

        [FieldSerialize]
        Sector sector;

        //the amount of entities left to spawn
        int aliensToSpawn;

        AlienSpawnerType _type = null; public new AlienSpawnerType Type { get { return _type; } }



        /*******************/
        /* Getter / Setter */
        /*******************/
        [Description("Sector in which the Spawner is")]
        public Sector Sector
        {
            get { return sector; }
            set { sector = value; }
        }

        [Description("AlienUnitType that will be spawned, the small alien")]
        [Browsable(false)]
        public AlienType SpawnedUnit
        {
            get { return spawnedUnit; }
            set { spawnedUnit = value; }
        }

        [Description("Default AI for this unit or none for empty unit, in general this should be the AI for the small alien")]
        [Browsable(false)]
        public AIType AIType
        {
            get { return aiType; }
            set { aiType = value; }
        }

        [Browsable(false)]
        public float BuildUnitProgress
        {
            get { return productUnitProgress; }
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



        /**************/
        /* Funktionen */
        /**************/
        /// <summary>
        /// Overridden from <see cref="Engine.EntitySystem.Entity.OnPostCreate(Boolean)"/>.
        /// </summary>
        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            SubscribeToTickEvent();

            //for world load/save
            if (spawnedUnit != null)
                CreateProductUnitAttachedMesh();

           

            if (!GameMap.Instance.IsAlien)
                Visible = false;

            UpdateAttachedObjectsVisibility();
        }
        
        /// <summary>Overridden from <see cref="Engine.EntitySystem.Entity.OnTick()"/>.</summary>
        protected override void OnTick()
        {
            base.OnTick();
            TickProductUnit();
        }

        /// <summary>
        /// Wird bei jedem Tick-Event ausgeführt. Überprüfung, ob die Zeit schon rum ist, um die zu spawnenden Aliens freizugeben.
        /// </summary>
        void TickProductUnit()
        {
            if (spawnedUnit == null)
                return;

            productUnitProgress += TickDelta / spawnedUnit.BuildTime;

            Degree angleDelta = TickDelta * 20;

            if (productUnitAttachedMesh != null)
                productUnitAttachedMesh.RotationOffset *= new Angles(0, 0, angleDelta).ToQuat();

            if (BuildUnitProgress >= 1 || Computer.noSpawnTime)
            {
                CreateProductedUnit();
                StopProductUnit();
            }

            MapObjectAttachedObject buildPlatformMesh = GetFirstAttachedObjectByAlias("buildPlatform");
            if (buildPlatformMesh != null)
                buildPlatformMesh.RotationOffset *= new Angles(0, 0, angleDelta).ToQuat();
        }

        /// <summary>
        /// Produktion von Aliens starten (Wird in der AlienSpawnerAI aufgerufen, wenn ein entsprechender Task ausgeführt werden soll).
        /// </summary>
        /// <param name="unitType"></param>
        /// <param name="spawnNumber"></param>
        public void StartProductUnit(AlienType unitType, int spawnNumber)
        {
            StopProductUnit();

            //check cost
            //RTSFactionManager.FactionItem factionItem = RTSFactionManager.Instance.
            //    GetFactionItemByType(Intellect.Faction);
            //if (factionItem != null)
            //{
            //    float cost = unitType.BuildCost;

            //    if (factionItem.Money - cost < 0)
            //        return;

            //    factionItem.Money -= cost;
            //}

            bool spawningAllowed = true;
            
            // Prüfung ob genügend Aliens verfügbar sind
            if (Computer.AvailableAliens == 0)
            {
                // Kein Alien verfügbar
                spawningAllowed = false;
                // Nachricht anzeigen
                StatusMessageHandler.sendMessage("Es sind keine Aliens verfügbar");
            }
            else if (Computer.AvailableAliens < spawnNumber)
            {
                // Nicht Genügend Aliens verfügbar, aber min. ein Alien kann gespawnt werden
                // Nachricht anzeigen
                StatusMessageHandler.sendMessage(String.Format("Es können nur {0:d} Aliens gespawnt werden.", Computer.AvailableAliens));
            }
            // Befinden sich Astronauten im Raum?
            if (IsAstronoutInSector())
            {
                spawningAllowed = false;
                // Nachricht anzeigen und nicht spawnen
                StatusMessageHandler.sendMessage("Es befinden sich Astronauten in unmittelbarer Nähe");
            }

            // Darf noch gespawnt werden?
            if (spawningAllowed)
            {
                // spawnedUnit setzen, damit diese gespawnt werden können
                spawnedUnit = unitType;
                aliensToSpawn = spawnNumber;
                productUnitProgress = 0;
                CreateProductUnitAttachedMesh();
                UpdateAttachedObjectsVisibility();
            }
        }

        /// <summary>
        /// Produktion stoppen
        /// </summary>
        public void StopProductUnit()
        {
            DestroyProductUnitAttachedMesh();

            spawnedUnit = null;
            productUnitProgress = 0;

            UpdateAttachedObjectsVisibility();
        }

        /// <summary>
        /// Mesh für das neue Alien erzeugen
        /// </summary>
        void CreateProductUnitAttachedMesh()
        {
            productUnitAttachedMesh = new MapObjectAttachedMesh();
            Attach(productUnitAttachedMesh);

            string meshName = null;
            Vec3 meshOffset = Vec3.Zero;
            Vec3 meshScale = new Vec3(1, 1, 1);
            {
                foreach (MapObjectTypeAttachedObject typeAttachedObject in
                    spawnedUnit.AttachedObjects)
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
        }

        /// <summary>
        /// Mesh für Alien zerstören, wenn Produktion abgebrochen wurde
        /// </summary>
        void DestroyProductUnitAttachedMesh()
        {
            if (productUnitAttachedMesh != null)
            {
                Detach(productUnitAttachedMesh);
                productUnitAttachedMesh = null;
            }
        }

        /// <summary>
        /// So viele Aliens wie geordert (und möglich) produzieren und an die berechnete Position setzen.
        /// </summary>
        void CreateProductedUnit()
        {
            while (aliensToSpawn > 0 && Computer.AvailableAliens > 0)
            {
                // Objekt erstellen
                Alien alien = (Alien)Entities.Instance.Create(spawnedUnit, Map.Instance);

                if (alien == null)
                    Log.Fatal("RTSBuilding: CreateProductedUnit: character == null");

                // Position berechnen
                GridBasedNavigationSystem navigationSystem = GridBasedNavigationSystem.Instances[0];
                Vec2 p = navigationSystem.GetNearestFreePosition(Position.ToVec2(), alien.Type.Radius * 2);
                alien.Position = new Vec3(p.X, p.Y, navigationSystem.GetMotionMapHeight(p) + alien.Type.Height * .5f);

                // Faction und AI setzen
                if (Intellect != null)
                    alien.InitialFaction = Intellect.Faction;

                // Alien in Map erstellen
                alien.PostCreate();

                // Anzahl zu spawnender Aliens anpassen
                aliensToSpawn--;
                // Computer aktualisieren
                Computer.AddUsedAlien();
            }
        }

        /// <summary>
        /// If the astronout is in the same sector as the activated spawnpoint, no small aliens are allowed to be spawned.
        /// </summary>
        /// <returns></returns>
        bool IsAstronoutInSector()
        {
            if (sector == null)
            {
                throw new ArgumentNullException("Der AlienSpawner hat keinen Sector zugewiesen. Bitte im MapEditor ändern!");
            }
            if (sector.ObjectsInRegion != null && sector.ObjectsInRegion.Count > 0)
            {
                // Iteriere durch alle MapObjects in dem Sector
                foreach (MapObject obj in sector.ObjectsInRegion)
                {
                    // wurde ein Astronaut gefunden?
                    if (obj.Type is GameCharacterType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // Der Spawnpoint soll nicht zerstörbar sein
        protected override void OnDamage(MapObject prejudicial, Vec3 pos, Shape shape, float damage,
            bool allowMoveDamageToParent)
        {
            // Einfach nichts machen
            return;
            //float oldLife = Health;

            //base.OnDamage(prejudicial, pos, shape, damage, allowMoveDamageToParent);

            //if (damage < 0 && BuildedProgress != 1)
            //{
            //    BuildedProgress += (-damage) / Type.HealthMax;
            //    if (BuildedProgress > 1)
            //        BuildedProgress = 1;

            //    if (BuildedProgress != 1 && Health == Type.HealthMax)
            //        Health = Type.HealthMax - .01f;
            //}

            //float halfLife = Type.HealthMax * .5f;
            //if (Health > halfLife && oldLife <= halfLife)
            //    UpdateAttachedObjectsVisibility();
            //else if (Health < halfLife && oldLife >= halfLife)
            //    UpdateAttachedObjectsVisibility();

            //float quarterLife = Type.HealthMax * .25f;
            //if (Health > quarterLife && oldLife <= quarterLife)
            //    UpdateAttachedObjectsVisibility();
            //else if (Health < quarterLife && oldLife >= quarterLife)
            //    UpdateAttachedObjectsVisibility();
        }

        /// <summary>
        /// Alien sichtbar machen
        /// </summary>
        void UpdateAttachedObjectsVisibility()
        {
            foreach (MapObjectAttachedObject attachedObject in AttachedObjects)
            {
                //spawnedUnit
                if (attachedObject.Alias == "spawnedUnit")
                {
                    attachedObject.Visible = spawnedUnit != null;
                    continue;
                }
            }
        }
    }
}