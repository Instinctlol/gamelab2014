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

        //Waffen schon in Besitz
        bool glockInBesitz = false;
        bool scarInBesitz = false;
        bool shotgunInBesitz = false;
        bool submachinegunInBesitz = false;


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
                    if (currentItem.Name == "")
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
                    if (currentItem.Name == "")
                        currentItem.Name = "Kabel";
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Kabel";
                    break;

                case "TaschenlampeItem": 
                    if (currentItem.Name == "")
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
                    if (scarInBesitz)
                        s = "Scar schon vorhanden. Nicht";
                    else
                    {
                        currentItem.TakeItem(unit);
                        scarInBesitz = true;
                    }
                    break;

                case "Brechstange_Item":
                    if (currentItem.Name == "")
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
                    if (glockInBesitz)
                        s = "Glock schon vorhanden. Nicht";
                    else
                    {
                        currentItem.TakeItem(unit);
                        glockInBesitz = true;
                    }
                    break;

                case "ShotgunItem":
                    if (shotgunInBesitz)
                        s = "Shotgun schon vorhanden. Nicht";
                    else
                    {
                        currentItem.TakeItem(unit);
                        shotgunInBesitz = true;
                    }
                    break;

                case "SubmachineGunItem":
                    if (submachinegunInBesitz)
                        s = "Submachinegun schon vorhanden. Nicht";
                    else
                    {
                        currentItem.TakeItem(unit);
                        submachinegunInBesitz = true;
                    }
                    break;

                case "HammerItem":
                    currentItem.TakeItem(unit);
                    break;

                case "battery":
                    if (currentItem.Name == "")
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
                    if (currentItem.Name == "")
                        currentItem.Name = "CPU";
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "CPU";
                    break;

                case "Schrauben":
                    if (currentItem.Name == "")
                        currentItem.Name = "Schrauben";
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Schrauben";
                    break;

                case "USB_Stick":
                    if (currentItem.Name == "")
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
                    if (currentItem.Name == "")
                        currentItem.Name = "Sicherung";
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Sicherung";
                    break;

                case "Dynamite":
                    if (currentItem.Name == "")
                        currentItem.Name = "Dynamit";
                    currentItem.TakeItem(unit);
                    unit.Inventar.addItem(currentItem);
                    s = "Dynamit";
                    break;

                case "AccessCard":
                    if (currentItem.Name == "")
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
