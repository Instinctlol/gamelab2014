using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using ProjectCommon;

namespace ProjectEntities
{
    public class ComputerType : DynamicType
    {
    }

    /// <summary>
    /// Main computer that can be controlled by the boss alien and
    /// the astronauts
    /// </summary>
    public class Computer : Dynamic
    {
        /*************/
        /* Attribute */
        /*************/
        ComputerType _type = null; public new ComputerType Type { get { return _type; } }
        const float maxSolvableRepairablesNeeded = 20;
        const int maxAliens = 20;
        const int maxPowerCoupons = 10;
        const int maxRotationCoupons = 10;

        [FieldSerialize]
        static int solvedRepairables = 10;
        static int experiencePoints = 100;
        static int rotationCoupons = 3;
        static int powerCoupons = 2;
        static int availableAliens = 5;
        static int usedAliens = 0;
        public static bool noSpawnTime = false;



        /*******************/
        /* Getter / Setter */
        /*******************/
        public static int ExperiencePoints
        {
            get { return experiencePoints; }
            set { experiencePoints = value; }
        }

        public static int RotationCoupons
        {
            get { return rotationCoupons; }
        }

        public static int PowerCoupons
        {
            get { return powerCoupons; }
        }

        public static int AvailableAliens
        {
            get { return availableAliens; }
        }

        public static int UsedAliens
        {
            get { return usedAliens; }
        }



        /**************/
        /* Funktionen */
        /**************/
        /// <summary>
        /// Rotation Coupons um Eins erhöhen
        /// </summary>
        public static void IncrementRotationCoupons()
        {
            if (rotationCoupons < maxRotationCoupons)
            {
                rotationCoupons++;
            }
        }
        /// <summary>
        /// Rotation Coupons um Eins erniedrigen
        /// </summary>
        public static void DecrementRotationCoupons()
        {
            if (rotationCoupons > 0)
            {
                rotationCoupons--;
            }
        }

        /// <summary>
        /// Power Coupons um Eins erhöhen
        /// </summary>
        public static void IncrementPowerCoupons()
        {
            if (powerCoupons < maxPowerCoupons)
            {
                powerCoupons++;
            }
        }
        /// <summary>
        /// Power Coupons um Eins erniedrigen
        /// </summary>
        public static void DecrementPowerCoupons()
        {
            if (powerCoupons > 0)
            {
                powerCoupons--;
            }
        }

        /// <summary>
        /// Ein Alien wurde gespawnt, d.h. Ein verfügbares Alien weniger und ein aktives Alien mehr
        /// </summary>
        public static void AddUsedAlien()
        {
            if (availableAliens > 0)
            {
                DecrementAvailableAliens();
                IncrementUsedAliens();
            }
        }

        /// <summary>
        /// Verfügbare Aliens um Eins erhöhen
        /// </summary>
        public static void IncrementAvailableAliens()
        {
            if ((availableAliens + usedAliens) < maxAliens)
            {
                availableAliens++;
            }
        }
        /// <summary>
        /// Verfügbare Aliens um Eins erniedrigen
        /// </summary>
        private static void DecrementAvailableAliens()
        {
            availableAliens--;
        }

        /// <summary>
        /// Aktive Aliens um Eins erhöhen
        /// </summary>
        private static void IncrementUsedAliens()
        {
            usedAliens++;
        }
        /// <summary>
        /// Aktive Aliens um Eins erniedrigen
        /// </summary>
        public static void DecrementUsedAliens()
        {
            if (usedAliens > 0)
            {
                usedAliens--;
            }
        }

        /// <summary>
        /// Die gelösten Repairables um Eins erhöhen
        /// </summary>
        public static void IncrementSolvedRepairables()
        {
            if (solvedRepairables < maxSolvableRepairablesNeeded)
            {
                solvedRepairables++;
            }
        }

        /// <summary>
        /// Liefert den Status der Raumstation in Form des Anteils gelöster Repairables
        /// </summary>
        /// <returns></returns>
        public static float GetStationStatus()
        {
            return solvedRepairables / maxSolvableRepairablesNeeded;
        }

        // der Zentralcomputer soll alle Images für die Minimap verwalten und immer das korrekte anzeigen
        // Zentralcomputer speichert also indirekt, wie die Ringe stehen

        // Alle Aktionen, die man über den Zentralcomputer steuern kann
        public enum Actions
        {
            State,
            RotateR1Left,
            RotateR1Right,
            RotateR3Left,
            RotateR3Right,
            LightS1,
            LightS2
        }

        /// <summary>
        /// Show the state of the station, e.g. how many life the astronauts 
        /// still have, how the rings are positioned...
        /// </summary>
        public static void ShowState()
        {

        }

        /// <summary>
        /// Do one rotation of one ring to the left or to the right side.
        /// </summary>
        /// <param name="ringName"></param>
        /// <param name="left"></param>
        public static void RotateRing(String ringName, bool left)
        {
            if (rotationCoupons > 0)
            {
                Computer.DecrementRotationCoupons();
                Ring ring = ((Ring)Entities.Instance.GetByName(ringName));
                if (left)
                {
                    ring.RotateLeft();
                }
                else
                {
                    ring.RotateRight();
                }
                ///ToDo: Gridaktualisierung für die Rotation in der Map
                GridBasedNavigationSystem.Instances[0].UpdateMotionMap();
                
            }
            else
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Keine Rotationen möglich");
            }
        }

        /// <summary>
        /// Switches off or on the power of one sector (room)
        /// </summary>
        /// <param name="sectorName"></param>
        public static void SetSectorPower(String sectorName)
        {
            if (powerCoupons > 0)
            {
                Computer.DecrementPowerCoupons();
                Sector sector = ((Sector)Entities.Instance.GetByName(sectorName));
                sector.SwitchLights(!sector.LightStatus);
            }
            else
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Kein Stromabschalten möglich");
            }
        }

        /// <summary>
        /// Switches off or on the power of one sector (room)
        /// </summary>
        /// <param name="sectorName"></param>
        public static void SetSectorGroupPower(String sectorGroupName)
        {
            if (powerCoupons > 0)
            {
                Computer.DecrementPowerCoupons();
                SectorGroup sectorgrp = ((SectorGroup)Entities.Instance.GetByName(sectorGroupName));
                sectorgrp.DoSwitchLight(!sectorgrp.LightStatus);
            }
            else
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Kein Stromabschalten möglich");
            }
        }

        /// <summary>
        /// Liefert die Anzahl Astronauten, die leben
        /// </summary>
        /// <returns></returns>
        public static int GetNumberOfActiveAstronauts()
        {
            IEnumerable<GameCharacter> player = Entities.Instance.EntitiesCollection.OfType<GameCharacter>();
            int counter = player.Count();
            // Falls tote Spieler trotzdem ausgelesen werden prüfen, ob diese noch Lebenspunkte haben oder schon tod sind
            foreach (GameCharacter p in player)
            {
                if (p.IsDestroyed || p.Died || p.Health <= 0)
                {
                    // Dann um Eins dekrementieren
                    counter--;
                }
            }
            // Anzahl zurückgeben
            return counter;
        }

        /// <summary>
        /// Alles wird auf das Maximum gesetzt
        /// </summary>
        public static void SetToMaximum()
        {
            availableAliens = (maxAliens - usedAliens);
            rotationCoupons = maxRotationCoupons;
            powerCoupons = maxPowerCoupons;
        }
    }
}
