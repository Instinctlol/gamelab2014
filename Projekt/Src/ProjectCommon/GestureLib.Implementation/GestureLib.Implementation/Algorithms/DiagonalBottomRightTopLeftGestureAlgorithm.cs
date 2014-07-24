﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GestureLib
{
    /// <summary>
    /// Algorithm for diagonal lines, which trend from bottom right to top left.
    /// </summary>
    public class DiagonalBottomRightTopLeftGestureAlgorithm : IPointerGestureAlgorithm
    {
        #region IPointerGestureAlgorithm Members

        /// <summary>
        /// Calculates the matching value for a diagonal line, which trends from bottom right to top left.
        /// </summary>
        /// <param name="fromPoint">Start point.</param>
        /// <param name="toPoint">End point.</param>
        /// <returns>
        /// An indicator between 0 and 1 for the probability, that the gesture matched.
        /// </returns>
        public float CalculateMatching(PointF fromPoint, PointF toPoint)
        {
            if (toPoint.X < fromPoint.X &&
                toPoint.Y < fromPoint.Y)
            {
                double angle = MathUtility.CalculateGradientAngle(fromPoint, toPoint);

                if (angle > 45.0)
                    angle = 90.0 - angle;

                float result = (float)angle / 45.0F;

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