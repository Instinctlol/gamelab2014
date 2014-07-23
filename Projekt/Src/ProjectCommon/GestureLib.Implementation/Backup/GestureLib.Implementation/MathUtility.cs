using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GestureLib
{
    internal static class MathUtility
    {
        public static double CalculateGradientAngle(
                                PointF startPoint,
                                PointF endPoint)
        {
            //Calculate the length of the adjacent and opposite
            float diffX = Math.Abs(endPoint.X - startPoint.X);
            float diffY = Math.Abs(endPoint.Y - startPoint.Y);

            //Calculates the Tan to get the radians (TAN(alpha) = opposite / adjacent)
            double radAngle = Math.Atan(diffY / diffX);

            //Converts the radians in degrees
            double degAngle = radAngle * 180 / Math.PI;

            return degAngle;
        }
    }
}
