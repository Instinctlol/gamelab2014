using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        ComputerType _type = null; public new ComputerType Type { get { return _type; } }

        // der Zentralcomputer soll alle Images für die Minimap verwalten und immer das korrekte anzeigen
        // Zentralcomputer speichert also indirekt, wie die Ringe stehen

        // Alle Aktionen, die man über den Zentralcomputer steuern kann
        public enum Actions
        {
            State,
            RotateRing1Left,
            RotateRing1Right,
            RotateRing3Left,
            RotateRing3Right,
            LightSector1,
            LightSector2
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
            Ring ring = ((Ring)Entities.Instance.GetByName(ringName));
            if (left)
            {
                ring.RotateLeft();
            }
            else
            {
                ring.RotateRight();
            }
        }

        /// <summary>
        /// Switches off or on the power of one sector (room)
        /// </summary>
        /// <param name="sectorName"></param>
        public static void SetSectorPower(String sectorName)
        {
            Sector sector = ((Sector)Entities.Instance.GetByName(sectorName));
            sector.SwitchLights(!sector.LightStatus);
        }
    }
}
