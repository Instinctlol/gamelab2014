using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class Statistic
    {
        /*************/
        /* Attribute */
        /*************/
        float damageAstronouts = 0;
        int killedAliens = 0;
        int killedAstronouts = 0;
        int spawnedAliens = 0;
        int reanimations = 0;




        /*******************/
        /* Getter / Setter */
        /*******************/
        public float DamageAstronouts
        {
            get { return damageAstronouts; }
            set { damageAstronouts = value; }
        }

        public int KilledAliens
        {
            get { return killedAliens; }
            set { killedAliens = value; }
        }

        public int KilledAstronouts
        {
            get { return killedAstronouts; }
            set { killedAstronouts = value; }
        }

        public int SpawnedAliens
        {
            get { return spawnedAliens; }
            set { spawnedAliens = value; }
        }

        public int Reanimations
        {
            get { return reanimations; }
            set { reanimations = value; }
        }




        /**************/
        /* Funktionen */
        /**************/
        public void AddDamageAstronouts(float value)
        {
            damageAstronouts += value;
        }

        public void IncrementKilledAliens()
        {
            killedAliens++;
        }

        public void IncrementKilledAstronouts()
        {
            killedAstronouts++;
        }

        public void IncrementSpawnedAliens()
        {
            spawnedAliens++;
        }

        public void IncrementReanimations()
        {
            reanimations++;
        }

        public String GetAlienData()
        {
            if (!Computer.Instance.Astronautwin)
            {
                KilledAstronouts = Reanimations + 2;
            }
            return "Astronautenschaden: " + ((int)DamageAstronouts) + "\n"
                + "Getötete Astronauten: " + KilledAstronouts + "\n"
                + "Gespawnte Aliens: " + SpawnedAliens;
        }

        public String GetAstronoutData()
        {
            return "Getötete Aliens: " + KilledAliens + "\n"
                + "Wiederbelebungen: " + Reanimations;
        }
    }
}
