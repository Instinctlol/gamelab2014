using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestureLib
{
    public class PitchGestureAlgorithm : IAccelerationGestureAlgorithm
    {
        #region IAccelerationGestureAlgorithm Members

        public float CalculateMatching(GestureStateCollection<AccelerationGestureState> gestureStates)
        {
            int initialPositionIndexes = 3;

            if (gestureStates.Count > initialPositionIndexes)
            {
                //Calculate the average of the first 3 x- and y-values
                float initAvgX = gestureStates.GetRange(0, initialPositionIndexes).Average(ags => ags.X);
                float initAvgY = gestureStates.GetRange(0, initialPositionIndexes).Average(ags => ags.Y);
                
                //Gets the highest y-value
                float maxY = gestureStates.Max(ags => ags.Y);

                //Gets the lowest and highest z-values
                float minZ = gestureStates.Min(ags => ags.Z);
                float maxZ = gestureStates.Max(ags => ags.Z);
                
                if (initAvgX > -1.0 && initAvgX < 1.0 && initAvgY > -1.0 && initAvgY < 1.0 && minZ < -2.0 && maxZ > 3.0 && maxY > 3.0)
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

        #endregion

        #region INamed Members

        public string Name { get; set; }

        #endregion
    }
}
