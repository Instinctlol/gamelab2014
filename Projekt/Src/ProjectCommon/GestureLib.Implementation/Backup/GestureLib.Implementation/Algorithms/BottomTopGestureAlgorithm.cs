using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureLib
{
    /// <summary>
    /// Algorithm for lines, which trend from bottom to top.
    /// </summary>
    public class BottomTopGestureAlgorithm : IPointerGestureAlgorithm
    {
        #region IPointerGestureAlgorithm Members

        /// <summary>
        /// Calculates the matching value for a line, which trends from bottom to top.
        /// </summary>
        /// <param name="fromPoint">Start point.</param>
        /// <param name="toPoint">End point.</param>
        /// <returns>
        /// An indicator between 0 and 1 for the probability, that the gesture matched.
        /// </returns>
        public float CalculateMatching(System.Drawing.PointF fromPoint, System.Drawing.PointF toPoint)
        {
            if (fromPoint.Y - toPoint.Y > Math.Abs(fromPoint.X - toPoint.X))
            {
                double angle = 90.0 - MathUtility.CalculateGradientAngle(fromPoint, toPoint);
                float result = 1.0F - (float)angle / 45.0F;

                return result;
            }
            else
            {
                return 0.0F;
            }
        }

        #endregion

        #region INamed Members

        public string Name { get; set; }

        #endregion
    }
}
