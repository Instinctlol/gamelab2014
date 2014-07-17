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
    public class MedicCabinetType : DynamicType
    {
        [FieldSerialize]
        float hpRestore;

        [FieldSerialize]
        string soundUse;

        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundUse
        {
            get { return soundUse; }
            set { soundUse = value; }
        }

        public float HpRestore
        {
            get { return hpRestore; }
            set { hpRestore = value; }
        }
    }

    public class MedicCabinet : Dynamic
    {
        MedicCabinetType _type = null; public new MedicCabinetType Type { get { return _type; } }

        enum NetworkMessages
        {
            PressToServer,
        }

        public void Press(Unit unit)
        {
            Client_SendPress(unit);
            SoundPlay3D(Type.SoundUse,.5f, false);
        }

        private void Client_SendPress(Unit unit)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(MedicCabinet),
                   (ushort)NetworkMessages.PressToServer);
            writer.Write(unit.NetworkUIN);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToServer, (ushort)NetworkMessages.PressToServer)]
        private void Server_ReceivePress(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            uint uin = reader.ReadUInt32();

            if (!reader.Complete())
                return;

            Unit unit = Entities.Instance.GetByNetworkUIN(uin) as Unit;
            if(unit != null)
                unit.Health += Type.HpRestore;
        }
    }
}
