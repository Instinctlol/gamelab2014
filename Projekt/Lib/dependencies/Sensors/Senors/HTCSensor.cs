using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace Sensors
{
    public enum HTCSensor : uint
    {
        Something = 0,
        GSensor = 1,
        Light = 2,
        Another = 3,
    }
    public class HTCSensorBase
    {
        // The following PInvokes were ported from the results of the reverse engineering done
        // by Scott at scottandmichelle.net.
        // Blog post: http://scottandmichelle.net/scott/comments.html?entry=784
        [DllImport("HTCSensorSDK")]
        protected extern static IntPtr HTCSensorOpen(HTCSensor sensor);

        [DllImport("HTCSensorSDK")]
        protected extern static void HTCSensorClose(IntPtr handle);
    }
}
