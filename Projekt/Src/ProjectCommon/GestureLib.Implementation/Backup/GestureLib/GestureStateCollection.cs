using System;
using System.Collections.Generic;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Holds a list of GestureStates
    /// </summary>
    /// <typeparam name="T">Must implement the Interface <see cref="IGestureState"/></typeparam>
    public class GestureStateCollection<T> : GenericBaseCollection<T> where T : IGestureState
    {
    }
}
