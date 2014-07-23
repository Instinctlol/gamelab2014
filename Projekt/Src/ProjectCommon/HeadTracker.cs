using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Engine;
using Engine.MathEx;

namespace ProjectCommon
{
    //public class HeadtrackerSpecialEvent : InputEvent
    //{
    //    opType type;
    //    float dx, dy, dz;

    //    public HeadtrackerSpecialEvent(InputDevice device, opType type, float dx, float dy, float dz)
    //        : base(device)
    //    {
    //        this.type = type;
    //        this.dx = dx;
    //        this.dy = dy;
    //        this.dz = dz;
    //    }

    //    public opType getOPType()
    //    {
    //        return this.type;
    //    }
    //    public float getx()
    //    {
    //        return this.dx;
    //    }
    //    public float gety()
    //    {
    //        return this.dy;
    //    }
    //    public float getz()
    //    {
    //        return this.dz;
    //    }
    //}


    public class HeadTracker
    {
        #region Private Members
        private volatile bool _shouldStop = false;
        private bool _isRunning = false;
        private Thread _thread = null;
        private int _port = 2424;

        UdpClient _listener;
        IPEndPoint _endPoint;
        byte[] receiveByteArray;

        private String path = "C:/Users/a_mati01/Desktop/GameLab SVN/Alien-Gruppe/Headtracking/head_tracking_matrix.txt";
        private Mat4D coordinates = new Mat4D();

        private static HeadTracker instance;

        public static HeadTracker Instance
        {
            get 
            { 
                if (instance == null)
                {
                    instance = new HeadTracker(2424);
                }
                return instance; 
            }
        }

        private string extractString(ref BinaryReader reader)
        {
            char c = reader.ReadChar();
            string result = "";
            while (c != '\0')
            {
                result += c;
                c = reader.ReadChar();
            }

            return result;
        }

        private void run()
        {
            this.CreateMatrix();
            const string magicWord = "VALI";
            //const string positionEvent = "POS";
            //const string accelerationEvent = "ACC";
            //const string velocityEvent = "VEL";

            string eventType = "";
            string trackerName = "";
            int sensorID;

            double posX, posY, posZ;

            while (!_shouldStop)
            {
                receiveByteArray = _listener.Receive(ref _endPoint);

                MemoryStream stream = new MemoryStream(receiveByteArray);
                BinaryReader reader = new BinaryReader(stream);

                // Get magic word
                string text = extractString(ref reader);
                if (String.Compare(text, magicWord) != 0)
                {
                    continue;
                }

                // Get event type
                eventType = extractString(ref reader);

                // Get tracker name
                trackerName = extractString(ref reader);

                // Get sensor ID
                sensorID = reader.ReadInt32();

                // Get valeus
                posX = reader.ReadDouble();
                posY = reader.ReadDouble();
                posZ = reader.ReadDouble();

            
                if (TrackingEvent != null)
                {
                    Vec3D pos = this.CalculatePos(new Vec4D(posX, posY, posZ, 1), this.coordinates);
                    TrackingEvent(sensorID, pos.X, pos.Y, pos.Z);
                    //TrackingEvent(sensorID, posX, posY, posZ);
                }
            }
        }

        private Vec3D CalculatePos(Vec4D v, Mat4D m)
        {
            Mat4D m1 = new Mat4D(
                -0.209122,-0.977655,-0.0213996,71.4782,
                0.15086,-0.0538752,0.987086,-290.879,
                -0.966183,0.203193,0.158756,388.412,
                0, 0, 0, 1
                );

            Vec4D tempPos = new Vec4D(100.0 * v.X, 100.0 * v.Y, 100.0 * v.Z, 1.0);

            //Console.WriteLine("----calculatepos()-----");
            //Vec4D calculatedPos = Mat4D.Multiply(m1, v * 100) / 100;
            Vec4D calculatedPos = new Vec4D();

            calculatedPos.X = m1.Item0.X * tempPos.X + m1.Item0.Y * tempPos.Y + m1.Item0.Z * tempPos.Z + m1.Item0.W;
            calculatedPos.Y = m1.Item1.X * tempPos.X + m1.Item1.Y * tempPos.Y + m1.Item1.Z * tempPos.Z + m1.Item1.W;
            calculatedPos.Z = m1.Item2.X * tempPos.X + m1.Item2.Y * tempPos.Y + m1.Item2.Z * tempPos.Z + m1.Item2.W;

            //calculatedPos /= calculatedPos.W;
            calculatedPos.X /= 100.0;
            calculatedPos.Y /= 100.0;
            calculatedPos.Z /= 100.0;
            //Console.WriteLine("M: " + m.ToString());
            //Console.WriteLine("V: " + v.ToString());
            Vec3D pos = new Vec3D();
            //if (calculatedPos.W != 1)
            {
                // Normalisieren
                pos.X = (-1)*calculatedPos.Y;// / calculatedPos.W;
                pos.Y = (-1) * calculatedPos.Z;// / calculatedPos.W;
                pos.Z = (-1) * calculatedPos.X;// / calculatedPos.W;
            }
            //Console.WriteLine(pos.ToString());

            //Vec3D initialPos = new Vec3D(-0.392910123, 0.000637284073, 0.204409659);
            //Vec3D currentPosition = new Vec3D(v.X, v.Y, v.Z) - initialPos;

            return pos;
        }

        #endregion

        private HeadTracker(int port)
        {
            _port = port;
        }

        public bool Start()
        {
            if (_isRunning)
            {
                return false;
            }

            _listener = new UdpClient(_port);
            _endPoint = new IPEndPoint(IPAddress.Any, _port);

            _shouldStop = false;
            _thread = new Thread(run);
            _isRunning = true;

            _thread.Start();
            while (!_thread.IsAlive) ;

            Thread.Sleep(1);

            return true;
        }

        public bool stop()
        {
            if (!_isRunning)
            {
                return false;
            }

            _shouldStop = true;
            _thread.Join();

            return true;
        }

        private void CreateMatrix()
        {
            try
            {
                string[] lines = File.ReadAllLines(this.path, Encoding.UTF8);
                if (lines.Length == 1)
                {
                    string[] coordinates = lines[0].Split(' ');
                    for (int i=0; i< coordinates.Length; i++)
                    {
                        int r = (int)i/4;
                        int c = i % 4;
                        // englisches Format, damit . zu , wird und richtig geparst wird
                        this.coordinates[r, c] = double.Parse(coordinates[i], System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
                    }
                }
            } catch (Exception e)
            { }
        }

        public delegate void receiveTrackingData(int sensorID, double x, double y, double z);

        public event receiveTrackingData TrackingEvent;
    }
}