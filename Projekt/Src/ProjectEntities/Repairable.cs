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
        
        [FieldSerialize]
        private List<RepairableType.RepairItem> repairItems;

        enum NetworkMessages
        {
            PressToServer,
            RepairedToClient,
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
                StatusMessageHandler.sendMessage("Falscher Gegenstand");
                return;
            }

            //Wenn nur client ist
            if (EntitySystemWorld.Instance.IsClientOnly())
            {
                SoundPlay3D(Type.SoundUsing, .5f, false);
                Client_SendPressToServer();                
            }
        }

        protected bool CanRepair(Unit unit)
        {
            string useItem = "";

            if (unit.Inventar.useItem != null)
                useItem = unit.Inventar.useItem.Type.Name;

            if(repairItems.RemoveAll(item => item.ItemType.Name.Equals(useItem)) > 0)
                unit.Inventar.remove(unit.Inventar.useItem);

            if (repairItems.Count == 0)
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
            

            repairItems = new List<RepairableType.RepairItem>(Type.RepairItems);
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
