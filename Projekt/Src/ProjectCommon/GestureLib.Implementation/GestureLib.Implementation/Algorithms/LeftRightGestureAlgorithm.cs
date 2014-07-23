using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GestureLib
{
    /// <summary>
    /// Algorithm for lines, which trend from left to right.
    /// </summary>
    public class LeftRightGestureAlgorithm : IPointerGestureAlgorithm
    {
        #region IPointerGestureAlgorithm Members

        /// <summary>
        /// Calculates the matching value for a line, which trends from left to right.
        /// </summary>
        /// <param name="fromPoint">Start point.</param>
        /// <param name="toPoint">End point.</param>
        /// <returns>
        /// An indicator between 0 and 1 for the probability, that the gesture matched.
        /// </returns>
        public float CalculateMatching(PointF fromPoint, PointF toPoint)
        {
            if (toPoint.X - fromPoint.X > Math.Abs(fromPoint.Y - toPoint.Y))
            {
                double angle = MathUtility.CalculateGradientAngle(fromPoint, toPoint);
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
