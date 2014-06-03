/*
	TUIO C# Example - part of the reacTIVision project
	http://reactivision.sourceforge.net/

	Copyright (c) 2005-2009 Martin Kaltenbrunner <mkalten@iua.upf.edu>

	This program is free software; you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation; either version 2 of the License, or
	(at your option) any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program; if not, write to the Free Software
	Foundation, Intcur., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
*/

using System;
using TUIO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using Engine;
using System.Collections.Generic;


	public class TuioDump : TuioListener
	{
        //members
        private const UInt32 MouseEventLeftDown = 0x0002;
        private const UInt32 MouseEventLeftUp = 0x0004;
        public static Mutex mutexLock = new Mutex();
        public static List<float[]> dataPoints = new List<float[]>();
        public enum MouseActionAdresses
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }

        //[DllImport("user32.dll")]
        //static extern void mouse_event(int dwFlags, int dx, int dy,
        //                       int dwData, int dwExtraInfo);

        //public void LeftClick(int x, int y)
        //{
        //    Cursor.Position = new System.Drawing.Point(x, y);
        //    mouse_event((int)(MouseActionAdresses.LEFTDOWN), 0, 0, 0, 0);
        //    mouse_event((int)(MouseActionAdresses.LEFTUP), 0, 0, 0, 0);
        //}

        //user32 API import
        [DllImport("user32.dll")]
        private static extern void mouse_event(UInt32 dwFlags, UInt32 dx, UInt32 dy, UInt32 dwData, IntPtr dwExtraInfo);


		public void addTuioObject(TuioObject tobj) {
			Console.WriteLine("add obj "+tobj.getSymbolID()+" "+tobj.getSessionID()+" "+tobj.getX()+" "+tobj.getY()+" "+tobj.getAngle());
		}

		public void updateTuioObject(TuioObject tobj) {
			Console.WriteLine("set obj "+tobj.getSymbolID()+" "+tobj.getSessionID()+" "+tobj.getX()+" "+tobj.getY()+" "+tobj.getAngle()+" "+tobj.getMotionSpeed()+" "+tobj.getRotationSpeed()+" "+tobj.getMotionAccel()+" "+tobj.getRotationAccel());
		}

		public void removeTuioObject(TuioObject tobj) {
			Console.WriteLine("del obj "+tobj.getSymbolID()+" "+tobj.getSessionID());
		}

		public void addTuioCursor(TuioCursor tcur) {
            //OnAddCursor
            writeData(tcur.getCursorID(), tcur.getSessionID(), 1, tcur.getX(), tcur.getY(), 0, 0);
			Console.WriteLine("add cur "+tcur.getCursorID() + " ("+tcur.getSessionID()+") "+tcur.getX()+" "+tcur.getY());
		}

		public void updateTuioCursor(TuioCursor tcur) {
            //OnUpdateCursor
            writeData(tcur.getCursorID(), tcur.getSessionID(), 2, tcur.getX(), tcur.getY(), tcur.getMotionSpeed(), tcur.getMotionAccel());
			Console.WriteLine("set cur "+tcur.getCursorID() + " ("+tcur.getSessionID()+") "+tcur.getX()+" "+tcur.getY()+" "+tcur.getMotionSpeed()+" "+tcur.getMotionAccel());

		}

		public void removeTuioCursor(TuioCursor tcur) {
            //OnRemoveCursor
            writeData(tcur.getCursorID(), tcur.getSessionID(), 3,0, 0, 0, 0);
			Console.WriteLine("del cur "+tcur.getCursorID() + " ("+tcur.getSessionID()+")");
		}

		public void refresh(TuioTime frameTime) {
            //OnRefresh
			//Console.WriteLine("refresh "+frameTime.getTotalMilliseconds());
		}

        public void writeData(float id, float sid, float insttype, float xcord, float ycord, float speed, float accel) {
            mutexLock.WaitOne();

            float[] newpoint = new float[] { id, sid, DateTime.Now.Millisecond, insttype, xcord, ycord, speed, accel };

            dataPoints.Add(newpoint);

            mutexLock.ReleaseMutex();
        }

        public static int[] getData() {
            mutexLock.WaitOne();

            mutexLock.ReleaseMutex();
            return new int[0];
        }

		static public void runTuio() {
			TuioDump demo = new TuioDump();
			TuioClient client = null;

					client = new TuioClient();

				client.addTuioListener(demo);
				client.connect();
				Console.WriteLine("listening to TUIO messages at port " + client.getPort());

		}
	}
