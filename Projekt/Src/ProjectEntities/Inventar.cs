using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using Engine;
using Engine.EntitySystem;
using Engine.MathEx;
using Engine.MapSystem;
using Engine.PhysicsSystem;
using Engine.Renderer;
using Engine.Utils;
using ProjectCommon;
using System.Timers;

namespace ProjectEntities
{

    public class InventarType : MapObjectType
    {

    }

    public class Inventar : MapObject
    {
        //Inventarliste
        List<Item> inventar;

        //aktuell ausgwähltes Item, welches benutzt werden kann
        private Item _useItem;

        enum NetworkMessages
        {
            AddItem,
            RemoveItem,
            FlashLightVisibleToClient,
            FlashLightOwnedToClient,
            FlashLightEnergyToClient,
            FlashLightVisibleToServer,
            FlashLightOwnedToServer,
            FlashLightEnergyToServer,
        }

        InventarType _type = null; public new InventarType Type { get { return _type; } }

        //Taschenlampe
        private int flashlightEnergy;
        private int flashlightEnergyMax = 100;
        private bool flashlightOwned = false;
        private bool flashlightVisible = false;
        //Taschenlampe Timer
        private Timer energieTimer;


        private bool isOpen;

        public bool IsOpen
        {
            get { return isOpen; }
            set { isOpen = value; }
        }
        public int FlashlightEnergyMax
        {
            get { return flashlightEnergyMax; }
        }

        public bool FlashlightVisible
        {
            get { return flashlightVisible; }
            set
            {
                if (value && FlashlightOwned && FlashlightEnergy > 0)
                {
                    flashlightVisible = true;
                }
                else
                {
                    flashlightVisible = false;
                }

                if (EntitySystemWorld.Instance.IsServer())
                {
                    if (flashlightVisible)
                    {

                        energieTimer.AutoReset = true;
                        energieTimer.Enabled = true;
                    }
                    else
                    {

                        energieTimer.AutoReset = false;
                        energieTimer.Enabled = false;
                    }

                    Server_SendFlashlightStatusToClient(flashlightVisible);
                }
                else
                    Client_SendFlashlightVisibleToServer(flashlightVisible);
            }
        }





        public bool FlashlightOwned
        {
            get { return flashlightOwned; }
            set
            {
                flashlightOwned = value;
                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendFlashlightOwnedToClient(flashlightOwned);
                else
                    Client_SendFlashlightOwnedToServer(flashlightOwned);
            }
        }





        public int FlashlightEnergy
        {
            get { return flashlightEnergy; }
            set
            {
                flashlightEnergy = value;
                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendFlashlightEnergyToClient(flashlightEnergy);
                else
                    Client_SendFlashlightEnergyToServer(flashlightEnergy);
            }
        }





        /// <summary>
        /// Default constructor
        /// </summary>
        public Inventar()
        {
            this.inventar = new List<Item>();

            if (EntitySystemWorld.Instance.IsServer())
            {
                energieTimer = new Timer();
                energieTimer.Interval = 5000;
                energieTimer.Elapsed += new ElapsedEventHandler(tlEnergieVerringern);
            }
        }

        public Item useItem
        {
            get { return _useItem; }
            set { _useItem = value; }
        }

        public List<Item> getInventarliste()
        {
            return inventar;
        }

        public void setUseItem(int index)
        {
            if (index >= 0 && index < inventar.Count)
                _useItem = inventar[index];
        }

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);
            Server_SendFlashlightEnergyToClient(flashlightEnergy);
            Server_SendFlashlightOwnedToClient(flashlightOwned);
            Server_SendFlashlightStatusToClient(flashlightVisible);
        }

        public void addItem(Item i)
        {
            if (i != null && !this.isWeaponOrBullet(i))
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
            if ((i.Type.ClassInfo.ToString() == "WeaponItem" || i.Type.ClassInfo.ToString() == "BulletItem") && !i.Type.FullName.ToLower().Equals("brechstangeitem"))
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

        public void add(Item item)
        {
            if (EntitySystemWorld.Instance.IsServer())
                addItem(item);
            else
            {
                Client_AddItem(item);
                addItem(item);
            }
        }

        void Client_AddItem(Item item)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Inventar), (ushort)NetworkMessages.AddItem);
            writer.Write(item.Type.Name);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.AddItem)]
        void Server_AddItem(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            String itemtype = reader.ReadString();

            if (!reader.Complete())
                return;

            Item i = (Item)Entities.Instance.Create(itemtype, Map.Instance);
            addItem(i);
        }

        public void remove(Item i)
        {
            if (EntitySystemWorld.Instance.IsServer())
                removeItem(i);
            else
            {
                Client_RemoveItem(i);
                removeItem(i);
            }
        }

        private void tlEnergieVerringern(object source, ElapsedEventArgs e)
        {
            if (FlashlightEnergy > 0)
                FlashlightEnergy -= 2;
            else
            {
                FlashlightVisible = false;
                energieTimer.AutoReset = false;
                energieTimer.Enabled = false;
            }
        }

        void Client_RemoveItem(Item i)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Inventar), (ushort)NetworkMessages.RemoveItem);
            writer.Write(i.Type.Name);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.RemoveItem)]
        void Server_RemoveItem(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            String itemtype = reader.ReadString();

            if (!reader.Complete())
                return;

            Item i = (Item)Entities.Instance.Create(itemtype, Map.Instance);
            removeItem(i);
        }

        private void Server_SendFlashlightStatusToClient(bool _taschenlampevisible)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Inventar), (ushort)NetworkMessages.FlashLightVisibleToClient);
            writer.Write(_taschenlampevisible);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.FlashLightVisibleToClient)]
        void Client_ReceiveFlashlightStatus(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool status = reader.ReadBoolean();

            if (!reader.Complete())
                return;

            flashlightVisible = status;
        }

        private void Server_SendFlashlightOwnedToClient(bool _taschenlampeBesitz)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Inventar), (ushort)NetworkMessages.FlashLightOwnedToClient);
            writer.Write(_taschenlampeBesitz);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.FlashLightOwnedToClient)]
        void Client_ReceiveFlashlightOwned(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool status = reader.ReadBoolean();

            if (!reader.Complete())
                return;

            flashlightOwned = status;
        }

        private void Server_SendFlashlightEnergyToClient(int _taschenlampeEnergie)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Inventar), (ushort)NetworkMessages.FlashLightEnergyToClient);
            writer.Write(_taschenlampeEnergie);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.FlashLightEnergyToClient)]
        void Client_ReceiveFlashlightBattery(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            int energy = reader.ReadInt32();

            if (!reader.Complete())
                return;

            flashlightEnergy = energy;
            if (flashlightEnergy <= 0)
                StatusMessageHandler.sendMessage("Batterie der Taschenlampe ist leer.");
        }

        private void Client_SendFlashlightEnergyToServer(int _taschenlampeEnergie)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Inventar), (ushort)NetworkMessages.FlashLightEnergyToServer);
            writer.Write(_taschenlampeEnergie);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.FlashLightEnergyToServer)]
        void Server_ReceiveFlashlightBattery(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            int energy = reader.ReadInt32();

            if (!reader.Complete())
                return;

            FlashlightEnergy = energy;
        }

        private void Client_SendFlashlightOwnedToServer(bool _taschenlampeBesitz)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Inventar), (ushort)NetworkMessages.FlashLightOwnedToServer);
            writer.Write(_taschenlampeBesitz);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.FlashLightOwnedToServer)]
        void Server_ReceiveFlashlightOwned(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool status = reader.ReadBoolean();

            if (!reader.Complete())
                return;

            FlashlightOwned = status;
        }

        private void Client_SendFlashlightVisibleToServer(bool _taschenlampevisible)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Inventar), (ushort)NetworkMessages.FlashLightVisibleToServer);
            writer.Write(_taschenlampevisible);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.FlashLightVisibleToServer)]
        void Server_ReceiveFlashlightVisible(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool status = reader.ReadBoolean();

            if (!reader.Complete())
                return;

            FlashlightVisible = status;
        }
    }
}
