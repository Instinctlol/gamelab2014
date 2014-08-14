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
                    if (!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.Name = "Schraubenschlüssel";
                        currentItem.TakeItem(unit);
                        unit.Inventar.add(currentItem);
                        s = "Schraubenschlüssel";
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
                    break;

                case "Kabel":
                    if (!unit.Inventar.inBesitz(currentItem))
                        currentItem.Name = "Kabel";
                    currentItem.TakeItem(unit);
                    unit.Inventar.add(currentItem);
                    s = "Kabel";
                    break;

                case "TaschenlampeItem":
                    if (!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.Name = "Taschenlampe";
                        currentItem.TakeItem(unit);
                        unit.Inventar.add(currentItem);
                        unit.Inventar.FlashlightOwned = true;
                        unit.Inventar.FlashlightEnergy = unit.Inventar.FlashlightEnergyMax;
                        s = "Taschenlampe";
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
                    break;

                case "ScarItem":
                    if (character.ScarInBesitz)
                        s = "Scar schon vorhanden. Nicht";
                    else
                    {
                        currentItem.TakeItem(unit);
                        character.ScarInBesitz = true;
                    }
                    break;

                case "Brechstange_Item":
                    if (!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.Name = "Brechstange";
                        currentItem.TakeItem(unit);
                        unit.Inventar.add(currentItem);
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
                    break;

                case "GlockItem":
                    if (character.GlockInBesitz)
                        s = "Glock schon vorhanden. Nicht";
                    else
                    {
                        currentItem.TakeItem(unit);
                        character.GlockInBesitz = true;
                    }
                    break;

                case "ShotgunItem":
                    if (character.ShotgunInBesitz)
                        s = "Shotgun schon vorhanden. Nicht";
                    else
                    {
                        currentItem.TakeItem(unit);
                        character.ShotgunInBesitz = true;
                    }
                    break;

                case "SubmachineGunItem":
                    if (character.SubmachinegunInBesitz)
                        s = "Submachinegun schon vorhanden. Nicht";
                    else
                    {
                        currentItem.TakeItem(unit);
                        character.SubmachinegunInBesitz = true;
                    }
                    break;

                case "HammerItem":
                    currentItem.TakeItem(unit);
                    break;

                case "battery":
                    if (!unit.Inventar.FlashlightOwned)
                        s = "Noch keine Taschenlampe vorhanden. Batterie nicht";
                    else
                    {
                        if (unit.Inventar.FlashlightEnergy == unit.Inventar.FlashlightEnergyMax)
                        {
                            s = "Taschenlampebatterie ist voll. Nicht";
                        }
                        else
                        {   
                            currentItem.TakeItem(unit);
                            if (unit.Inventar.FlashlightEnergy + 50 < unit.Inventar.FlashlightEnergyMax)
                                unit.Inventar.FlashlightEnergy += 50;
                            else
                                unit.Inventar.FlashlightEnergy = unit.Inventar.FlashlightEnergyMax;
                            s = "Batterie";
                        }
                    }
                    break;

                case "cpu":
                    if (!unit.Inventar.inBesitz(currentItem))
                        currentItem.Name = "CPU";
                    currentItem.TakeItem(unit);
                    unit.Inventar.add(currentItem);
                    s = "CPU";
                    break;

                case "Schrauben":
                    if (!unit.Inventar.inBesitz(currentItem))
                        currentItem.Name = "Schrauben";
                    currentItem.TakeItem(unit);
                    unit.Inventar.add(currentItem);
                    s = "Schrauben";
                    break;

                case "USB_Stick":
                    if(!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.Name = "USB-Stick";
                        currentItem.TakeItem(unit);
                        unit.Inventar.add(currentItem);
                        s = "USB Stick";
                    }
                    else
                    {
                        s = "Item schon vorhanden. Nicht";
                    }
                    break;

                case "sicherung":
                    if (!unit.Inventar.inBesitz(currentItem))
                        currentItem.Name = "Sicherung";
                    currentItem.TakeItem(unit);
                    unit.Inventar.add(currentItem);
                    s = "Sicherung";
                    break;

                case "Dynamite":
                    if (!unit.Inventar.inBesitz(currentItem))
                        currentItem.Name = "Dynamit";
                    currentItem.TakeItem(unit);
                    unit.Inventar.add(currentItem);
                    s = "Dynamit";
                    break;

                case "AccessCard":
                    if(!unit.Inventar.inBesitz(currentItem))
                    {
                        currentItem.Name = "Zugangskarte";
                        currentItem.TakeItem(unit);
                        unit.Inventar.add(currentItem);
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


        private bool isAstronaut()
        {
            if (EntitySystemWorld.Instance.IsServer())
                return false;
            else
                return true;
        }
    }
}
