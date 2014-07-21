using Engine.EntitySystem;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class ServerRackType : DynamicType
    {
        [FieldSerialize]
        private int secondsBetweenUse = 0;

        [FieldSerialize]
        private string soundOnUse = "";

        [FieldSerialize]
        private string requiredToUse;

        public string RequiredToUse
        {
            get { return requiredToUse; }
            set { requiredToUse = value; }
        }

        [Description("The sound when the object gets used.")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundOnUse
        {
            get { return soundOnUse; }
            set { soundOnUse = value; }
        }

        public int SecondsBetweenUse
        {
            get { return secondsBetweenUse; }
            set { secondsBetweenUse = value; }
        }


    }

    public class ServerRack : Dynamic
    {
        ServerRackType _type = null; public new ServerRackType Type { get { return _type; } }

        enum NetworkMessages
        {
            PressToServer,
            LastUsedToClient,
        }

        private UInt32 lastUse;

        public UInt32 LastUse
        {
            get { return lastUse; }
            set { 
                
                lastUse = value;

                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendLastUse();
            }
        }


        public void Press(Unit unit)
        {
            if(CanUse() && hasItem(unit))
            {
                SoundPlay3D(Type.SoundOnUse, .5f, false);
                Client_SendPress();
            }
            else if(!hasItem(unit))
                StatusMessageHandler.sendMessage("You should try to get " + Type.RequiredToUse + " instead of inserting your finger");
        }


        public bool CanUse()
        {
            UInt32 now = (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            return now - lastUse - Type.SecondsBetweenUse >= 0;
        }

        public bool hasItem(Unit unit)
        {
            return true;
        }

        private void Client_SendPress()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(ServerRack),
                (ushort)NetworkMessages.PressToServer);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.PressToServer)]
        private void Server_ReceivePress(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;

            lastUse = (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        private void Server_SendLastUse()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(ServerRack),
                (ushort)NetworkMessages.LastUsedToClient);

            writer.Write(lastUse);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.LastUsedToClient)]
        private void Client_ReceiveLastUsed(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            UInt32 lastUsed = reader.ReadUInt32();

            if (!reader.Complete())
                return;

            lastUse = lastUsed;
        }
    }
}
