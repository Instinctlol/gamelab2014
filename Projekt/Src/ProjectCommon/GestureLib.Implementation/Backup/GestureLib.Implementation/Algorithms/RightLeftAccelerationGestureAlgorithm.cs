using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureLib
{
    public class RightLeftAccelerationGestureAlgorithm : IAccelerationGestureAlgorithm
    {
        #region IAccelerationGestureAlgorithm Members

        public float CalculateMatching(GestureStateCollection<AccelerationGestureState> gestureStates)
        {
            if (gestureStates.Count > 0)
            {
                float firstX = gestureStates[0].X;
                float lastX = gestureStates[gestureStates.Count - 1].X;

                float firstY = gestureStates[0].Y;
                float lastY = gestureStates[gestureStates.Count - 1].Y;
                float diffY = Math.Abs(lastY - firstY);

                //Calculate the average of the z-values
                float avgZ = Math.Abs(gestureStates.Average(ags => ags.Z));

                if (avgZ > 0.5 && avgZ < 1.5)
                {
                    if (lastX - firstX < -2 && diffY < 1.0)
                    {
                        return 1.0F;
                    }
                    else
                    {
                        return 0.0F;
                    }
                }
                else
                {
                    return 0.0F;
                }
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
