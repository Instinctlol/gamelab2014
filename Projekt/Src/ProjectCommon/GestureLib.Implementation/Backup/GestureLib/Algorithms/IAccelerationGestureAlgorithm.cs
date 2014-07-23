using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Describes methods and properties must be used in new acceleration algorithms
    /// </summary>
    public interface IAccelerationGestureAlgorithm : IGestureAlgorithm
    {
        /// <summary>
        /// Calculates the matching of a collection of acceleration gesture states, with this algorithm.
        /// </summary>
        /// <param name="gestureStates">The acceleration gesture states.</param>
        /// <returns>A indicator between 0 and 1 for the probability, that the gesture matched.</returns>
        float CalculateMatching(GestureStateCollection<AccelerationGestureState> gestureStates);
    }
}
