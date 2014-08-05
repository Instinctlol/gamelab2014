using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Utils;
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

        private bool loaded = false;

        //Liste der Lichter wird automatisch generiert
        private List<Light> lights = new List<Light>();

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

        //Ring zu dem dieser Sektore gehört
        [FieldSerialize]
        private Ring ring;

        //Sektor Gruppe dem dieser Sektor angehört
        [FieldSerialize]
        private SectorGroup group;


        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public Ring Ring
        {
            get { return ring; }
            set {
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
            set { 
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


        public Sector() : base()
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


        //Registriert wenn ein Objekt den Sektor betritt
        protected override void OnObjectIn(MapObject obj)
        {
            base.OnObjectIn(obj);

            if (obj is Sector || obj is Ring)
                return;


            if (obj is AlienUnit)
                OnAlienIn();
            else if (obj is OutDoor && ring != null)
                ring.RotateRing += ((OutDoor)obj).OnRotate;

            if (obj is Light)
                OnLightIn((Light)obj);
            else if (obj is Dynamic)
                OnDynamicIn((Dynamic)obj);
            else if (obj is Room)
                OnRoomIn((Room)obj);
            else if (obj is StaticObject)
                OnStaticIn((StaticObject) obj);


        }

        //Registriert wenn ein Object den Sektor verlässt
        protected override void OnObjectOut(MapObject obj)
        {
            base.OnObjectOut(obj);

            if (obj is Sector || obj is Ring)
                return;

            if(obj is Light)
                OnLightOut((Light)obj);
            else if (obj is Dynamic)
                OnDynamicOut((Dynamic)obj);

            if (obj is AlienUnit)
                OnAlienOut();

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

            if (aliensInSector <= 0 && (GameMap.Instance != null && GameMap.Instance.IsAlien))
            {
                aliensInSector = 0;
                IsHidden = true;
            }

            loaded = true;

        }


        private void SetFoWEnabled(bool status)
        {
            if( GameMap.Instance.IsAlien )
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
                    d.Visible = status;

            }
        }

        private void SetLights(bool status, bool sync = true)
        {

                foreach (Light l in lights)
                {
                    l.Visible = status;
                }
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
        }


        //Wenn Ring rotiert alles im Sektor rotieren
        private void OnRotateRing(Vec3 pos, Quat rot, bool left)
        {

            Rotation = rot * Rotation;
            Vec3 offset = Position - pos;
            Position = rot * offset + pos;

            Quat newRot = Rotation * OldRotation.GetInverse();
            newRot.Normalize();

            foreach (MapObject m in lights)
            {
                m.Rotation = newRot * m.Rotation;
                offset = m.Position - OldPosition;
                m.Position = newRot * offset + Position;
            }

            foreach (MapObject m in dynamics)
            {
                m.Rotation = newRot * m.Rotation;
                offset = m.Position - OldPosition;
                m.Position = newRot * offset + Position;
            }

            foreach (MapObject m in rooms)
            {
                m.Rotation = newRot * m.Rotation;
                offset = m.Position - OldPosition;
                m.Position = newRot * offset + Position;
            }

            foreach(MapObject m in statics)
            {
                m.Rotation = newRot * m.Rotation;
                offset = m.Position - OldPosition;
                m.Position = newRot * offset + Position;
            }

        }


        private void OnRoomIn(Room obj)
        {
            rooms.Add(obj);

            obj.LightStatus = lightStatus;

            if (isHidden)
                obj.SetLights(false);
                
        }

        private void OnStaticIn(StaticObject obj)
        {
            statics.Add(obj);

            obj.LightStatus = lightStatus;

            if (isHidden)
                obj.SetLights(false);
                
        }

        private void OnDynamicIn(Dynamic obj)
        {
            dynamics.Add(obj);

            obj.Visible = !IsHidden;
        }

        private void OnDynamicOut(Dynamic obj)
        {
            dynamics.Remove(obj);

            Vec3 source = obj.Position;
            source.Z = 100;
            Vec3 direction = new Vec3(0, 0, -1000);
            Ray ray = new Ray(source, direction);


            Map.Instance.GetObjects(ray, delegate(MapObject mObj, float scale)
            {
                Sector sec = mObj as Sector;

                if (sec != null)
                {
                    obj.Visible = !sec.IsHidden;
                    return false;
                }

                return true;
            });
        }

        private void OnLightIn(Light obj)
        {
            lights.Add(obj);

            if (isHidden)
                obj.Visible = false;
            else
                obj.Visible = true;

        }

        private void OnLightOut(Light obj)
        {
            lights.Remove(obj);
        }

        private void OnAlienIn()
        {
            if (GameMap.Instance.IsAlien)
            {
                aliensInSector++;
                IsHidden = false;
            }
        }

        private void OnAlienOut()
        {
            if (GameMap.Instance.IsAlien)
            {
                aliensInSector--;
                if (aliensInSector <= 0)
                {
                    aliensInSector = 0;
                    IsHidden = true;
                }
            }
        }
    }
}
