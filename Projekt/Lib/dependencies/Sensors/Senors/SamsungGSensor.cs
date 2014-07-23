using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.WindowsCE.Forms;

namespace Sensors
{
    class SamsungGSensor : IGSensor
    {
        const int OrientationChangedMessage = 50000;
        class GSensorWindow : MessageWindow
        {
            public GSensorWindow(SamsungGSensor sensor)
            {
                mySensor = sensor;
            }
            public SamsungGSensor mySensor;
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == OrientationChangedMessage)
                {
                    if (mySensor.OrientationChanged != null)
                        mySensor.OrientationChanged(mySensor);
                }
                base.WndProc(ref m);
            }
        }

        enum ECreationDisposition : uint
        {
            New = 1,
            CreateAlways = 2,
            OpenExisting = 3,
            OpenAlways = 4,
            TruncateExisting = 5
        }


        Thread myThread;
        ScreenOrientation myOrientation;
        GSensorWindow myWindow;
        public SamsungGSensor()
        {
            DeviceIoControl(ACCOnRot, new int[1], new int[1]);
            myWindow = new GSensorWindow(this);
            myOrientation = GetGVector().ToScreenOrientation();
            myThread = new Thread(GSensorThread);
            myThread.Start();
        }

        void GSensorThread()
        {
            ScreenOrientation lastOrientation = myOrientation;
            int difCount = 0;
            while (true)
            {
                ScreenOrientation newOrientation = GetGVector().ToScreenOrientation();
                if (newOrientation != lastOrientation)
                    difCount = 0;
                lastOrientation = newOrientation;
                if (newOrientation != myOrientation)
                    difCount++;
                if (difCount > 2)
                {
                    myOrientation = lastOrientation = newOrientation;
                    difCount = 0;
                    Message msg = new Message();
                    msg.Msg = OrientationChangedMessage;
                    msg.HWnd = myWindow.Hwnd;
                    MessageWindow.SendMessage(ref msg);
                }
                Thread.Sleep(1000);
            }
        }

        static void DeviceIoControl(int controlCode, int[] inBuffer, int[] outBuffer)
        {
            IntPtr file = CreateFile("ACS1:", 0, 0, IntPtr.Zero, ECreationDisposition.OpenExisting, 0, IntPtr.Zero);
            if (file == (IntPtr)(-1))
                throw new InvalidOperationException("Unable to Create File");

            try
            {
                int bytesReturned = 0;
                int inSize = sizeof(int) * inBuffer.Length;
                int outSize = sizeof(int) * outBuffer.Length;
                if (!DeviceIoControl(file, controlCode, inBuffer, inSize, outBuffer, outSize, ref bytesReturned, IntPtr.Zero))
                    throw new InvalidOperationException("Unable to perform operation.");
            }
            finally
            {
                CloseHandle(file);
            }
        }

        const int ACCOnRot = 0x44E;
        const int ACCOffRot = 0x44F;
        const int ACCReadValues = 0x3F7;

        [DllImport("coredll")]
        extern static bool CloseHandle(IntPtr handle);

        [DllImport("coredll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern bool DeviceIoControl(IntPtr hDevice, int dwIoControlCode, [In] int[] inBuffer, int nInBufferSize, [Out] int[] outBuffer, int nOutBufferSize, ref int pBytesReturned, IntPtr lpOverlapped);

        [DllImport("coredll")]
        extern static IntPtr CreateFile(string filename, int desiredAccess, int shareMode, IntPtr securityAttributes, ECreationDisposition creationDisposition, int flags, IntPtr template);

        #region IGSensor Members

        public GVector GetGVector()
        {
            int[] outBuffer = new int[3];
            DeviceIoControl(ACCReadValues, new int[1], outBuffer);

            GVector ret = new GVector();
            ret.X = outBuffer[1];
            ret.Y = outBuffer[0];
            ret.Z = -outBuffer[2];
            double samsungScaleFactor = 1.0 / 1000.0 * 9.8 * 3.3793103448275862068965517241379;
            return ret.Scale(samsungScaleFactor);
        }

        public ScreenOrientation Orientation
        {
            get { return myOrientation; }
        }

        public event OrientationChangedHandler OrientationChanged;

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            DeviceIoControl(ACCOffRot, new int[1], new int[1]);
            myThread.Abort();
        }

        #endregion
    }
}