using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.Utils;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class DetonationObjectType : DynamicType
    {
        [FieldSerialize]
        UInt32 secondsToUse = 10;

        [DefaultValue(10)]
        public UInt32 SecondsToUse
        {
            get { return secondsToUse; }
            set { secondsToUse = value; }
        }

        [FieldSerialize]
        string detonationItem;

        public string DetonationItem
        {
            get { return detonationItem; }
            set { detonationItem = value; }
        }

        [FieldSerialize]
        string soundUseStart;

        [FieldSerialize]
        string soundUseDuring;

        [FieldSerialize]
        string soundUserEnd;

    }

    public class DetonationObject : Dynamic
    {
        DetonationObjectType _type = null; public new DetonationObjectType Type { get { return _type; } }

        enum NetworkMessages
        {
            StartUseToServer,
            EndUseToServer,
            UseableToClient,
        }

        public delegate void PreparedDelegate();

        [LogicSystemBrowsable(true)]
        public event PreparedDelegate Prepared;


        private UInt32 useStart;

        private UInt32 UseStart
        {
            get { return useStart; }
            set { useStart = value; }
        }

        bool useable = true;

        public bool Useable
        {
            get { return useable; }
            set
            {
                useable = value;
                if(!useable)
                    foreach (MapObjectAttachedObject obj in AttachedObjects)
                    {
                        MapObjectAttachedMesh mesh = obj as MapObjectAttachedMesh;
                        if(mesh != null && mesh.Alias=="dynamesh")
                        {
                            mesh.Visible = true;
                        }
                    }
                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendUsable(useable);
            }
        }


        public void StartUse(Unit unit)
        {
            if (useable && HasItem(unit))
            {
                if (EntitySystemWorld.Instance.IsClientOnly())
                    Client_SendStartUse();
            }
            else if (!HasItem(unit))
                StatusMessageHandler.sendMessage("You dont have what it takes to make this go boom");
        }

        public void EndUse(Unit unit)
        {
            if (useable && HasItem(unit))
            {
                if (EntitySystemWorld.Instance.IsClientOnly())
                    Client_SendEndUse();
            }
        }

        public bool HasItem(Unit unit)
        {
            string useItem = "";
            if(unit.Inventar.useItem != null )
             useItem = unit.Inventar.useItem.Type.FullName.ToLower();
            return !String.IsNullOrEmpty(Type.DetonationItem) && useItem.Equals(Type.DetonationItem.ToLower());
        }

        private void Client_SendStartUse()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(DetonationObject),
                (ushort)NetworkMessages.StartUseToServer);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.StartUseToServer)]
        private void Server_ReceiveStartUse(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;

            UseStart = (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private void Client_SendEndUse()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(DetonationObject),
               (ushort)NetworkMessages.EndUseToServer);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.EndUseToServer)]
        private void Server_ReceiveEndUse(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;

            UInt32 now = (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            if ( (int)(now - useStart - Type.SecondsToUse) >= 0)
            {
                Useable = false;

                if (Prepared != null)
                    Prepared();
            }
        }

        private void Server_SendUsable(bool useable)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(DetonationObject),
                   (ushort)NetworkMessages.UseableToClient);

            writer.Write(useable);

            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.UseableToClient)]
        private void Client_ReceiveUsuable(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool useable = reader.ReadBoolean();

            if (!reader.Complete())
                return;

            Useable = useable;
        }
    }
}
