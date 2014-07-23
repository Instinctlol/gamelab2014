using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GestureLib
{
    /// <summary>
    /// Defines a tendence of a line in the horizontal direction
    /// </summary>
    public enum HorizontalTendenceDirection
    {
        /// <summary>
        /// Line runs exactly straight down or up
        /// </summary>
        None,
        /// <summary>
        /// Line runs in the left direction
        /// </summary>
        Left,
        /// <summary>
        /// Line runs in the right direction
        /// </summary>
        Right
    }

    /// <summary>
    /// Defines a tendence of a line in the vertical direction
    /// </summary>
    public enum VerticalTendenceDirection
    {
        /// <summary>
        /// Line runs exactly straight left or right
        /// </summary>
        None,
        /// <summary>
        /// Line runs in the top direction
        /// </summary>
        Top,
        /// <summary>
        /// Line runs in the bottom direction
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Describes a tendence between two points
    /// </summary>
    public class PointTendence
    {
        internal PointTendence(PointF from, PointF to)
        {
            From = from;
            To = to;

            PointF diffPoint = new PointF(To.X - From.X, To.Y - From.Y);

            #region Horizontal
            if (diffPoint.X > 0)
            {
                Horizontal = HorizontalTendenceDirection.Right;
            }
            else if (diffPoint.X < 0)
            {
                Horizontal = HorizontalTendenceDirection.Left;
            }
            else
            {
                Horizontal = HorizontalTendenceDirection.None;
            }
            #endregion

            #region Vertical
            if (diffPoint.Y > 0)
            {
                Vertical = VerticalTendenceDirection.Bottom;
            }
            else if (diffPoint.Y < 0)
            {
                Vertical = VerticalTendenceDirection.Top;
            }
            else
            {
                Vertical = VerticalTendenceDirection.None;
            }
            #endregion
        }

        /// <summary>
        /// Gets the calculated horizontal tendence.
        /// </summary>
        /// <value>The horizontal.</value>
        public HorizontalTendenceDirection Horizontal { get; private set; }

        /// <summary>
        /// Gets the calculated vertical tendence.
        /// </summary>
        /// <value>The vertical.</value>
        public VerticalTendenceDirection Vertical { get; private set; }
        
        /// <summary>
        /// Gets the start point.
        /// </summary>
        /// <value>From.</value>
        public PointF From { get; private set; }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>To.</value>
        public PointF To { get; private set; }

        internal bool IgnoreDirectionHorizontal { get; set; }
        internal bool IgnoreDirectionVertical { get; set; }
    }
}
