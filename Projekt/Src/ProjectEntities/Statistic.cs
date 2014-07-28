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
    }
}
