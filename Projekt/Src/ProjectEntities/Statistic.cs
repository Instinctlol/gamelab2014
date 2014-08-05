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
        }

        public int KilledAliens
        {
            get { return killedAliens; }
        }

        public int KilledAstronouts
        {
            get { return killedAstronouts; }
        }

        public int SpawnedAliens
        {
            get { return spawnedAliens; }
        }

        public int Reanimations
        {
            get { return reanimations; }
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
            return "Astronautenschaden: " + DamageAstronouts + "\n"
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
