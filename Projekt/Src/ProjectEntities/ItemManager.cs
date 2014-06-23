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

        public void TakeItem(Unit unit, Item item)
        {
            if(isAstronaut() == true)
            { 
            currentItem = item;
            character = unit as PlayerCharacter;

            switch (currentItem.Type.Name)
            {
                case "ShotgunBulletsItem":
                    currentItem.TakeItem(unit);
                    break;

                case "SubmachineGunBulletsItem":
                    currentItem.TakeItem(unit);
                    break;

                case "GlockBulletItem":
                    currentItem.TakeItem(unit);
                    break;

                case "ScarBulletItem":
                    currentItem.TakeItem(unit);
                    break;

                case "Defkit":
                    currentItem.TakeItem(unit);
                    s = "Werkzeuggürtel aufgenommen";
                    break;

                case "SmallHealthItem":
                    if (unit.Health == unit.Type.HealthMax)
                        s = "Gesundheit Voll";
                    else
                        currentItem.TakeItem(unit);
                    s = "Gesundheit aufgenommen";
                    break;

                case "Medipack":
                    if (unit.Health == unit.Type.HealthMax)
                        s = "Gesundheit Voll";
                    else
                        currentItem.TakeItem(unit);
                    s = "Gesundheit aufgenommen";
                    break;

                case "Schraubenschlüssel":
                    currentItem.TakeItem(unit);
                    s = "Schraubenschlüssel aufgenommen";
                    break;

                case "TaschenlampeItem":
                    currentItem.TakeItem(unit);
                    s = "Taschenlampe aufgenommen";
                    break;

                case "ScarItem":
                    currentItem.TakeItem(unit);
                    break;

                case "BrechstangeItem":
                    currentItem.TakeItem(unit);
                    break;

                case "GlockItem":
                    currentItem.TakeItem(unit);
                    break;

                case "ShotgunItem":
                    currentItem.TakeItem(unit);
                    break;

                case "SubmachineGunItem":
                    currentItem.TakeItem(unit);
                    break;

                case "HammerItem":
                    currentItem.TakeItem(unit);
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
