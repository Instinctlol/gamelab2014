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
using Engine;


	public class TuioDump : TuioListener
	{
        //members
        private const UInt32 MouseEventLeftDown = 0x0002;
        private const UInt32 MouseEventLeftUp = 0x0004;

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
			Console.WriteLine("add cur "+tcur.getCursorID() + " ("+tcur.getSessionID()+") "+tcur.getX()+" "+tcur.getY());
            if (tcur.getCursorID().Equals(0))
            {
                Cursor.Position = new Point((int)(tcur.getX() * 1024), (int)(tcur.getY() * 768));
                //mouse_event((UInt32)MouseActionAdresses.ABSOLUTE, (uint)(tcur.getX() * 1024), 0, 0, new System.IntPtr());
                mouse_event((UInt32)MouseActionAdresses.LEFTDOWN, 0, 0, 0, new System.IntPtr());
                Console.WriteLine("Klick");
                //EngineApp.Instance.MousePosition = new Engine.MathEx.Vec2(500,500);
                //EngineApp.Instance.DoMouseMove();
            }
            else if (tcur.getCursorID().Equals(1)) {
                mouse_event((UInt32)MouseActionAdresses.LEFTUP, 0, 0, 0, new System.IntPtr());
                Cursor.Position = new Point((int)(tcur.getX() * 1024), (int)(tcur.getY() * 768));
                mouse_event((UInt32)MouseActionAdresses.RIGHTDOWN, 0, 0, 0, new System.IntPtr());
            }
            //LeftClick((int)(tcur.getX() * 1024), (int)(tcur.getY() * 1024));
		}

		public void updateTuioCursor(TuioCursor tcur) {
            Cursor.Position = new Point((int)(tcur.getX() * 1024), (int)(tcur.getY() * 768));
			Console.WriteLine("set cur "+tcur.getCursorID() + " ("+tcur.getSessionID()+") "+tcur.getX()+" "+tcur.getY()+" "+tcur.getMotionSpeed()+" "+tcur.getMotionAccel());
		}

		public void removeTuioCursor(TuioCursor tcur) {
			Console.WriteLine("del cur "+tcur.getCursorID() + " ("+tcur.getSessionID()+")");
            if (tcur.getCursorID().Equals(0))
            {
                mouse_event((UInt32)MouseActionAdresses.LEFTUP, 0, 0, 0, new System.IntPtr());
            }
            else if (tcur.getCursorID().Equals(1))
            {
                mouse_event((UInt32)MouseActionAdresses.RIGHTUP, 0, 0, 0, new System.IntPtr());
            }
		}

		public void refresh(TuioTime frameTime) {
			//Console.WriteLine("refresh "+frameTime.getTotalMilliseconds());
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
