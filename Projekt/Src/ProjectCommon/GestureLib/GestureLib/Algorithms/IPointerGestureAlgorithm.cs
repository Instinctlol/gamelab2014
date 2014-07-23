using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GestureLib
{
    /// <summary>
    /// Describes methods and properties must be used in new pointer algorithms
    /// </summary>
    public interface IPointerGestureAlgorithm : IGestureAlgorithm
    {
        /// <summary>
        /// Calculates the matching of two points with this algorithm.
        /// </summary>
        /// <param name="fromPoint">Start point.</param>
        /// <param name="toPoint">End point.</param>
        /// <returns>An indicator between 0 and 1 for the probability, that the gesture matched.</returns>
        float CalculateMatching(PointF fromPoint, PointF toPoint);
    }
}
