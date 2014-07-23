using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureLib
{
    public class JabGestureAlgorithm : IAccelerationGestureAlgorithm
    {
        #region IAccelerationGestureAlgorithm Members

        public float CalculateMatching(GestureStateCollection<AccelerationGestureState> gestureStates)
        {
            float minX = gestureStates.Min(ags => ags.X);
            float maxX = gestureStates.Max(ags => ags.X);
            float diffX = Math.Abs(maxX - minX);
            float avgX = gestureStates.Average(ags => ags.X);

            float minY = gestureStates.Min(ags => ags.Y);
            float maxY = gestureStates.Max(ags => ags.Y);
            float diffY = Math.Abs(maxY - minY);
            float avgY = gestureStates.Average(ags => ags.Y);

            float avgZ = gestureStates.Average(ags => ags.Z);

            if (avgZ > 0.9 && avgX < 0.2 && avgY < 0.2 && diffX < 1.0 && diffY > 1.0)
            {
                return 1.0F;
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
