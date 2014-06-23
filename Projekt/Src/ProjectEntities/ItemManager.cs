using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine;
using Engine.EntitySystem;

namespace ProjectEntities
{
    public sealed class ItemManager
    {
        private static readonly ItemManager _instance = new ItemManager();

        Item currentItem;
        PlayerCharacter character;
        string s = "";
        ItemManager()
        {

        }

        public static ItemManager Instance
        {
            get { return _instance; }
        }

        public void takeItem(Unit unit, Item item)
        {
            if(isAstronaut() == true)
            { 
            currentItem = item;
            character = unit as PlayerCharacter;

            switch (currentItem.Type.Name)
            {
                case "ShotgunBulletsItem":
                    currentItem.Take(unit);
                    break;

                case "SubmachineGunBulletsItem":
                    currentItem.Take(unit);
                    break;

                case "GlockBulletItem":
                    currentItem.Take(unit);
                    break;

                case "ScarBulletItem":
                    currentItem.Take(unit);
                    break;

                case "Defkit":
                    currentItem.Take(unit);
                    s = "Werkzeuggürtel aufgenommen";
                    break;

                case "SmallHealthItem":
                    if (unit.Health == unit.Type.HealthMax)
                        s = "Gesundheit Voll";
                    else
                        currentItem.Take(unit);
                    s = "Gesundheit aufgenommen";
                    break;

                case "Medipack":
                    if (unit.Health == unit.Type.HealthMax)
                        s = "Gesundheit Voll";
                    else
                        currentItem.Take(unit);
                    s = "Gesundheit aufgenommen";
                    break;

                case "Schraubenschlüssel":
                    currentItem.Take(unit);
                    s = "Schraubenschlüssel aufgenommen";
                    break;

                case "TaschenlampeItem":
                    currentItem.Take(unit);
                    s = "Taschenlampe aufgenommen";
                    break;

                case "ScarItem":
                    currentItem.Take(unit);
                    break;

                case "BrechstangeItem":
                    currentItem.Take(unit);
                    break;

                case "GlockItem":
                    currentItem.Take(unit);
                    break;

                case "ShotgunItem":
                    currentItem.Take(unit);
                    break;

                case "SubmachineGunItem":
                    currentItem.Take(unit);
                    break;

                case "HammerItem":
                    currentItem.Take(unit);
                    break;
                default:
                    break;
            }
            }
            
        }

        public string notificationstring()
        {
            return s;
        }


        protected bool isAstronaut()
        {
            if (EntitySystemWorld.Instance.IsServer())
                return false;
            else
                return true;
        }
    }
}
