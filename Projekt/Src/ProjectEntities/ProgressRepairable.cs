using Engine.EntitySystem;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ProjectEntities
{

    public class ProgressRepairableType : RepairableType
    {
        [FieldSerialize]
        private int progressPerPress = 1;

        [FieldSerialize]
        private int progressRequired = 10;


        [Description("Progress that gets added each time when used.")]
        public int ProgressPerPress
        {
            get { return progressPerPress; }
            set { progressPerPress = value; }
        }

        [Description("Progress at which object is repaired.")]
        public int ProgressRequired
        {
            get { return progressRequired; }
            set { progressRequired = value; }
        }


    }

    public class ProgressRepairable : Repairable
    {
        ProgressRepairableType _type = null; public new ProgressRepairableType Type { get { return _type; } }
        private int progress = 0;

        enum NetworkMessages
        {
            PressToServer,
            ProgressToClient,
        }

        public int Progress
        {
            get { return progress; }
            set { 
                progress = value;
                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendProgressToAllClients();
            }
        }

        public override void Press()
        {
            if (EntitySystemWorld.Instance.IsClientOnly())
            {
                SoundPlay3D(Type.SoundUsing, .5f, false);
                Client_SendPressToServer();
            }            
        }

        void Client_SendPressToServer()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(ProgressRepairable),
                (ushort)NetworkMessages.PressToServer);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.PressToServer)]
        void Server_ReceivePress(RemoteEntityWorld sender, Engine.Utils.ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            Progress += Type.ProgressPerPress;

            if (progress >= Type.ProgressRequired)
                Repaired = true;
        }

        void Server_SendProgressToAllClients()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(ProgressRepairable),
                (ushort)NetworkMessages.ProgressToClient);

            writer.Write(progress);

            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.ProgressToClient)]
        void Client_ReceiveProgress(RemoteEntityWorld sender, Engine.Utils.ReceiveDataReader reader)
        {
            int prog = reader.ReadInt32();

            if (!reader.Complete())
                return;

            Progress = prog;
        }
        

    }
}
