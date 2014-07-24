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

            s = "";

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
                    unit.Inventar.addItem(currentItem);
                    s = "Werkzeuggürtel";
                    break;

                case "SmallHealthItem":
                    if (unit.Health == unit.Type.HealthMax)
                        s = "Gesundheit Voll";
                    else
                        currentItem.TakeItem(unit);
                    s = "Gesundheit";
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
                    unit.Inventar.addItem(currentItem);
                    s = "Schraubenschlüssel";
                    break;

                case "Kabel":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Kabel";
                    break;

                case "TaschenlampeItem":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    unit.Inventar.taschenlampeBesitz = true;
                    unit.Inventar.taschenlampeEnergie = 100;
                    s = "Taschenlampe";
                    break;

                case "ScarItem":
                    currentItem.TakeItem(unit);
                    break;

                case "BrechstangeItem":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
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

                case "battery":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Batterie";
                    break;

                case "cpu":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "CPU";
                    break;

                case "Schrauben":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Schrauben";
                    break;

                case "USB_Stick":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "USB Stick";
                    break;

                case "sicherung":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Sicherung";
                    break;

                case "Dynamite":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Dynamit";
                    break;

                case "AccessCard":
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "AccessCard";
                    break;

                default:
                    currentItem.TakeItem(unit);
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
