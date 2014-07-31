using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.MathEx;

namespace ProjectEntities
{
    /// <summary>
    /// Klasse für das Alien-Radar
    /// </summary>
    public class Signal
    {
        Vec2 pos;

        public Signal(Vec2 pos)
        {
            this.pos = pos;
        }

        public Vec2 Pos 
        {
            get { return pos; }
            set { pos = value; }
        }
    }
}
