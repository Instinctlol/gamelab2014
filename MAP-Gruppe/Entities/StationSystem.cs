using Engine.MapSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private List<Ring> rings = new List<Ring>();
        private Dictionary<int, List<Sector>> sectors;


        public void RegisterRing(Ring ring)
        { 
        }

    }
}
