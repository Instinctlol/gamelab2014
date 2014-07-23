using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.ObjectModel;

namespace GestureLib
{
    /// <summary>
    /// Used for getting all corner marks and some additional information of a pointer gesture
    /// </summary>
    public class PointTendenceAnalyzer
    {
        private List<PointTendence> _pointTendences;

        internal PointTendenceAnalyzer(GestureStateCollection<PointerGestureState> recordedGestureStates)
        {
            List<PointF> cornerMarks = new List<PointF>();
            List<List<PointTendence>> pointTendenceGroups = new List<List<PointTendence>>();
            List<ReadOnlyCollection<PointTendence>> pointTendenceGroupsReadOnlyWrapper = new List<ReadOnlyCollection<PointTendence>>();

            RecordedPointerGestureStates = recordedGestureStates;

            #region Recognize the drawn lines
            if (recordedGestureStates.Count > 0)
            {
                _pointTendences = new List<PointTendence>();

                for (int i = 0; i < RecordedPointerGestureStates.Count - 1; i++)
                {
                    PointTendence pointTendence = new PointTendence(
                        new PointF(RecordedPointerGestureStates[i].X, RecordedPointerGestureStates[i].Y),
                        new PointF(RecordedPointerGestureStates[i + 1].X, RecordedPointerGestureStates[i + 1].Y));

                    _pointTendences.Add(pointTendence);

                    //System.Diagnostics.Debug.Print("{0}\t{1}", pointTendence.Horizontal, pointTendence.Vertical);
                }

                #region Calculate Gesture Bounds
                float minX = RecordedPointerGestureStates.Min(pgs => pgs.X);
                float minY = RecordedPointerGestureStates.Min(pgs => pgs.Y);

                float maxX = RecordedPointerGestureStates.Max(pgs => pgs.X);
                float maxY = RecordedPointerGestureStates.Max(pgs => pgs.Y);

                GestureBounds = new RectangleF(minX, minY, maxX - minX, maxY - minY);
                #endregion

                List<List<PointTendence>> pointTendenceHorizontalGroups = new List<List<PointTendence>>();
                List<PointTendence> pointTendenceHorizontalGroup = new List<PointTendence>();

                List<List<PointTendence>> pointTendenceVerticalGroups = new List<List<PointTendence>>();
                List<PointTendence> pointTendenceVerticalGroup = new List<PointTendence>();

                #region Grouping of Tendences
                for (int i = 1; i < _pointTendences.Count; i++)
                {
                    PointTendence previousPointTendence = _pointTendences[i - 1];
                    PointTendence currentPointTendence = _pointTendences[i];

                    pointTendenceHorizontalGroup.Add(previousPointTendence);
                    pointTendenceVerticalGroup.Add(previousPointTendence);

                    //Split the full Tendence-List into 2 categories (horizontal and vertical) and
                    //Create everytime the Tendence changes to the last point a new subgroup in the right catagory and add the Tendence

                    if (previousPointTendence.Horizontal != currentPointTendence.Horizontal)
                    {
                        pointTendenceHorizontalGroups.Add(pointTendenceHorizontalGroup);
                        pointTendenceHorizontalGroup = new List<PointTendence>();
                    }

                    if (previousPointTendence.Vertical != currentPointTendence.Vertical)
                    {
                        pointTendenceVerticalGroups.Add(pointTendenceVerticalGroup);
                        pointTendenceVerticalGroup = new List<PointTendence>();
                    }
                }

                pointTendenceHorizontalGroups.Add(pointTendenceHorizontalGroup);
                pointTendenceVerticalGroups.Add(pointTendenceVerticalGroup);
                #endregion

                #region Eliminate groups with just a few elements
                
                //Clear all groups in every category with less than 10 elements
                //Mark these points as removed (IgnoreDirecetionHorizontal and IgnoreDirecetionVertical) so they can
                //be ignored in the mergeing process, which follows up after this step
                for (int i = 0; i < pointTendenceHorizontalGroups.Count - 1; i++)
                {
                    if (pointTendenceHorizontalGroups[i + 1].Count < 10)
                    {
                        pointTendenceHorizontalGroups[i + 1].ForEach(pt => pt.IgnoreDirectionHorizontal = true);

                        pointTendenceHorizontalGroups[i].AddRange(pointTendenceHorizontalGroups[i + 1]);
                        pointTendenceHorizontalGroups.RemoveAt(i + 1);
                        i--;
                    }
                }

                for (int i = 0; i < pointTendenceVerticalGroups.Count - 1; i++)
                {
                    if (pointTendenceVerticalGroups[i + 1].Count < 10)
                    {
                        pointTendenceVerticalGroups[i + 1].ForEach(pt => pt.IgnoreDirectionVertical = true);

                        pointTendenceVerticalGroups[i].AddRange(pointTendenceVerticalGroups[i + 1]);
                        pointTendenceVerticalGroups.RemoveAt(i + 1);
                        i--;
                    }
                }
                #endregion

                List<PointTendence> pointTendenceGroup = new List<PointTendence>();

                #region Re-Grouping of Tendences

                //In the last step the IgnoreDirectionX property of the PointTendence-Refernces were manipulated
                //All Points, which are marked to be ignored, will not be merged
                for (int i = 1; i < _pointTendences.Count; i++)
                {
                    PointTendence previousPointTendence = _pointTendences[i - 1];
                    PointTendence currentPointTendence = _pointTendences[i];

                    pointTendenceGroup.Add(previousPointTendence);

                    //Create a new cleaned PointTendence-List without all ignored PointTendences
                    if ((previousPointTendence.Horizontal != currentPointTendence.Horizontal && !currentPointTendence.IgnoreDirectionHorizontal) ||
                        (previousPointTendence.Vertical != currentPointTendence.Vertical && !currentPointTendence.IgnoreDirectionVertical))
                    {
                        pointTendenceGroups.Add(pointTendenceGroup);
                        pointTendenceGroup = new List<PointTendence>();
                    }
                }

                pointTendenceGroups.Add(pointTendenceGroup);
                #endregion

                //Cleanup groups with less than 10 elements
                pointTendenceGroups.RemoveAll(pdg => pdg.Count < 10);

                #region Extract CornerMarks

                foreach (List<PointTendence> pointTendenceGroupItem in pointTendenceGroups)
                {
                    pointTendenceGroupsReadOnlyWrapper.Add(new ReadOnlyCollection<PointTendence>(pointTendenceGroupItem));
                }

                List<PointF> dirtyCornerMarks = new List<PointF>();

                //Generate the CornerMarks of the groups (still dirty, cause CornerMarks may exist, which successively follow in the same direction)
                foreach (List<PointTendence> pointTendenceGroupItem in pointTendenceGroups)
                {
                    if (pointTendenceGroupItem.Count > 0)
                    {
                        dirtyCornerMarks.Add(pointTendenceGroupItem[0].From);
                    }
                }

                if (pointTendenceGroups.Count > 0)
                {
                    List<PointTendence> pointTendenceGroupItem = pointTendenceGroups[pointTendenceGroups.Count - 1];

                    if (pointTendenceGroupItem.Count > 0)
                    {
                        PointTendence pointTendenceTemp = pointTendenceGroupItem[pointTendenceGroupItem.Count - 1];
                        dirtyCornerMarks.Add(pointTendenceTemp.To);
                    }
                }


                if (dirtyCornerMarks.Count > 0)
                {
                    List<PointTendence> dirtyPointTendences = new List<PointTendence>();

                    //Generate Tendences of the CornerMarks
                    for (int i = 0; i < dirtyCornerMarks.Count - 1; i++)
                    {
                        PointTendence pointTendence = new PointTendence(
                            new PointF(dirtyCornerMarks[i].X, dirtyCornerMarks[i].Y),
                            new PointF(dirtyCornerMarks[i + 1].X, dirtyCornerMarks[i + 1].Y));

                        dirtyPointTendences.Add(pointTendence);
                    }

                    //Cleanup CornerMarks which follows in the same direction
                    for (int i = 0; i < dirtyPointTendences.Count - 1; i++)
                    {
                        if (dirtyPointTendences[i].Horizontal == dirtyPointTendences[i + 1].Horizontal &&
                            dirtyPointTendences[i].Vertical == dirtyPointTendences[i + 1].Vertical)
                        {
                            dirtyPointTendences.RemoveAt(i + 1);
                            i--;
                        }
                    }

                    //Generate the real CornerMarks
                    foreach (PointTendence pointTendenceItem in dirtyPointTendences)
                    {
                        cornerMarks.Add(pointTendenceItem.From);
                    }

                    if (dirtyPointTendences.Count > 0)
                    {
                        cornerMarks.Add(dirtyPointTendences[dirtyPointTendences.Count - 1].To);
                    }
                }
                #endregion
            }
            #endregion

            CornerMarks = cornerMarks.AsReadOnly();
            PointTendenceGroups = pointTendenceGroupsReadOnlyWrapper.AsReadOnly();
        }

        /// <summary>
        /// Gets the recorded pointer gesture states.
        /// </summary>
        /// <value>The recorded pointer gesture states.</value>
        public GestureStateCollection<PointerGestureState> RecordedPointerGestureStates { get; private set; }

        /// <summary>
        /// Gets the bounds, whithin the gesture is drawn.
        /// </summary>
        /// <value>The gesture bounds.</value>
        public RectangleF? GestureBounds { get; private set; }

        /// <summary>
        /// Gets the analyzed corner marks of the pointer gesture.
        /// </summary>
        /// <value>The corner marks.</value>
        public ReadOnlyCollection<PointF> CornerMarks { get; private set; }

        /// <summary>
        /// Gets the grouped point tendences. The list is grouped by all points between two corner marks.
        /// </summary>
        /// <value>The point tendence groups.</value>
        public ReadOnlyCollection<ReadOnlyCollection<PointTendence>> PointTendenceGroups { get; private set; }

        /// <summary>
        /// Gets all the matched gesture algorithms.
        /// </summary>
        /// <value>The matched gesture algorithms.</value>
        public GestureAlgorithmCollection MatchedGestureAlgorithms { get; internal set; }
    }
}
