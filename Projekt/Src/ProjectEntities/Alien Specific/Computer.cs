using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectEntities
{
    public class ComputerType : DynamicType
    {
    }

    /// <summary>
    /// Main computer that can be controlled by the boss alien and
    /// the astronauts
    /// </summary>
    class Computer : Dynamic
    {
        ComputerType _type = null; public new ComputerType Type { get { return _type; } }

        /// <summary>
        /// Show the state of the station, e.g. how many life the astronauts 
        /// still have, how the rings are positioned...
        /// </summary>
        public void ShowState()
        {

        }

        /// <summary>
        /// Do one rotation of one ring to the left or to the right side.
        /// </summary>
        /// <param name="ringNumber"></param>
        /// <param name="left"></param>
        public void RotateRing(int ringNumber, bool left)
        {

        }

        /// <summary>
        /// Switches off or on the power of one sector (room)
        /// </summary>
        /// <param name="on"></param>
        public void SetSectorPower(bool on)
        {

        }
    }
}
