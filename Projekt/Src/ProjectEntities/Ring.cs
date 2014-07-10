using Engine;
using Engine.EntitySystem;
using Engine.MapSystem;
using Engine.MathEx;
using Engine.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Text;
using ProjectCommon;

namespace ProjectEntities
{

    public class RingType : RegionType
    {}

    /*
     * Klasse für Ringe. Unterstützt drehen nach links/rechts
     */
    public class Ring : Region
    {
        RingType _type = null; public new RingType Type { get { return _type; } }


        //Variable ob der Ring drehbar sein soll
        [FieldSerialize]
        private bool rotatable = true;

        //Deprecated
        [FieldSerialize]
        private int id = -1;

        //Anzahl der Ecken des Ringes, standard 8. Nur gebraucht für drehen, ist egal bei festen Ringen
        [FieldSerialize]
        private byte corners = 8;

        //Aktuelle Position des Ringes
        private byte position;


        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public bool Rotatable
        {
            get { return rotatable; }
            set { rotatable = value; }
        }

        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        public byte Corners
        {
            get { return corners; }
            set { corners = value; }
        }

        public byte RingPosition
        {
            get { return position; }
            set { position = value; }
        }
        //*************************** 

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void RotateRingDelegate(Vec3 pos, Quat rot, bool left);


        [LogicSystemBrowsable(true)]
        public event RotateRingDelegate RotateRing;
        //*****************************



        public Ring() : base()
        {
            base.ShapeType = ShapeTypes.Box;
            base.Filter = Filters.All;
        }


        //Rotiert "links" herum
        [LogicSystemBrowsable(true)]
        public void RotateLeft()
        {

            EngineConsole.Instance.Print("ring links drehen");
            //Vllt Error wenn man versucht statischen ring zu drehen
            if (!Rotatable)
                return;

            position =(byte)( position + corners - 1);

            double angle = position * (Math.PI / corners);

            Quat rot = new Quat(0, 0, (float)Math.Sin(angle), (float)Math.Cos(angle));

            Quat newRot = rot * Rotation.GetInverse();
            newRot.Normalize();

            Rotation = rot;

            if (RotateRing != null)
            {
                RotateRing(this.Position, newRot, true);
            }        
        }

        //Rotiert "rechts" herum
        [LogicSystemBrowsable(true)]
        public void RotateRight()
        {

            EngineConsole.Instance.Print("ring rechts drehen");
            //Vllt Error wenn man versucht statischen ring zu drehen
            if (!Rotatable)
                return;

            position = (byte)( (position + 1) % corners );

            double angle = position * (Math.PI/corners);

            Quat rot = new Quat(0,0,(float) Math.Sin( angle ), (float) Math.Cos( angle ));

            Quat newRot = rot * Rotation.GetInverse();
            newRot.Normalize();

            Rotation = rot;

            if (RotateRing != null)
            {
                RotateRing(this.Position, newRot, false);
            }

        }

        /// <summary>
        /// Liefert die Ring-Nummer anhand des Ring-Namens: 1 für F1_Ring, 2 für F2_Ring...
        /// </summary>
        /// <returns></returns>
        public int GetRingNumber()
        {
            int n = Name.ToCharArray()[1];
            return (n-48);
        }
    }
}
