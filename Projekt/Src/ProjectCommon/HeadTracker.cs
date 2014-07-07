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
            Console.WriteLine("----calculatepos()-----");
            Vec4D calculatedPos = Mat4D.Multiply(m, v);
            Console.WriteLine("M: " + m.ToString());
            Console.WriteLine("V: " + v.ToString());
            Vec3D pos = new Vec3D();
            if (calculatedPos.W != 1)
            {
                // Normalisieren
                pos.X = calculatedPos.X / calculatedPos.W;
                pos.Y = calculatedPos.Y / calculatedPos.W;
                pos.Z = calculatedPos.Z / calculatedPos.W;
            }
            Console.WriteLine("Berechnete Positionen: "+pos.ToString()+"\n-----");
            
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