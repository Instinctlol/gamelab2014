using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class RoomType : MapObjectType
    { }

    public class Room : MapObject
    {
        RoomType _type = null; public new RoomType Type { get { return _type; } }


        enum NetworkMessages
        {
            PositionToClient,//used for object which have not a physics model
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (EntitySystemWorld.Instance.IsServer())
                Server_SendPositionToAllClients(Position, Rotation, Scale);

        }


        protected override void OnSetTransform(ref Vec3 pos, ref Quat rot, ref Vec3 scl)
        {
            base.OnSetTransform(ref pos, ref rot, ref scl);

            if (EntitySystemWorld.Instance.IsServer())
                Server_SendPositionToAllClients(pos, rot, scl);
        }


        void Server_SendPositionToAllClients(Vec3 pos, Quat rot, Vec3 scl)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(Room),
                (ushort)NetworkMessages.PositionToClient);
            writer.Write(pos);
            writer.Write(rot);
            writer.Write(scl);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.PositionToClient)]
        void Client_ReceivePosition(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            Vec3 pos = reader.ReadVec3();
            Quat rot = reader.ReadQuat();
            Vec3 scl = reader.ReadVec3();
            if (!reader.Complete())
                return;

            Client_Snapshot snapshot = new Client_Snapshot();

            SetTransform(pos, rot, scl);
            SetOldTransform(pos, rot, scl);
        }


        class Client_Snapshot
        {
            public Vec3 position;
            public Quat rotation;
            public Vec3 scale;
        }
    }
}
