using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GestureLib
{
    /// <summary>
    /// Algorithm for lines, which trend from top to bottom.
    /// </summary>
    public class TopBottomGestureAlgorithm : IPointerGestureAlgorithm
    {
        #region IPointerGestureAlgorithm Members

        /// <summary>
        /// Calculates the matching value for a line, which trends from top to bottom.
        /// </summary>
        /// <param name="fromPoint">Start point.</param>
        /// <param name="toPoint">End point.</param>
        /// <returns>
        /// An indicator between 0 and 1 for the probability, that the gesture matched.
        /// </returns>
        public float CalculateMatching(PointF fromPoint, PointF toPoint)
        {
            //When the difference of the Y-coordinate between end- and
            //start-point is greater than the absolute difference between
            //the X-coordinates 
            //  ==> line runs from top to bottom
            if (toPoint.Y - fromPoint.Y >
                Math.Abs(fromPoint.X - toPoint.X))
            {
                //cause the line runs in a vertical direction, the 
                //calculated degrees are substracted from 90, which
                //describes the degree value for a optimal vertical line
                double angle = 90.0 - 
                                MathUtility.CalculateGradientAngle(
                                    fromPoint,
                                    toPoint);

                //the nearer the angle reaches 45 degrees, the less
                //is the return value of the CalculationMatching-function
                float result = 1.0F - (float)angle / 45.0F;

                return result;
            }
            else
            {
                //==> line does not run from top to bottom
                return 0.0F;
            }
        }

        #endregion

        #region INamed Members

        public string Name { get; set; }

        #endregion
    }
}
