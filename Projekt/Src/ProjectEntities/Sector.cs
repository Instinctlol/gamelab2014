using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
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

        private List<MapObjectAttachedLight> attachedLights = new List<MapObjectAttachedLight>();

        //Liste der Lichter wird automatisch generiert
        private List<Light> lights = new List<Light>();

        //Aktueller status der Lichter
        //[FieldSerialize]
        private bool lightStatus = true;

        private bool isHidden = false;

        private int aliensInSector = 0;

        //Lister aller Dynamischen Objekte wird automatisch generiert
        private List<Dynamic> dynamics = new List<Dynamic>();

        //Liste aller statischen Objekte wird automatisch generiert
        private List<MapObject> statics = new List<MapObject>();

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
                if(lightStatus != value)
                {
                    lightStatus = value;
                    SetLights(lightStatus);
                }
                
                
 }
        }

        public bool IsHidden
        {
            get { return isHidden; }
            set
            {

                if (isHidden != value)
                {
                    isHidden = value;

                    if(isHidden)
                    {
                        SetFoWEnabled(true);
                    }
                    else
                    {
                        SetFoWEnabled(false);
                    }
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

        
        //DEPRECATED!!!
        //Setzt die Lichter zu bestimmten status
        public void SwitchLights(bool status)
        {
            LightStatus = status;
        }


        //Registriert wenn ein Objekt den Sektor betritt
        protected override void OnObjectIn(MapObject obj)
        {
            base.OnObjectIn(obj);

            if (obj is Sector || obj is Ring)
                return;


            if (obj is AlienUnit)
                OnAlienIn((AlienUnit)obj);
            else if (obj is OutDoor && ring != null)
                ring.RotateRing += ((OutDoor)obj).OnRotate;

            if (obj is Light)
                OnLightIn((Light)obj);
            else if (obj is Dynamic)
                OnDynamicIn((Dynamic)obj);
            else
                OnStaticIn(obj);
        }

        //Registriert wenn ein Object den Sektor verlässt
        protected override void OnObjectOut(MapObject obj)
        {
            base.OnObjectOut(obj);

            if (obj is AlienUnit)
                OnAlienOut((AlienUnit)obj);

            if (obj is Sector || obj is Ring)
                return;

            if(obj is Light)
                OnLightOut((Light)obj);
            else if (obj is Dynamic)
                OnDynamicOut((Dynamic)obj);

        }

        //Initialisieren bestimmter Teile wenn Sektor erstellt wird
        protected override void OnPostCreate(bool loaded)
        {

            base.OnPostCreate(loaded);
        }

        protected override void OnRender(Engine.Renderer.Camera camera)
        {
            base.OnRender(camera);

            if (loaded)
                return;

            //Wenn Ring vorhanden bei seinem Event unterschreiben
            if (ring != null)
                ring.RotateRing += OnRotateRing;

            if (group != null)
                group.SwitchLight += SwitchLights;

            if (aliensInSector <= 0  && GameMap.Instance.IsAlien)
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
                    SetLights(false);
                }
                else
                {
                    SetLights(lightStatus);
                }

                status = !status;
                foreach (Dynamic d in dynamics)
                    d.Visible = status;

            }
        }

        private void SetLights(bool status)
        {

                foreach (Light l in lights)
                {
                    l.Visible = status;
                }
                foreach (MapObjectAttachedLight l in attachedLights)
                {
                    l.Visible = status;
                }
        }


        //Wenn Ring rotiert alles im Sektor rotieren
        private void OnRotateRing(Vec3 pos, Quat rot)
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

            foreach (MapObject m in statics)
            {
                m.Rotation = newRot * m.Rotation;
                offset = m.Position - OldPosition;
                m.Position = newRot * offset + Position;
            }
        }

        private void OnStaticIn(MapObject obj)
        {
            statics.Add(obj);

            foreach (MapObjectAttachedObject o in obj.AttachedObjects)
            {
                if (o is MapObjectAttachedLight)
                    AddAttachedLight((MapObjectAttachedLight)o);
            }
        }

        private void OnDynamicIn(Dynamic obj)
        {
            dynamics.Add(obj);

            if (isHidden)
                obj.Visible = false;
            else
                obj.Visible = true;

        }

        private void OnDynamicOut(Dynamic obj)
        {
            dynamics.Remove(obj);
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

        private void OnAlienIn(AlienUnit obj)
        {
            if (GameMap.Instance.IsAlien)
            {
                aliensInSector++;
                IsHidden = false;
            }
        }

        private void OnAlienOut(AlienUnit obj)
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


        private void AddAttachedLight(MapObjectAttachedLight l)
        {
            attachedLights.Add(l);
            if (isHidden)
                l.Visible = false;
            else
                l.Visible = true;
        }
    }
}
