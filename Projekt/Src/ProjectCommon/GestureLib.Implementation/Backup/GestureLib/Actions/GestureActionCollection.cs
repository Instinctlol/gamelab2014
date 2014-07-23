using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace GestureLib
{
    /// <summary>
    /// Holds a list of gesture actions
    /// </summary>
    public class GestureActionCollection : GenericBaseCollection<IGestureAction>
    {
        internal bool AutonamingEnabled { get; set; }

        /// <summary>
        /// Calls the Execute-method of all available gesture actions in this collection.
        /// </summary>
        public void ExecuteAll()
        {
            ForEach(a => a.Execute());
        }

        /// <summary>
        /// Adds the specified gesture action item and names it automatically if no name was assigned and Autonaming is enabled internally.
        /// </summary>
        /// <param name="item">The gesture action item.</param>
        public override void Add(IGestureAction item)
        {
            base.Add(item);

            if (AutonamingEnabled && item.Name == null)
            {
                item.Name = Utility.GenerateNextAvailableName<IGestureAction>(this, "Action");
            }
        }
    }
}
