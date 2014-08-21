using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Utils;
using ProjectCommon;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;
namespace ProjectEntities
{
    public class SectorType : RegionType
    { }

    /*
    * Klasse für Sektoren. Versorgt sich mit den meisten Daten von selbst
    */
    public class Sector : Region
    {
        SectorType _type = null; public new SectorType Type { get { return _type; } }

        private bool isRotating = false;

        private bool loaded = false;

        //Aktueller status der Lichter
        //[FieldSerialize]
        private bool lightStatus = true;

        private bool isHidden = false;

        private int aliensInSector = 0;

        //Lister aller Dynamischen Objekte wird automatisch generiert
        private List<Dynamic> dynamics = new List<Dynamic>();

        //Liste aller Raum Objekte wird automatisch generiert
        private List<Room> rooms = new List<Room>();

        //Liste aller statischen Objecte. TBD
        private List<StaticObject> statics = new List<StaticObject>();

        private List<OutDoor> doors = new List<OutDoor>();

        //Ring zu dem dieser Sektore gehört
        [FieldSerialize]
        private Ring ring;

        //Sektor Gruppe dem dieser Sektor angehört
        [FieldSerialize]
        private SectorGroup group;


        enum NetworkMessages
        {
            LightToClient,
        }

        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public Ring Ring
        {
            get { return ring; }
            set
            {
                ring = value;
            }
        }

        public SectorGroup Group
        {
            get { return group; }
            set { group = value; }
        }

        public bool LightStatus
        {
            get { return lightStatus; }
            set
            {
                lightStatus = value;


                /*if (group != null && group.LightStatus != value)
                {
                   // if()
                        group.LightStatus = value;
                }
                else*/
                SetLights(lightStatus);
            }

        }

        //Sets the room to be hidden for the alien
        public bool IsHidden
        {
            get { return isHidden; }
            set
            {

                if (isHidden != value)
                {
                    isHidden = value;

                    SetFoWEnabled(isHidden);
                }
            }
        }
        //***************************


        public Sector()
            : base()
        {
            base.ShapeType = ShapeTypes.Box;
            base.Filter = Filters.All;
            base.CheckType = CheckTypes.Center;
        }



        //Setzt die Lichter zu bestimmten status
        public void SwitchLights(bool status)
        {
            LightStatus = status;
        }

        protected override void Server_OnClientConnectedAfterPostCreate(RemoteEntityWorld remoteEntityWorld)
        {
            base.Server_OnClientConnectedAfterPostCreate(remoteEntityWorld);
            if (group != null)
                SwitchLights(group.LightStatus);
        }

        public void ObjectIn(MapObject obj)
        {
            if (obj is Sector || obj is Ring)
                return;

            if (obj is PlayerCharacter)
            {
                Server_SendLightToClient(lightStatus, ((PlayerCharacter)obj).Owner);
            }

            if (obj is AlienUnit && !(obj is AlienSpawner))
                OnAlienIn();

            if (obj is Dynamic)
                OnDynamicIn((Dynamic)obj);
            else if (obj is Room)
                OnRoomIn((Room)obj);
            else if (obj is StaticObject)
                OnStaticIn((StaticObject)obj);
        }

        public void ObjectOut(MapObject obj)
        {
            if (obj is Sector || obj is Ring)
                return;


            if (obj is Dynamic)
                OnDynamicOut((Dynamic)obj);

            if (obj is AlienUnit && !(obj is AlienSpawner))
                OnAlienOut();
            else if (obj is Room)
                OnRoomOut((Room)obj);
            else if (obj is StaticObject)
                OnStaticOut((StaticObject)obj);
        }

        //Registriert wenn ein Objekt den Sektor betritt
        protected override void OnObjectIn(MapObject obj)
        {
            if (isRotating)
                return;

            Vec3 source = obj.Position;
            source.Z = 100;
            Vec3 direction = new Vec3(0, 0, -1000);
            Ray ray = new Ray(source, direction);

            //Find first sector and throw him in
            Map.Instance.GetObjects(ray, delegate(MapObject mObj, float scale)
            {
                Sector sec = mObj as Sector;

                if (sec != null && sec != this)
                {
                    sec.ObjectOut(obj);
                }

                return true;
            });

            base.OnObjectIn(obj);

            if (obj is Sector || obj is Ring)
                return;

            if (obj is PlayerCharacter)
            {
                Server_SendLightToClient(lightStatus, ((PlayerCharacter)obj).Owner);
            }

            if (obj is AlienUnit && !(obj is AlienSpawner))
                OnAlienIn();

            if (obj is Dynamic)
                OnDynamicIn((Dynamic)obj);
            else if (obj is Room)
                OnRoomIn((Room)obj);
            else if (obj is StaticObject)
                OnStaticIn((StaticObject)obj);
        }

        //Registriert wenn ein Object den Sektor verlässt
        protected override void OnObjectOut(MapObject obj)
        {
            if (isRotating)
                return;

            Vec3 source = obj.Position;
            source.Z = 100;
            Vec3 direction = new Vec3(0, 0, -1000);
            Ray ray = new Ray(source, direction);

            //Find first sector and throw him in
            Map.Instance.GetObjects(ray, delegate(MapObject mObj, float scale)
            {
                Sector sec = mObj as Sector;

                if (sec != null && sec != this)
                {
                    sec.ObjectIn(obj);
                    return false;
                }

                return true;
            });

            base.OnObjectOut(obj);

            if (obj is Sector || obj is Ring)
                return;


            if (obj is Dynamic)
                OnDynamicOut((Dynamic)obj);

            if (obj is AlienUnit && !(obj is AlienSpawner))
                OnAlienOut();
            else if (obj is Room)
                OnRoomOut((Room)obj);
            else if (obj is StaticObject)
                OnStaticOut((StaticObject)obj);

        }

        //Initialisieren bestimmter Teile wenn Sektor erstellt wird
        protected override void OnPostCreate(bool loaded)
        {

            base.OnPostCreate(loaded);


            //Wenn Ring vorhanden bei seinem Event unterschreiben
            if (ring != null)
                ring.RotateRing += OnRotateRing;

            if (group != null)
            {
                group.SwitchLight += SwitchLights;
                SwitchLights(group.LightStatus);
            }
        }

        protected override void OnRender(Engine.Renderer.Camera camera)
        {
            base.OnRender(camera);

            if (loaded)
                return;

            if (aliensInSector <= 0 && EntitySystemWorld.Instance.IsServer())
            {
                aliensInSector = 0;
                IsHidden = true;
            }

            loaded = true;
        }


        private void SetFoWEnabled(bool status)
        {
            if (EntitySystemWorld.Instance.IsServer())
            {

                if (status)
                {
                    SetLights(false, false);
                }
                else
                {
                    SetLights(lightStatus, false);
                }

                status = !status;
                foreach (Dynamic d in dynamics)
                    if(!(d is AlienSpawner) && !(d is Repairable) && !(d is ServerRack) && !(d is DetonationObject))
                        d.Visible = status;

            }
        }

        private void SetLights(bool status, bool sync = true)
        {
            foreach (Room r in rooms)
            {
                if (sync)
                    r.LightStatus = status;
                else
                    r.SetLights(status);
            }

            foreach (StaticObject s in statics)
            {
                if (sync)
                    s.LightStatus = status;
                else
                    s.SetLights(status);
            }

            if (sync)
                foreach (Dynamic d in dynamics)
                {
                    PlayerCharacter unit = d as PlayerCharacter;
                    if (unit != null)
                        Server_SendLightToClient(status, unit.Owner);
                }
        }


        //Wenn Ring rotiert alles im Sektor rotieren
        private void OnRotateRing(Vec3 pos, Quat rot, bool left)
        {
            isRotating = true;
            Rotation = rot * Rotation;
            Vec3 offset = Position - pos;
            Position = rot * offset + pos;

            Quat newRot = Rotation * OldRotation.GetInverse();
            newRot.Normalize();

            foreach (MapObject m in doors)
            {
                offset = m.Position - OldPosition;

                m.SetTransform(newRot * offset + Position, newRot * m.Rotation, m.Scale);
            }

            foreach (MapObject m in dynamics)
            {
                offset = m.Position - OldPosition;

                m.SetTransform(newRot * offset + Position, newRot * m.Rotation, m.Scale);
            }

            foreach (MapObject m in rooms)
            {
                offset = m.Position - OldPosition;

                m.SetTransform(newRot * offset + Position, newRot * m.Rotation, m.Scale);
            }

            foreach (MapObject m in statics)
            {
                offset = m.Position - OldPosition;

                m.SetTransform(newRot * offset + Position, newRot * m.Rotation, m.Scale);
            }

            foreach (OutDoor d in doors)
                d.CheckForPartner();

            isRotating = false;
        }


        private void OnRoomIn(Room obj)
        {
            if (rooms.Contains(obj))
                return;

            rooms.Add(obj);

            obj.LightStatus = lightStatus;

            if (isHidden)
                obj.SetLights(false);

        }

        private void OnRoomOut(Room obj)
        {
            rooms.Remove(obj);
        }

        private void OnStaticIn(StaticObject obj)
        {
            if (statics.Contains(obj))
                return;

            statics.Add(obj);

            obj.LightStatus = lightStatus;

            if (isHidden)
                obj.SetLights(false);

        }

        private void OnStaticOut(StaticObject obj)
        {
            statics.Remove(obj);
        }

        private void OnDynamicIn(Dynamic obj)
        {
            if (obj is OutDoor || dynamics.Contains(obj))
                return;

            dynamics.Add(obj);

            if ((obj is AlienSpawner) || (obj is Repairable) || (obj is ServerRack) || (obj is DetonationObject))
                return;

            Vec3 source = obj.Position;
            source.Z = 100;
            Vec3 direction = new Vec3(0, 0, -1000);
            Ray ray = new Ray(source, direction);

            bool visible = !IsHidden;

            

            Map.Instance.GetObjects(ray, delegate(MapObject mObj, float scale)
            {
                Sector sec = mObj as Sector;

                if (sec != null && sec != this)
                {
                    visible = visible || !sec.IsHidden;
                }

                return true;
            });



            obj.Visible = visible;


        }

        private void OnDynamicOut(Dynamic obj)
        {
            if (obj is OutDoor)
                return;

            dynamics.Remove(obj);
        }

        private void OnAlienIn()
        {
            if (EntitySystemWorld.Instance.IsServer())
            {
                aliensInSector++;
                IsHidden = false;
            }
        }

        private void OnAlienOut()
        {
            if (EntitySystemWorld.Instance.IsServer())
            {
                aliensInSector--;
                if (aliensInSector <= 0)
                {
                    aliensInSector = 0;
                    IsHidden = true;
                }
            }
        }

        private void Server_SendLightToClient(bool status, RemoteEntityWorld target)
        {
            if (target != null)
            {
                SendDataWriter writer = BeginNetworkMessage(target, typeof(Sector), (ushort)NetworkMessages.LightToClient);
                writer.Write(status);
                EndNetworkMessage();
            }
        }

        [NetworkReceive(NetworkDirections.ToClient, (ushort)NetworkMessages.LightToClient)]
        private void Client_ReceiveLightToClient(RemoteEntityWorld sender, ReceiveDataReader reader)
        {
            bool status = reader.ReadBoolean();

            if (!reader.Complete())
                return;

            if (status)
                Map.Instance.AmbientLight = new ColorValue(150f / 255f, 150f / 255f, 150f / 255f);
            else
                Map.Instance.AmbientLight = new ColorValue(15f / 255f, 15f / 255f, 15f / 255f);
        }

        internal void AddDoor(OutDoor outDoor)
        {
            doors.Add(outDoor);
        }

        internal void RemoveDoor(OutDoor outDoor)
        {
            doors.Remove(outDoor);
        }
    }
}
