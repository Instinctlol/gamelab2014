using Engine.MapSystem;
using Engine.MathEx;
using System;
using System.Collections.Generic;

namespace ProjectEntities
{
    public class StationSystem
    {
        //************* Singleton ***************
        private static StationSystem instance;

        private StationSystem() { }

        public static StationSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new StationSystem();
                }
                return instance;
            }
        }
        //***************************************

        private Dictionary<int, Ring> rings = new Dictionary<int, Ring>();
        private Dictionary<int, List<Sector>> sectors = new Dictionary<int,List<Sector>>();

        //Positionen einfach Hardcoden
        private Dictionary<int, Dictionary<int, Quat>> ringPosition = new Dictionary<int, Dictionary<int, Quat>>();

        public void RegisterRing(Ring ring)
        {
            /*
            if (rings.Keys.Contains(ring.Id) || ring.Id < 0)
                throw new Exception("Duplicated or invalid ID");


            rings.Add(ring.Id, ring);

            sectors.Add(ring.Id, new List<Sector>());

            foreach(Sector s in Map.Instance.Children.OfType<Sector>())
            {
                if(s.Ring.Id == ring.Id)
                {
                    sectors[ring.Id].Add(s);
                }
            }*/
        }

        public void RotateRing(int ringId, int position)
        {
            /*
            if (!rings.Keys.Contains(ringId))
                return;

            rings[ringId].Rotate(new Engine.MathEx.Quat(0, 0, 2, 1)); */
        }

    }
}
