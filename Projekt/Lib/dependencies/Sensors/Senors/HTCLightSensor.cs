using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsMobile;
using Microsoft.WindowsMobile.Status;
using System.Threading;
using Microsoft.WindowsCE.Forms;

namespace Sensors
{
    public struct HTCLightSensorData
    {
        public int Reserved0; // this value is always 3
        public byte Luminance; // this value ranges between 0 and 30
    }

    public class HTCLightSensor : HTCSensorBase, ILightSensor, IDisposable
    {
        #region HTCSensorSDK
        // The followign PInvoke is the results of reverse engineering done by Koushik Dutta
        // at www.koushikdutta.com
        [DllImport("HTCSensorSDK")]
        extern static IntPtr HTCSensorGetDataOutput(IntPtr handle, out HTCLightSensorData sensorData);
        #endregion

        public HTCLightSensor()
        {
            myHandle = HTCSensorOpen(HTCSensor.Light);

            for (int i = 0; i < myBrightnessSamples.Length; i++)
                myBrightnessSamples[i] = 0;

            myBrightnessUpdateThread = new Thread(new ThreadStart(BrightnessThread));
            myBrightnessUpdateThread.Start();
            myWindow = new LightSensorWindow(this);
        }

        LightSensorWindow myWindow;

        IntPtr myHandle;
        public HTCLightSensorData GetRawSensorData()
        {
            HTCLightSensorData data;
            HTCSensorGetDataOutput(myHandle, out data);
            return data;
        }

        void BrightnessThread()
        {
            while (true)
            {
                myBrightnessSamples[myCurrentSample++] = GetLumens();
                myCurrentSample %= myBrightnessSamples.Length;
                Brightness currentBrightness = CalculateBrightness();
                if (currentBrightness != myBrightness)
                {
                    myBrightness = currentBrightness;
                    Message msg = new Message();
                    msg.Msg = BrightnessChangedMessage;
                    msg.HWnd = myWindow.Hwnd;
                    MessageWindow.SendMessage(ref msg);
                }
                Thread.Sleep(1000);
            }
        }

        const int BrightnessChangedMessage = 50000;
        class LightSensorWindow : Microsoft.WindowsCE.Forms.MessageWindow
        {
            HTCLightSensor mySensor;
            public LightSensorWindow(HTCLightSensor sensor)
            {
                mySensor = sensor;
            }

            protected override void WndProc(ref Microsoft.WindowsCE.Forms.Message m)
            {
                if (m.Msg == BrightnessChangedMessage)
                {
                    if (mySensor.myBrightnessChanged != null)
                        mySensor.myBrightnessChanged(mySensor);
                }
                base.WndProc(ref m);
            }
        }

        int myCurrentSample = 0;
        double[] myBrightnessSamples = new double[5];
        Brightness myBrightness = Brightness.Dark;
        Thread myBrightnessUpdateThread = null;

        Brightness CalculateBrightness()
        {
            double total = 0;
            for (int i = 0; i < myBrightnessSamples.Length; i++)
            {
                total += myBrightnessSamples[i];
            }
            total /= myBrightnessSamples.Length;
            if (total < 20)
                return Brightness.Dark;
            if (total < 80)
                return Brightness.Dim;
            if (total < 300)
                return Brightness.Normal;
            return Brightness.Bright;
        }

        #region ILightSensor Members

        /// <summary>
        /// This method will return the luminance of the surrounding environment in view of the sensor.
        /// The return value will be in candela/m^2 (aka nit).
        /// </summary>
        /// <returns></returns>
        public double GetLumens()
        {
            HTCLightSensorData data = GetRawSensorData();
            // Not really sure a good way to calibrate this.
            // Did it by holding it up to my 700 lumen lightbulb, and it returns ~208.
            return (double)data.Luminance * ((double)755 / (double)208);
        }

        public Brightness Brightness
        {
            get
            {
                return myBrightness;
            }
        }

        BrightnessChangedHandler myBrightnessChanged;
        public event BrightnessChangedHandler BrightnessChanged
        {
            add
            {
                myBrightnessChanged += value;
            }
            remove
            {
                myBrightnessChanged -= value;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (myBrightnessUpdateThread != null)
            {
                myBrightnessUpdateThread.Abort();
                myBrightnessUpdateThread = null;
            }
            if (myHandle != IntPtr.Zero)
            {
                HTCSensorClose(myHandle);
                myHandle = IntPtr.Zero;
            }
            if (myWindow != null)
            {
                myWindow.Dispose();
                myWindow = null;
            }
        }

        #endregion
    }
}
