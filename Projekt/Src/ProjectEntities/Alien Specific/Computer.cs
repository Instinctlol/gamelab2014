using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.ComponentModel;
//using Engine.EntitySystem;
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
            RotateRing2Left,
            RotateRing2Right,
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
        /// <param name="ring"></param>
        /// <param name="left"></param>
        public static void RotateRing(Ring ring, bool left)
        {
            ring = Map.Instance.SceneGraphObjects.OfType<Ring>().First<Ring>();
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
        /// <param name="sector"></param>
        /// <param name="on"></param>
        public static void SetSectorPower(Sector sector, bool on)
        {
            //sector = Map.Instance.SceneGraphObjects.OfType<Sector>().First<Sector>();

            //IEnumerable<Sector> sectors = Map.Instance.SceneGraphObjects.OfType<Sector>();
            //foreach (Sector sector in sectors)
            //{
            //    // TODO lese den entsprechenden ring zu ringNumber aus
            //    if (sector.Id == sectorId)
            //    {
                    sector.SwitchLights(on);
            //    }
            //}
        }

        //public void RotateRing(int ringId, bool left)
        //{

        //    IEnumerable<Ring> rings = Map.Instance.SceneGraphObjects.OfType<Ring>();
        //    foreach (Ring ring in rings)
        //    {
        //        // TODO lese den entsprechenden ring zu ringNumber aus
        //        if (ring.Id == ringId)
        //        {
        //            if (left)
        //            {
        //                ring.RotateLeft();
        //            }
        //            else
        //            {
        //                ring.RotateRight();
        //            }
        //        }
        //    }
        // Alternative zum Suchen der Ring-Objekte
        //List<MapObject> myspawnerpoints = Map.Instance.SceneGraphObjects.FindAll(delegate(MapObject obj)
        //{
        //    ProjectEntities.Ring ring = obj as ProjectEntities.Ring;

        //    if (ring != null)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //});
        //}
    }
}
