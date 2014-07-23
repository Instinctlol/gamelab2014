using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace GestureLib
{
    /// <summary>
    /// Holds a list TrainedGestures
    /// </summary>
    public class TrainedGestureCollection : GenericBaseCollection<TrainedGesture>
    {
        /// <summary>
        /// Gets the TrainedGesture, which matches best to given gesture algorithms.
        /// </summary>
        /// <param name="matchedAlgorithms">The gesture algorithms, which will be searched in the TrainedGesture items.</param>
        /// <returns></returns>
        public TrainedGesture GetTrainedGestureByMatchedAlgorithms(GestureAlgorithmCollection matchedAlgorithms)
        {
            return this.FirstOrDefault(
                tg => tg.GestureAlgorithms.SequenceEqual(matchedAlgorithms));
        }
    }
}
