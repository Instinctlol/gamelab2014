using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace GestureLib
{
    internal partial class OverlayForm : Form
    {
        private Queue<PointF> _pointListQueue;
        private Size _pointerSize;

        internal OverlayForm()
        {
            InitializeComponent();
            
            _pointListQueue = new Queue<PointF>(10);
            _pointerSize = new Size(20, 20);
        }


        private void OverlayForm_Paint(object sender, PaintEventArgs e)
        {
            if (_pointListQueue.Count > 0)
            {
                PointF[] pointListArray = _pointListQueue.ToArray();
                int alphaStepValue = 255 / _pointListQueue.Count;

                for (int i = 0; i < pointListArray.Length; i++)
                {
                    PointF pointF = pointListArray[i];

                    if (!pointF.IsEmpty)
                    {
                        Color color = Color.FromArgb(alphaStepValue * i, Color.Black);

                        Point point = new Point(
                            (int)(pointF.X * (float)Width) - _pointerSize.Width / 2,
                            (int)(pointF.Y * (float)Height) - _pointerSize.Height / 2);

                        e.Graphics.FillEllipse(
                            new SolidBrush(color),
                            point.X, point.Y, _pointerSize.Width, _pointerSize.Height);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the size of the pointer, which shows the last drawn coordinates.
        /// </summary>
        /// <value>The size of the pointer.</value>
        public Size PointerSize 
        {
            get { return _pointerSize; }
            set
            {
                _pointerSize = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Sets the current point, which will bei shown on the semi-transparency form.
        /// </summary>
        /// <param name="point">The point.</param>
        public void SetCurrentPoint(PointF point)
        {
            System.Threading.Monitor.Enter(_pointListQueue);

            if (_pointListQueue.Count == 2)
                _pointListQueue.Dequeue();

            _pointListQueue.Enqueue(point);
            Invalidate();

            System.Threading.Monitor.Exit(_pointListQueue);
        }
    }
}
