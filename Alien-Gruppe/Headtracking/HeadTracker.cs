using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

using System.Runtime.InteropServices;

using Engine.MathEx;

namespace Game {

class HeadTracker
{
    #region Private Members
    private volatile bool _shouldStop = false;
    private bool _isRunning = false;
    private Thread _thread = null;
    private int _port = 2424;

    UdpClient _listener;
    IPEndPoint _endPoint;
    byte[] receiveByteArray;

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
                TrackingEvent(sensorID, posX, posY, posZ);
            }
        }
    }

    #endregion

    public HeadTracker(int port)
    {
        _port = port;
    }

    public bool start()
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

    public delegate void receiveTrackingData(int sensorID, double x, double y, double z);

    public event receiveTrackingData TrackingEvent;
}

}