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
using Engine.SoundSystem;

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


        private UInt32 lastRotate = 0;

        //Variable ob der Ring drehbar sein soll
        [FieldSerialize]
        private bool rotatable = true;

        //Anzahl der Ecken des Ringes, standard 8. Nur gebraucht für drehen, ist egal bei festen Ringen
        [FieldSerialize]
        private byte corners = 8;

        //Aktuelle Position des Ringes
        private byte ringPosition;

        [FieldSerialize]
        private int initialPosition = 0;







        //***************************
        //*******Getter-Setter*******
        //*************************** 
        public int InitialPosition
        {
            get { return initialPosition; }
            set { initialPosition = value; }
        }

        public bool Rotatable
        {
            get { return rotatable; }
            set { rotatable = value; }
        }

        public byte Corners
        {
            get { return corners; }
            set { corners = value; }
        }

        public byte RingPosition
        {
            get { return ringPosition; }
        }
        //*************************** 

        //******************************
        //*******Delegates/Events*******
        //****************************** 
        public delegate void RotateRingDelegate(Vec3 pos, Quat rot, bool left);


        [LogicSystemBrowsable(true)]
        public event RotateRingDelegate RotateRing;
        //*****************************


        //Rotiert "links" herum
        [LogicSystemBrowsable(false)]
        public void RotateLeft(bool force = false)
        {
           if (!CanRotate() && !force)
               return;

           lastRotate = (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + 10;

           Sound sound = SoundWorld.Instance.SoundCreate("Sounds\\rotationSound.ogg", 0);
           SoundWorld.Instance.SoundPlay(sound, EngineApp.Instance.DefaultSoundChannelGroup,
                   0.5f);

           ringPosition = mod(ringPosition - 1, corners);

           double angle = ringPosition * (Math.PI / corners);

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
        [LogicSystemBrowsable(false)]
        public void RotateRight(bool force = false)
        {
            if (!CanRotate() && !force)
                return;

            lastRotate = (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds + 10;

            Sound sound = SoundWorld.Instance.SoundCreate("Sounds\\rotationSound.ogg", 0);
            SoundWorld.Instance.SoundPlay(sound, EngineApp.Instance.DefaultSoundChannelGroup,
                    0.5f);

            ringPosition = mod(ringPosition + 1, corners);

            double angle = ringPosition * (Math.PI / corners);

            Quat rot = new Quat(0, 0, (float)Math.Sin(angle), (float)Math.Cos(angle));

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

        public bool CanRotate()
        {
            return rotatable && ( (UInt32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds > lastRotate );
        }

        private byte mod(int x, int m)
        {
            return (byte)((x % m + m) % m);
        }
    }
}
