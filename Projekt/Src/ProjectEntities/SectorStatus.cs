namespace ProjectEntities
{
    public class SectorStatus
    {
        private bool light, spawnpoint;
        private string name, uebergang, raum;

        //Wie der Raum gedreht ist
        public enum raumRot
        {
            raum0,
            raum90,
            raum45,
            raum135,
            raum225,
            raum315
        }
        

    }
}
