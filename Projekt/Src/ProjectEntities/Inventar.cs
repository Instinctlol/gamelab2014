using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class Inventar
    {
        //Inventarliste
        List<Item> inventar;

        //aktuell ausgwähltes Item, welches benutzt werden kann
        private Item _useItem;

        //Taschenlampe
        private int _taschenlampeEnergie;
        private int _taschenlampeEnergieMax = 100;
        private bool _taschenlampeBesitz = false;
        private bool _taschenlampevisible = false;

        private bool isOpen;

        public bool IsOpen
        {
            get { return isOpen; }
            set { isOpen = value; }
        }
        public int taschenlampeEnergieMax
        {
            get { return _taschenlampeEnergieMax; }
            set { _taschenlampeEnergieMax = value; }
        }

        public bool taschenlampevisible
        {
            get { return _taschenlampevisible; }
            set { _taschenlampevisible = value; }
        }

        public bool taschenlampeBesitz
        {
            get { return _taschenlampeBesitz; }
            set { _taschenlampeBesitz = value; }
        }

        public int taschenlampeEnergie
        {
            get { return _taschenlampeEnergie; }
            set { _taschenlampeEnergie = value; }
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Inventar()
        {
            this.inventar = new List<Item>();
        }

        public Item useItem
        {
            get{ return _useItem; }
            set{ _useItem = value; }
        }

        public List<Item> getInventarliste()
        {
            return inventar;
        }

        public void setUseItem(int index)
        {
            if(index >= 0 && index < inventar.Count)
                _useItem = inventar[index];
        }

        public void addItem(Item i)
        {
            if(i != null && !this.isWeaponOrBullet(i))
            {
                //Prüft ob das Item schon vorhanden ist
                //wenn ja dann wird die Anzahl erhöht, ansonsten wird es hinzugefügt
                if (inventar.Exists(x => x.Type.Name == i.Type.Name))
                {
                    inventar.Find(x => x.Type.Name == i.Type.Name).anzahl++;
                }
                else
                {
                    i.anzahl++;
                    inventar.Add(i);

                    //Das erste useItem setzen
                    if (useItem == null)
                        setUseItem(0);
                }
                    
                
            }
        }

        public bool removeItem(Item i)
        {
            //Prüft ob Inventar und Item nicht leer sind und ob das Item im Inventar enthalten ist 
            if (i != null && inventar.Count != 0 && inventar.Exists(x => x.Type.Name == i.Type.Name))
            {
                //Item komplett entfernen bzw Itemanzahl verringern
                if (inventar.Find(x => x.Type.Name == i.Type.Name).anzahl == 1)
                {
                    inventar.RemoveAt(inventar.IndexOf(inventar.Find(x => x.Type.Name == i.Type.Name)));
                    setUseItem(0);
                }
                else
                    inventar.Find(x => x.Type.Name == i.Type.Name).anzahl--;
            }
                
            return false;
        }

        public bool isWeaponOrBullet(Item i)
        {
            //Handelt es sich um eine Waffe bzw Munition oder um ein anderes Item
            if ( (i.Type.ClassInfo.ToString() == "WeaponItem" || i.Type.ClassInfo.ToString() == "BulletItem")  && !i.Type.FullName.ToLower().Equals("brechstangeitem") )
                return true;
            else
                return false;
        }

        public Item getItem(int index)
        {
            if (index < 0)
                return null;
            else
            {
                if (inventar.Count <= 0)
                {
                    //Fehler ausgeben, dass Inventar leer ist
                    return null;
                }
                else
                {
                    Item item = inventar[index];
                    return item;
                }
            }        
        }

        public int getIndexUseItem() 
        {
            return inventar.IndexOf(useItem);
        }

        public bool inBesitz(Item i)
        {
            if (inventar.Exists(x => x.Type.Name == i.Type.Name))
                return true;
            else
                return false;
        }
    }
}
