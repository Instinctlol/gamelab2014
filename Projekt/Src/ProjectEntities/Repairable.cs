using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.Renderer;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;

namespace ProjectEntities
{
    public class RepairableType : DynamicType
    {
        [FieldSerialize]
        private string soundRepaired;

        [FieldSerialize]
        private string soundUsing;

        [FieldSerialize]
        List<RepairItem> repairItems = new List<RepairItem>();

        public class RepairItem
        {
            [FieldSerialize]
            ItemType itemType;

            public ItemType ItemType
            {
                get { return itemType; }
                set { itemType = value; }
            }

            public override string ToString()
            {
                if (itemType == null)
                    return "(not initialized)";
                return itemType.Name;
            }
        }

        public List<RepairItem> RepairItems
        {
            get { return repairItems; }
        }

        [FieldSerialize]
        private string destroyedTexture = "";

        [Editor(typeof(EditorMaterialUITypeEditor), typeof(UITypeEditor))]
        public string DestroyedTexture
        {
            get { return destroyedTexture; }
            set { destroyedTexture = value; }
        }


        [Description("The sound when the object got repaired.")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundRepaired
        {
            get { return soundRepaired; }
            set { soundRepaired = value; }
        }


        [Description("The sound when the object is getting repaired.")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundUsing
        {
            get { return soundUsing; }
            set { soundUsing = value; }
        }

    }

    public class Repairable : Dynamic
    {
        RepairableType _type = null; public new RepairableType Type { get { return _type; } }

        [FieldSerialize]
        private bool repaired = false;

        private MeshObject mesh;
        private string originalTexture;
        private string destroyedTexture;

        //random high number
        private int itemsRequired = 500;

        public int ItemsRequired
        {
            get { return itemsRequired; }
            set
            {

                itemsRequired = value;
                if (itemsRequired <= 0 && EntitySystemWorld.Instance.IsServer())
                    Repaired = true;
            }
        }

        enum NetworkMessages
        {
            PressToServer,
            RepairedToClient,
            DecreaseItemRequiredToServer,
            ItemsRequiredToClient,
        }


        public delegate void RepairDelegate(Repairable entity);

        [LogicSystemBrowsable(true)]
        public event RepairDelegate Repair;



        protected virtual void OnRepair()
        {
            if (Repair != null)
                Repair(this);
        }


        public virtual bool Repaired
        {
            get { return repaired; }
            set
            {
                if (this.repaired == value)
                    return;

                this.repaired = value;

                foreach (var item in AttachedObjects)
                {
                    MapObjectAttachedMesh mesh = item as MapObjectAttachedMesh;
                    if (mesh != null)
                        mesh.Collision = false;
                }

                if (!String.IsNullOrEmpty(destroyedTexture))
                {
                    if (repaired)
                        mesh.SetMaterialNameForAllSubObjects(originalTexture);
                    else
                        mesh.SetMaterialNameForAllSubObjects(destroyedTexture);
                }

                if (EntitySystemWorld.Instance.IsClientOnly())
                    SoundPlay3D(Type.SoundRepaired, 1f, false);
                else
                {
                    if (!EntitySystemWorld.Instance.IsEditor())
                        Server_SendRepairedToAllClients();
                    OnRepair();
                }

            }
        }

        public virtual void Press(Unit unit)
        {

            if (!CanRepair(unit))
            {
                return;
            }

            Client_SendPressToServer();
        }

        protected bool CanRepair(Unit unit)
        {
            string useItem = "";
            bool wrongItem = true;

            if (unit.Inventar.useItem != null)
                useItem = unit.Inventar.useItem.Type.FullName;

            if (itemsRequired <= 0)
                return true;

            foreach (var item in Type.RepairItems)
            {
                if (item.ItemType.FullName == useItem)
                {
                    unit.Inventar.remove(unit.Inventar.useItem);
                    SoundPlay3D(Type.SoundUsing, .5f, false);
                    Client_SendDecreaseItemsRequired();
                    wrongItem = false;
                    break;
                }
            }

            if (wrongItem)
                StatusMessageHandler.sendMessage("Falscher Gegenstand");

            if (itemsRequired <= 0)
                return true;

            return false;
        }




        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            foreach (var v in AttachedObjects)
            {
                MapObjectAttachedMesh attachedMesh = v as MapObjectAttachedMesh;
                if (attachedMesh != null)
                {
                    mesh = attachedMesh.MeshObject;
                    originalTexture = mesh.SubObjects[0].MaterialName;
                    break;
                }
            }

            destroyedTexture = null;
            if (!String.IsNullOrEmpty(Type.DestroyedTexture))
                destroyedTexture = Type.DestroyedTexture;

            if (mesh != null && !String.IsNullOrEmpty(destroyedTexture))
                mesh.SetMaterialNameForAllSubObjects(destroyedTexture);

            if (EntitySystemWorld.Instance.IsServer())
                itemsRequired = Type.RepairItems.Count;
        }


        private void Client_SendDecreaseItemsRequired()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Repairable),
                (ushort)NetworkMessages.DecreaseItemRequiredToServer);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.DecreaseItemRequiredToServer)]
        void Server_ReceveiveDecreaseItemsRequired(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            itemsRequired--;



            Server_SendItemsRequiredToClients(itemsRequired);
        }


        private void Server_SendItemsRequiredToClients(int itemsRequired)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Repairable),
                (ushort)NetworkMessages.ItemsRequiredToClient);
            writer.WriteVariableInt32(itemsRequired);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.ItemsRequiredToClient)]
        void Client_ReceveiveItemsRequired(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            int value = reader.ReadVariableInt32();

            if (!reader.Complete())
                return;
            itemsRequired = value;
        }

        void Client_SendPressToServer()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Repairable),
                (ushort)NetworkMessages.PressToServer);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.PressToServer)]
        void Server_ReceivePress(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            Repaired = true;
        }

        void Server_SendRepairedToAllClients()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Repairable),
                (ushort)NetworkMessages.RepairedToClient);
            writer.Write(Repaired);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.RepairedToClient)]
        void Client_ReceiveRepaired(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool rep = reader.ReadBoolean();
            if (!reader.Complete())
                return;
            Repaired = rep;
        }

    }
}
