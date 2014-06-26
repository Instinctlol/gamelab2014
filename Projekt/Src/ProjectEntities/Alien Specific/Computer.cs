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

        // Speichert die im Spiel durchgeführten Rotationen, damit das SectorStatusWindow diese beim initialize nachmachen kann
        // ringRotations[0] ist Ring F1, ringRotations[1] ist Ring F2 und ringRotations[2] ist Ring F3 (innerer Ring)
        // Rechtsrotationen werden mit +1 gespeichert, Linksrotationen mit -1
        static int[] ringRotations = new int[3];



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

        public static int[] RingRotations
        {
            get { return ringRotations; }
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

        /// <summary>
        /// Do one rotation of one ring to the left or to the right side.
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="left"></param>
        public static void RotateRing(Ring ring, bool left)
        {
            if (ring == null)
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Kein Ring ausgewählt");
            }
            else if (rotationCoupons <= 0)
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Keine Rotationen möglich");
            }
            else
            {
                Computer.DecrementRotationCoupons();
                int ringNumber = ring.GetRingNumber();
                if (left)
                {
                    EngineConsole.Instance.Print("links drehen");
                    ringRotations[ringNumber - 1]--;
                    ring.RotateLeft();
                }
                else
                {
                    EngineConsole.Instance.Print("rechts drehen");
                    ringRotations[ringNumber - 1]++;
                    ring.RotateRight();
                }
                ///ToDo: Gridaktualisierung für die Rotation in der Map
                GridBasedNavigationSystem.Instances[0].UpdateMotionMap();
            }
        }

        /// <summary>
        /// Switches off or on the power of one sector (room)
        /// </summary>
        /// <param name="sector"></param>
        public static void SetSectorPower(Sector sector)
        {
            if (sector == null)
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Kein Sector ausgewählt");
            }
            else if (powerCoupons <= 0)
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Kein Stromabschalten möglich");
            }
            else
            {
                Computer.DecrementPowerCoupons();
                sector.SwitchLights(false);
            }
        }

        /// <summary>
        /// Switches off or on the power of one sector (room)
        /// </summary>
        /// <param name="sectorName"></param>
        public static void SetSectorGroupPower(SectorGroup sectorGroup)
        {
            if (sectorGroup == null)
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Kein Sector ausgewählt");
            }
            else if (powerCoupons <= 0)
            {
                // Nachricht ausgeben
                StatusMessageHandler.sendMessage("Kein Stromabschalten möglich");
            }
            else
            {
                Computer.DecrementPowerCoupons();
                sectorGroup.DoSwitchLight(false);
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
