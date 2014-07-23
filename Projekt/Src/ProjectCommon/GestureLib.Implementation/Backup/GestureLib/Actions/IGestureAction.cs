using System;
using System.Collections.Generic;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Defines the methods and properties of an action command, which can be executed, when GestureAlgorithms match.
    /// </summary>
    public interface IGestureAction : INamed
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        void Execute();
    }
}
