using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GestureLib
{
    /// <summary>
    /// Holds a list of gesture algorithms
    /// </summary>
    public class GestureAlgorithmCollection : GenericBaseCollection<IGestureAlgorithm>
    {
        internal bool AutonamingEnabled { get; set; }

        /// <summary>
        /// Adds the specified gesture algorithm item and names it automatically if no name was assigned and Autonaming is enabled internally.
        /// </summary>
        /// <param name="item">The item.</param>
        public override void Add(IGestureAlgorithm item)
        {
            base.Add(item);

            if (AutonamingEnabled && item.Name == null)
            {
                item.Name = Utility.GenerateNextAvailableName<IGestureAlgorithm>(this, "Algorithm");
            }
        }
    }
}
