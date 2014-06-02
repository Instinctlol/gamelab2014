using Engine.MapSystem;
using Engine.MathEx;
using ProjectCommon;
using System;
using System.Collections.Generic;

namespace ProjectEntities
{
    /*
     * Klasse zur Verwaltung von allem möglichen was auf der Station vor sich geht
     * Was genau hier alles passiert ist noch ungewiss
     */
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

        //Sektor zu einer Position  kriegen
        public Sector GetSector(Vec2 position)
        {
            Sector result = null;

            Vec3 source = new Vec3(position, 1);
            Vec3 direction = new Vec3(0, 0, -1);
            Ray ray = new Ray(source, direction);


            Map.Instance.GetObjects(ray, delegate(MapObject obj, float scale)
            {
                Sector sec = obj as Sector;

                if (sec != null)
                {
                    result = sec;
                    return false;
                }

                return true;
            });

            return result;
        }

        public Sector GetSector(Vec3 position)
        {
            Sector result = null;

            Vec3 source = position;
            Vec3 direction = new Vec3(0, 0, -1);
            Ray ray = new Ray(source, direction);


            Map.Instance.GetObjects(ray, delegate(MapObject obj, float scale)
            {
                Sector sec = obj as Sector;

                if (sec != null)
                {
                    result = sec;
                    return false;
                }

                return true;
            });

            return result;
        }

        //Ring zu einer Position kriegen
        public Ring GetRing(Vec2 position)
        {
            Sector sec = GetSector(position);

            if (sec != null)
                return sec.Ring;
            else
                return null;
        }

        //SectorGroup zzu einer Position kriegen
        public SectorGroup GetGroup(Vec2 position)
        {
            Sector sec = GetSector(position);

            if (sec != null)
                return sec.Group;
            else
                return null;
        }

    }
}
