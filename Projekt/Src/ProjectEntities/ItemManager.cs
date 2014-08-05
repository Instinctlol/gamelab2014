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
                    currentItem.Name = "Werkzeuggürtel";
                    if(!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.TakeItem(unit);
                        unit.Inventar.addItem(currentItem);
                        s = "Werkzeuggürtel";
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
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
                    currentItem.Name = "Schraubenschlüssel";
                    if (!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.TakeItem(unit);
                        unit.Inventar.addItem(currentItem);
                        s = "Schraubenschlüssel";
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
                    break;

                case "Kabel":
                    currentItem.Name = "Kabel";
                        currentItem.TakeItem(unit);
                        unit.Inventar.addItem(currentItem);
                        s = "Kabel";
                    break;

                case "TaschenlampeItem":
                    currentItem.Name = "Taschenlampe";
                    if (!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.TakeItem(unit);
                        unit.Inventar.addItem(currentItem);
                        unit.Inventar.taschenlampeBesitz = true;
                        unit.Inventar.taschenlampeEnergie = unit.Inventar.taschenlampeEnergieMax;
                        s = "Taschenlampe";
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
                    break;

                case "ScarItem":
                    currentItem.TakeItem(unit);
                    break;

                case "Brechstange_Item":
                    currentItem.Name = "Brechstange";
                    if (!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.TakeItem(unit);
                        unit.Inventar.addItem(currentItem);
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
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
                    currentItem.Name = "Batterie";
                    if (!unit.Inventar.taschenlampeBesitz)
                        s = "Noch keine Taschenlampe vorhanden. Batterie nicht";
                    else
                    {
                        if (unit.Inventar.taschenlampeEnergie == unit.Inventar.taschenlampeEnergieMax)
                        {
                            s = "Taschenlampebatterie ist voll. Nicht";
                        }
                        else
                        {
                            currentItem.TakeItem(unit);
                            if (unit.Inventar.taschenlampeEnergie + 50 < unit.Inventar.taschenlampeEnergieMax)
                                unit.Inventar.taschenlampeEnergie += 50;
                            else
                                unit.Inventar.taschenlampeEnergie = unit.Inventar.taschenlampeEnergieMax;
                            s = "Batterie";
                        }
                    }
                    break;

                case "cpu":
                    currentItem.Name = "CPU";
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "CPU";
                    break;

                case "Schrauben":
                    currentItem.Name = "Schrauben";
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Schrauben";
                    break;

                case "USB_Stick":
                    currentItem.Name = "USB-Stick";
                    if(!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.TakeItem(unit);
                        unit.Inventar.addItem(currentItem);
                        s = "USB Stick";
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
                    break;

                case "sicherung":
                    currentItem.Name = "Sicherung";
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Sicherung";
                    break;

                case "Dynamite":
                    currentItem.Name = "Dynamit";
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Dynamit";
                    break;

                case "AccessCard":
                    currentItem.Name = "Zugangskarte";
                    if(!unit.Inventar.inBesitz(currentItem))
                    { 
                        currentItem.TakeItem(unit);
                        unit.Inventar.addItem(currentItem);
                        s = "AccessCard";
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
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
