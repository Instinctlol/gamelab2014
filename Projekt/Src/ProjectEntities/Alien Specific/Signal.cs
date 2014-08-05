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
        Vec2 min;
        Vec2 max;

        public Signal(Vec2 min, Vec2 max)
        {
            this.min = min;
            this.max = max;
        }

        public Vec2 Min
        {
            get { return min; }
            set { min = value; }
        }

        public Vec2 Max
        {
            get { return max; }
            set { max = value; }
        }

        public override bool Equals(Object obj){
            
            Signal other = obj as Signal;
            if (other != null){
                return min == other.min && max == other.max;
            }
            return false;
        }
    }
}
