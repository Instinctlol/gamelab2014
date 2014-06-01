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

        private List<MapObjectAttachedLight> attachedLights = new List<MapObjectAttachedLight>();

        //Liste der Lichter wird automatisch generiert
        private List<Light> lights = new List<Light>();

        //Aktueller status der Lichter
        [FieldSerialize]
        private bool lightStatus;

        //Lister aller OutDoor Objekte wird automatisch generiert
        private List<OutDoor> outDoors = new List<OutDoor>();

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
            if (lightStatus != status)
                lightStatus = status;

            if (lightStatus)
            {
                foreach (Light l in lights)
                {
                    l.Visible = true;
                }
                foreach (MapObjectAttachedLight l in attachedLights)
                {
                    l.Visible = true;
                }
            }
            else
            {
                foreach (Light l in lights)
                {
                    l.Visible = false;
                }
                foreach (MapObjectAttachedLight l in attachedLights)
                {
                    l.Visible = false;
                }
            }
        }


        //Registriert wenn ein Objekt den Sektor betritt
        protected override void OnObjectIn(MapObject obj)
        {
            base.OnObjectIn(obj);

            if (obj is Sector || obj is Ring)
                return;

            if (obj is OutDoor)
                AddOutDoor((OutDoor)obj);
            else if (obj is Light)
                AddLight((Light)obj);
            else if (obj is Dynamic)
                AddDynamic((Dynamic)obj);
            else
                AddStatic(obj);
        }

        //Registriert wenn ein Object den Sektor verlässt
        protected override void OnObjectOut(MapObject obj)
        {
            base.OnObjectOut(obj);

            if (obj is Sector || obj is Ring)
                return;
            if (obj is Dynamic)
                RemoveDynamic((Dynamic)obj);

        }

        //Initialisieren bestimmter Teile wenn Sektor erstellt wird
        protected override void OnPostCreate(bool loaded)
        {

            base.OnPostCreate(loaded);

            if (!loaded)
                return;

            //Wenn Ring vorhanden bei seinem Event unterschreiben
            if (ring != null)
                ring.RotateRing += OnRotateRing;

            if (group != null)
                group.SwitchLight += SwitchLights;
        }


        //Wenn Ring rotiert alles im Sektor rotieren
        private void OnRotateRing(Vec3 pos, Quat rot)
        {

            Rotation = rot * Rotation;
            Vec3 offset = Position - pos;
            Position = rot * offset + pos;

            Quat newRot = Rotation * OldRotation.GetInverse();
            newRot.Normalize();

            foreach (MapObject m in outDoors)
            {
                m.Rotation = newRot * m.Rotation;
                offset = m.Position - OldPosition;
                m.Position = newRot * offset + Position;
            }

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


        //Private Methoden zum verwalten der Listen
        private void AddStatic(MapObject m)
        {
            statics.Add(m);

            foreach (MapObjectAttachedObject obj in m.AttachedObjects)
            {
                if (obj is MapObjectAttachedLight)
                    AddAttachedLight((MapObjectAttachedLight)obj);
            }
        }

        private void AddLight(Light l)
        {
            lights.Add(l);
        }

        private void AddAttachedLight(MapObjectAttachedLight l)
        {
            attachedLights.Add(l);
        }

        private void AddDynamic(Dynamic d)
        {
            dynamics.Add(d);
        }

        private void RemoveDynamic(Dynamic d)
        {
            dynamics.Remove(d);
        }

        private void AddOutDoor(OutDoor d)
        {
            outDoors.Add(d);
            if (ring != null)
                ring.RotateRing += d.OnRotate;
        }

    }
}
