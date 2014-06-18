using Engine;
using Engine.EntitySystem;
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

       
        [Description( "The sound when the object got repaired." )]
		[Editor( typeof( EditorSoundUITypeEditor ), typeof( UITypeEditor ) )]
		[SupportRelativePath]
        public string SoundRepaired
        {
            get { return soundRepaired; }
            set { soundRepaired = value; }
        }


        [Description( "The sound when the object is getting repaired." )]
		[Editor( typeof( EditorSoundUITypeEditor ), typeof( UITypeEditor ) )]
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


        //TODO: Network, updating physical object
        public bool Repaired
        {
            get { return repaired; }
            set
            {
                if (this.repaired == value)
                    return;

                this.repaired = value;

                if (EntitySystemWorld.Instance.IsClientOnly())
                    SoundPlay3D(Type.SoundRepaired, .5f, false);
                else
                {
                    OnRepair();
                    Server_SendRepairedToAllClients();
                }

            }
        }

        public virtual void Press()
        {

            //Wenn nur client ist
            if(EntitySystemWorld.Instance.IsClientOnly())
            {
                SoundPlay3D(Type.SoundUsing, .5f, false);
                Client_SendPressToServer();
            }
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
