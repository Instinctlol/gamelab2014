using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Utils;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class StaticObjectType : MapObjectType
    {

    }

    public class StaticObject : MapObject
    {
        StaticObjectType _type = null; public new StaticObjectType Type { get { return _type; } }

        Client_Snapshot lastSnapshot = null;
        bool lightStatus = true;

        enum NetworkMessages
        {
            TranformationToClient,
            LightStatusToClient,
        }

        public bool LightStatus
        {
            get { return lightStatus; }
            set
            {
                lightStatus = value;
                SetLights(lightStatus);

                if (EntitySystemWorld.Instance.IsServer())
                    Server_SendLightStatusToAllClients(lightStatus);
            }
        }

        public void SetLights(bool status)
        {
            foreach (var v in AttachedObjects)
            {
                MapObjectAttachedLight light = v as MapObjectAttachedLight;
                if (light != null)
                    light.Visible = status;


                MapObjectAttachedMesh attachedMesh = v as MapObjectAttachedMesh;
                if (attachedMesh != null)
                {
                    foreach (Engine.Renderer.MeshObject.SubObject s in attachedMesh.MeshObject.SubObjects)
                    {

                        if (status == false)
                        {
                            s.SetCustomGpuParameter((int)ShaderBaseMaterial.GpuParameters.emissionMapTransformMul, new Vec4(0, 0, 0, 0));
                        }
                        if (status == true)
                        {
                            s.RemoveCustomGpuParameter((int)ShaderBaseMaterial.GpuParameters.emissionMapTransformMul);
                        }
                    }
                }
            }
        }

     protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);

            if (EntitySystemWorld.Instance.IsServer())
            {
                Server_SendPositionToAllClients(Position, Rotation, Scale);
                Server_SendLightStatusToAllClients(lightStatus);
            }

        }



        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);
            if (EntitySystemWorld.Instance.IsServer())
            {
                Server_SendPositionToAllClients(Position, Rotation, Scale);
                Server_SendLightStatusToAllClients(lightStatus);
            }
        }


        protected override void OnSetTransform(ref Vec3 pos, ref Quat rot, ref Vec3 scl)
        {
            base.OnSetTransform(ref pos, ref rot, ref scl);

            if (EntitySystemWorld.Instance.IsServer())
                Server_SendPositionToAllClients(pos, rot, scl);
        }


        void Server_SendLightStatusToAllClients(bool status)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(StaticObject),
                (ushort)NetworkMessages.LightStatusToClient);
            writer.Write(status);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.LightStatusToClient)]
        void Client_ReceiveLightStatus(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool status = reader.ReadBoolean();
            if (!reader.Complete())
                return;

            LightStatus = status;
        }

        void Server_SendPositionToAllClients(Vec3 pos, Quat rot, Vec3 scl)
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(StaticObject),
                (ushort)NetworkMessages.TranformationToClient);
            writer.Write(pos);
            writer.Write(rot);
            writer.Write(scl);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.TranformationToClient)]
        void Client_ReceivePosition(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            Vec3 pos = reader.ReadVec3();
            Quat rot = reader.ReadQuat();
            Vec3 scl = reader.ReadVec3();
            if (!reader.Complete())
                return;

            Client_Snapshot snapshot = new Client_Snapshot();
            snapshot.position = pos;
            snapshot.rotation = rot;
            snapshot.scale = scl;


            if(IsPostCreated)
                Client_UpdateTransformationWithSnapshot(snapshot);
        }

        private void Client_UpdateTransformationWithSnapshot(Client_Snapshot snapshot)
        {
            if (snapshot != null)
            {
                SetTransform(snapshot.position, snapshot.rotation, snapshot.scale);
                lastSnapshot = snapshot;
            }
        }


        class Client_Snapshot
        {
            public Vec3 position;
            public Quat rotation;
            public Vec3 scale;
        }
    }
}
