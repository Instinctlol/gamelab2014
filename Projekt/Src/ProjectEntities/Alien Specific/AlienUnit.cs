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
    }
}
