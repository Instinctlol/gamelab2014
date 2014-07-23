using System;
using System.Collections.Generic;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Describes an association between algorithms and actions.
    /// </summary>
    public class TrainedGesture : INamed
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TrainedGesture"/> class.
        /// </summary>
        public TrainedGesture()
        {
            GestureActions = new GestureActionCollection();
            GestureAlgorithms = new GestureAlgorithmCollection();
        }

        /// <summary>
        /// Collection of the GestureActions, which should be executed, when the GestureAlgorithms match.
        /// </summary>
        /// <value>The gesture actions.</value>
        public GestureActionCollection GestureActions { get; private set; }

        /// <summary>
        /// Collection of the GestureAlgorithms, wich can be checked, when a gesture was executed.
        /// </summary>
        /// <value>The gesture algorithms.</value>
        public GestureAlgorithmCollection GestureAlgorithms { get; private set; }

        /// <summary>
        /// Gets or sets the date of the last gesture execution.
        /// </summary>
        /// <value>The last execution at.</value>
        public DateTime? LastExecutionAt { get; set; }

        /// <summary>
        /// Gets or sets the date when this item was saved at.
        /// </summary>
        /// <value>The saved at.</value>
        public DateTime? SavedAt { get; set; }

        #region INamed Members

        /// <summary>
        /// Gets or sets the name of this item.
        /// So, the member can be identified in the collection of TrainedGestures.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; set; }

        #endregion
    }
}
