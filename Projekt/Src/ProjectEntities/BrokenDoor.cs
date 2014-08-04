using Engine.EntitySystem;
using Engine.MathEx;
using Engine.PhysicsSystem;
using Engine.SoundSystem;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class BrokenDoorType : RepairableType
    {
        [FieldSerialize]
        Vec3 openDoorBodyOffset = new Vec3(0, 0, 1);
        [FieldSerialize]
        Vec3 openDoor2BodyOffset = new Vec3(0, 0, 1);

        [FieldSerialize]
        [DefaultValue(1.0f)]
        float openTime = 1.0f;

        [FieldSerialize]
        string soundOpen;
        [FieldSerialize]
        string soundClose;

        //

        /// <summary>
        /// Gets or sets the displacement a position of a body "door" when the door is open.
        /// </summary>
        [Description("The displacement a position of a body \"door\" when the door is open.")]
        [DefaultValue(typeof(Vec3), "0 0 1")]
        public Vec3 OpenDoorBodyOffset
        {
            get { return openDoorBodyOffset; }
            set { openDoorBodyOffset = value; }
        }

        /// <summary>
        /// Gets or sets the displacement a position of a body "door2" when the door is open.
        /// </summary>
        [Description("The displacement a position of a body \"door2\" when the door is open.")]
        [DefaultValue(typeof(Vec3), "0 0 1")]
        public Vec3 OpenDoor2BodyOffset
        {
            get { return openDoor2BodyOffset; }
            set { openDoor2BodyOffset = value; }
        }

        /// <summary>
        /// Gets or set the time of opening/closing of a door.
        /// </summary>
        [Description("The time of opening/closing of a door.")]
        [DefaultValue(1.0f)]
        public float OpenTime
        {
            get { return openTime; }
            set { openTime = value; }
        }

        /// <summary>
        /// Gets or sets the sound at opening a door.
        /// </summary>
        [Description("The sound at opening a door.")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundOpen
        {
            get { return soundOpen; }
            set { soundOpen = value; }
        }

        /// <summary>
        /// Gets or sets the sound at closing a door.
        /// </summary>
        [Description("The sound at closing a door.")]
        [Editor(typeof(EditorSoundUITypeEditor), typeof(UITypeEditor))]
        [SupportRelativePath]
        public string SoundClose
        {
            get { return soundClose; }
            set { soundClose = value; }
        }

        protected override void OnPreloadResources()
        {
            base.OnPreloadResources();

            PreloadSound(SoundOpen, SoundMode.Mode3D);
            PreloadSound(SoundClose, SoundMode.Mode3D);
        }
    }

    public class BrokenDoor : Repairable
    {

        BrokenDoorType _type = null; public new BrokenDoorType Type { get { return _type; } }

        enum NetworkMessages
        {
            OpenSettingsToClient,
            SoundOpenToClient,
            SoundCloseToClient,
        }


        [FieldSerialize]
        bool needOpen;
        [FieldSerialize]
        float openDoorOffsetCoefficient;

        private Vec3 doorBody1InitPosition;
        private Vec3 doorBody2InitPosition;
        private Body doorBody1;
        private Body doorBody2;

        public override bool Repaired
        {
            get
            {
                return base.Repaired;
            }
            set
            {
                base.Repaired = value;

                if (needOpen == value || !value)
                    return;

                needOpen = value;

                if (EntitySystemWorld.Instance.IsEditor())
                {
                    openDoorOffsetCoefficient = 1;
                    UpdateDoorBodies();
                }
                else
                {
                    if (needOpen)
                    {
                        SoundPlay3D(Type.SoundOpen, .5f, false);

                        //send message to client
                        if (EntitySystemWorld.Instance.IsServer())
                        {
                            if (Type.NetworkType == EntityNetworkTypes.Synchronized)
                                Server_SendSoundOpenToAllClients();
                        }
                    }
                    else
                    {
                        SoundPlay3D(Type.SoundClose, .5f, false);

                        //send message to client
                        if (EntitySystemWorld.Instance.IsServer())
                        {
                            if (Type.NetworkType == EntityNetworkTypes.Synchronized)
                                Server_SendSoundCloseToAllClients();
                        }
                    }
                }

            }
        }

        protected override void OnPostCreate(bool loaded)
        {
            base.OnPostCreate(loaded);
            SubscribeToTickEvent();

            if (PhysicsModel != null)
            {
                for (int n = 0; n < PhysicsModel.Bodies.Length; n++)
                {
                    if (PhysicsModel.Bodies[n].Name == "door" || PhysicsModel.Bodies[n].Name == "door1")
                    {
                        Mat4 transform = PhysicsModel.ModelDeclaration.Bodies[n].GetTransform();
                        doorBody1InitPosition = transform.Item3.ToVec3();
                    }
                    else if (PhysicsModel.Bodies[n].Name == "door2")
                    {
                        Mat4 transform = PhysicsModel.ModelDeclaration.Bodies[n].GetTransform();
                        doorBody2InitPosition = transform.Item3.ToVec3();
                    }
                }

                doorBody1 = PhysicsModel.GetBody("door1");
                if (doorBody1 == null)
                    doorBody1 = PhysicsModel.GetBody("door");
                doorBody2 = PhysicsModel.GetBody("door2");
            }

            UpdateDoorBodies();
        }

        private void UpdateDoorBodies()
        {
            if (doorBody1 != null)
            {
                Vec3 pos = Position +
                    (doorBody1InitPosition + Type.OpenDoorBodyOffset * openDoorOffsetCoefficient) * Rotation;
                Vec3 oldPosition = doorBody1.Position;
                doorBody1.Position = pos;
                doorBody1.OldPosition = oldPosition;
            }
            if (doorBody2 != null)
            {
                Vec3 pos = Position +
                    (doorBody2InitPosition + Type.OpenDoor2BodyOffset * openDoorOffsetCoefficient) * Rotation;
                Vec3 oldPosition = doorBody2.Position;
                doorBody2.Position = pos;
                doorBody2.OldPosition = oldPosition;
            }

            //send event to clients in networking mode
            if (EntitySystemWorld.Instance.IsServer() &&
                Type.NetworkType == EntityNetworkTypes.Synchronized)
            {
                Server_SendBodiesPositionsToAllClients(false);
            }
        }

        protected override void OnTick()
        {
            base.OnTick();

            if (needOpen || openDoorOffsetCoefficient != 0)
            {
                float offset = TickDelta / Type.OpenTime;

                float oldOpenDoorOffsetCoefficient = openDoorOffsetCoefficient;

                if (needOpen)
                {
                    openDoorOffsetCoefficient += offset;
                    if (openDoorOffsetCoefficient >= 1)
                    {
                        openDoorOffsetCoefficient = 1;                        
                    }
                    
                }
                else
                {
                    openDoorOffsetCoefficient -= offset;
                    if (openDoorOffsetCoefficient <= 0)
                    {
                        openDoorOffsetCoefficient = 0;                        
                    }
                }

                if ( oldOpenDoorOffsetCoefficient != openDoorOffsetCoefficient)
                {
                    if (EntitySystemWorld.Instance.IsServer() && Type.NetworkType == EntityNetworkTypes.Synchronized)
                        Server_SendOpenSettingsToClients(EntitySystemWorld.Instance.RemoteEntityWorlds);
                }
            }

            UpdateDoorBodies();
        }


        void Server_SendOpenSettingsToClients(IList<RemoteEntityWorld> remoteEntityWorlds)
        {
            SendDataWriter writer = BeginNetworkMessage(remoteEntityWorlds, typeof(BrokenDoor),
                (ushort)NetworkMessages.OpenSettingsToClient);
            
            writer.Write(openDoorOffsetCoefficient);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.OpenSettingsToClient)]
        void Client_ReceiveOpenSettings(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            float value2 = reader.ReadSingle();
            if (!reader.Complete())
                return;
            openDoorOffsetCoefficient = value2;
        }

        void Server_SendSoundOpenToAllClients()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(BrokenDoor),
                (ushort)NetworkMessages.SoundOpenToClient);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.SoundOpenToClient)]
        void Client_ReceiveSoundOpen(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            SoundPlay3D(Type.SoundOpen, .5f, false);
        }

        void Server_SendSoundCloseToAllClients()
        {
            SendDataWriter writer = BeginNetworkMessage(typeof(BrokenDoor),
                (ushort)NetworkMessages.SoundCloseToClient);
            EndNetworkMessage();
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.SoundCloseToClient)]
        void Client_ReceiveSoundClose(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            if (!reader.Complete())
                return;
            SoundPlay3D(Type.SoundClose, .5f, false);
        }
    }
}
