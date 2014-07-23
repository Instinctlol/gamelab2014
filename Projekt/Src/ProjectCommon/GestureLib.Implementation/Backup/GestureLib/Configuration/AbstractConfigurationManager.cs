using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using GestureLib;

namespace GestureLib
{
    /// <summary>
    /// Describes a base class for new configuration managers (e.g. xml or several databases).
    /// With this classes TrainedGestures easily can be saved or loaded.
    /// </summary>
    public abstract class AbstractConfigurationManager
    {
        private GestureLib _gestureLib;

        /// <summary>
        /// Gets the reference to the source instance of the gesture lib.
        /// </summary>
        /// <value>The gesture lib.</value>
        protected GestureLib GestureLib
        {
            get { return _gestureLib; }
        }

        internal GestureLib InternalGestureLib
        {
            get { return _gestureLib; }
            set { _gestureLib = value; }
        }

        /// <summary>
        /// Saves the TrainedGesture values.
        /// </summary>
        public abstract void Save();

        /// <summary>
        /// Loads the TrainedGesture values.
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Gets a gesture algorithm by a given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected IGestureAlgorithm GetGestureAlgorithmByName(string name)
        {
            return GestureLib.AvailableGestureAlgorithms.Single(a => a.Name == name);
        }

        /// <summary>
        /// Gets a gesture action by a given name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        protected IGestureAction GetGestureActionByName(string name)
        {
            return GestureLib.AvailableGestureActions.Single(a => a.Name == name);
        }
    }
}
