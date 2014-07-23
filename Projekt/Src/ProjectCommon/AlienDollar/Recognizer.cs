#region Disclainmer
/**
 * The $1 Unistroke Recognizer (C# version)
 *
 *		Jacob O. Wobbrock, Ph.D.
 * 		The Information School
 *		University of Washington
 *		Mary Gates Hall, Box 352840
 *		Seattle, WA 98195-2840
 *		wobbrock@u.washington.edu
 *
 *		Andrew D. Wilson, Ph.D.
 *		Microsoft Research
 *		One Microsoft Way
 *		Redmond, WA 98052
 *		awilson@microsoft.com
 *
 *		Yang Li, Ph.D.
 *		Department of Computer Science and Engineering
 * 		University of Washington
 *		The Allen Center, Box 352350
 *		Seattle, WA 98195-2840
 * 		yangli@cs.washington.edu
 *
 * The Protractor enhancement was published by Yang Li and programmed here by 
 * Jacob O. Wobbrock.
 *
 *	Li, Y. (2010). Protractor: A fast and accurate gesture 
 *	  recognizer. Proceedings of the ACM Conference on Human 
 *	  Factors in Computing Systems (CHI '10). Atlanta, Georgia
 *	  (April 10-15, 2010). New York: ACM Press, pp. 2169-2172.
 * 
 * This software is distributed under the "New BSD License" agreement:
 * 
 * Copyright (c) 2007-2011, Jacob O. Wobbrock, Andrew D. Wilson and Yang Li.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright
 *      notice, this list of conditions and the following disclaimer in the
 *      documentation and/or other materials provided with the distribution.
 *    * Neither the names of the University of Washington nor Microsoft,
 *      nor the names of its contributors may be used to endorse or promote 
 *      products derived from this software without specific prior written
 *      permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS
 * IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO,
 * THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL Jacob O. Wobbrock OR Andrew D. Wilson
 * OR Yang Li BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
 * OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY
 * OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
**/
#endregion
using System;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using WobbrockLib;
using WobbrockLib.Extensions;

namespace Recognizer.Dollar
{
	public class Recognizer
	{
		#region Members

        public const int NumPoints = 64;
        private const float DX = 250f;
        public static readonly SizeF SquareSize = new SizeF(DX, DX);
        public static readonly double Diagonal = Math.Sqrt(DX * DX + DX * DX);
        public static readonly double HalfDiagonal = 0.5 * Diagonal;
        public static readonly PointF Origin = new PointF(0f, 0f);
        private static readonly double Phi = 0.5 * (-1.0 + Math.Sqrt(5.0)); // Golden Ratio

        // batch testing
        private const int NumRandomTests = 100;
        public event ProgressEventHandler ProgressChangedEvent;

		//private Hashtable _gestures;
        private Dictionary<string, Unistroke> _gestures;

		#endregion

		#region Constructor
	
		public Recognizer()
		{
            _gestures = new Dictionary<string, Unistroke>(256);
		}

		#endregion

        #region Gesture Loader
        public bool LoadGesture(string filename)
        {
            bool success = true;
            XmlTextReader reader = null;
            try
            {
                reader = new XmlTextReader(filename);
                reader.WhitespaceHandling = WhitespaceHandling.None;
                reader.MoveToContent();

                Unistroke p = ReadGesture(reader);

                // remove any with the same name and add the prototype gesture
                if (_gestures.ContainsKey(p.Name))
                    _gestures.Remove(p.Name);
                _gestures.Add(p.Name, p);
            }
            catch (XmlException xex)
            {
                Console.Write(xex.Message);
                success = false;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                success = false;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return success;
        }
        #endregion

        #region Recognition

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timepoints"></param>
        /// <param name="protractor"></param>
        /// <returns></returns>
        public NBestList Recognize(List<TimePointF> timepoints, bool protractor) // candidate points
        {
            double I = GeotrigEx.PathLength(timepoints) / (NumPoints - 1); // interval distance between points
            List<PointF> points = TimePointF.ConvertList(SeriesEx.ResampleInSpace(timepoints, I));
            double radians = GeotrigEx.Angle(GeotrigEx.Centroid(points), points[0], false);
            points = GeotrigEx.RotatePoints(points, -radians);
            points = GeotrigEx.ScaleTo(points, SquareSize);
            points = GeotrigEx.TranslateTo(points, Origin, true);
            List<double> vector = Unistroke.Vectorize(points); // candidate's vector representation

            NBestList nbest = new NBestList();
            foreach (Unistroke u in _gestures.Values)
            {
                if (protractor) // Protractor extension by Yang Li (CHI 2010)
                {
                    double[] best = OptimalCosineDistance(u.Vector, vector);
                    double score = 1.0 / best[0];
                    nbest.AddResult(u.Name, score, best[0], best[1]); // name, score, distance, angle
                }
                else // original $1 angular invariance search -- Golden Section Search (GSS)
                {
                    double[] best = GoldenSectionSearch(
                            points,                             // to rotate
                            u.Points,                           // to match
                            GeotrigEx.Degrees2Radians(-45.0),   // lbound
                            GeotrigEx.Degrees2Radians(+45.0),   // ubound
                            GeotrigEx.Degrees2Radians(2.0)      // threshold
                        );

                    double score = 1.0 - best[0] / HalfDiagonal;
                    nbest.AddResult(u.Name, score, best[0], best[1]); // name, score, distance, angle
                }
            }
            nbest.SortDescending(); // sort descending by score so that nbest[0] is best result
            return nbest;
        }

        // From http://www.math.uic.edu/~jan/mcs471/Lec9/gss.pdf
        private double[] GoldenSectionSearch(List<PointF> pts1, List<PointF> pts2, double a, double b, double threshold)
        {
            double x1 = Phi * a + (1 - Phi) * b;
            List<PointF> newPoints = GeotrigEx.RotatePoints(pts1, x1);
            double fx1 = PathDistance(newPoints, pts2);

            double x2 = (1 - Phi) * a + Phi * b;
            newPoints = GeotrigEx.RotatePoints(pts1, x2);
            double fx2 = PathDistance(newPoints, pts2);

            double i = 2.0; // calls to pathdist
            while (Math.Abs(b - a) > threshold)
            {
                if (fx1 < fx2)
                {
                    b = x2;
                    x2 = x1;
                    fx2 = fx1;
                    x1 = Phi * a + (1 - Phi) * b;
                    newPoints = GeotrigEx.RotatePoints(pts1, x1);
                    fx1 = PathDistance(newPoints, pts2);
                }
                else
                {
                    a = x1;
                    x1 = x2;
                    fx1 = fx2;
                    x2 = (1 - Phi) * a + Phi * b;
                    newPoints = GeotrigEx.RotatePoints(pts1, x2);
                    fx2 = PathDistance(newPoints, pts2);
                }
                i++;
            }
            return new double[3] { Math.Min(fx1, fx2), GeotrigEx.Radians2Degrees((b + a) / 2.0), i }; // distance, angle, calls to pathdist
        }

        /// <summary>
        /// From Protractor by Yang Li, published at CHI 2010. See http://yangl.org/protractor/. 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        private double[] OptimalCosineDistance(List<double> v1, List<double> v2)
        {
            double a = 0.0;
            double b = 0.0;
            for (int i = 0; i < Math.Min(v1.Count, v2.Count); i += 2)
            {
                a += v1[i] * v2[i] + v1[i + 1] * v2[i + 1];
                b += v1[i] * v2[i + 1] - v1[i + 1] * v2[i];
            }
            double angle = Math.Atan(b / a);
            double distance = Math.Acos(a * Math.Cos(angle) + b * Math.Sin(angle));
            return new double[3] { distance, GeotrigEx.Radians2Degrees(angle), 0.0 }; // distance, angle, calls to pathdist
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <returns></returns>
        public static double PathDistance(List<PointF> path1, List<PointF> path2)
        {
            double distance = 0;
            for (int i = 0; i < Math.Min(path1.Count, path2.Count); i++)
            {
                distance += GeotrigEx.Distance(path1[i], path2[i]);
            }
            return distance / path1.Count;
        }

        // continues to rotate 'pts1' by 'step' degrees as long as points become ever-closer 
        // in path-distance to pts2. the initial distance is given by D. the best distance
        // is returned in array[0], while the angle at which it was achieved is in array[1].
        // array[3] contains the number of calls to PathDistance.
        private double[] HillClimbSearch(List<PointF> pts1, List<PointF> pts2, double D, double step)
        {
            double i = 0.0;
            double theta = 0.0;
            double d = D;
            do
            {
                D = d; // the last angle tried was better still
                theta += step;
                List<PointF> newPoints = GeotrigEx.RotatePoints(pts1, GeotrigEx.Degrees2Radians(theta));
                d = PathDistance(newPoints, pts2);
                i++;
            }
            while (d <= D);
            return new double[3] { D, theta - step, i }; // distance, angle, calls to pathdist
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts1"></param>
        /// <param name="pts2"></param>
        /// <param name="writer"></param>
        /// <returns></returns>
        private double[] FullSearch(List<PointF> pts1, List<PointF> pts2, StreamWriter writer)
        {
            double bestA = 0d;
            double bestD = PathDistance(pts1, pts2);

            for (int i = -180; i <= +180; i++)
            {
                List<PointF> newPoints = GeotrigEx.RotatePoints(pts1, GeotrigEx.Degrees2Radians(i));
                double d = PathDistance(newPoints, pts2);
                if (writer != null)
                {
                    writer.WriteLine("{0}\t{1:F3}", i, Math.Round(d, 3));
                }
                if (d < bestD)
                {
                    bestD = d;
                    bestA = i;
                }
            }
            writer.WriteLine("\nFull Search (360 rotations)\n{0:F2}{1}\t{2:F3} px", Math.Round(bestA, 2), (char) 176, Math.Round(bestD, 3)); // calls, angle, distance
            return new double[3] { bestD, bestA, 360.0 }; // distance, angle, calls to pathdist
        }

        #endregion

        #region Gestures & Xml

        public int NumGestures
		{
			get
			{
                return _gestures.Count;
			}
		}

        public List<Unistroke> Gestures
        {
            get
            {
                List<Unistroke> list = new List<Unistroke>(_gestures.Values);
                list.Sort();
                return list;
            }
        }

		public void ClearGestures()
		{
            _gestures.Clear();
		}

		public bool SaveGesture(string filename, List<TimePointF> points)
		{
			// add the new prototype with the name extracted from the filename.
            string name = Unistroke.ParseName(filename);
            if (_gestures.ContainsKey(name))
                _gestures.Remove(name);
			Unistroke newPrototype = new Unistroke(name, points);
            _gestures.Add(name, newPrototype);

            // do the xml writing
			bool success = true;
			XmlTextWriter writer = null;
			try
			{
				// save the prototype as an Xml file
				writer = new XmlTextWriter(filename, Encoding.UTF8);
				writer.Formatting = Formatting.Indented;
				writer.WriteStartDocument(true);
				writer.WriteStartElement("Gesture");
				writer.WriteAttributeString("Name", name);
				writer.WriteAttributeString("NumPts", XmlConvert.ToString(points.Count));
                writer.WriteAttributeString("Millseconds", XmlConvert.ToString(points[points.Count - 1].Time - points[0].Time));
                writer.WriteAttributeString("AppName", Assembly.GetExecutingAssembly().GetName().Name);
				writer.WriteAttributeString("AppVer", Assembly.GetExecutingAssembly().GetName().Version.ToString());
				writer.WriteAttributeString("Date", DateTime.Now.ToLongDateString());
				writer.WriteAttributeString("TimeOfDay", DateTime.Now.ToLongTimeString());

				// write out the raw individual points
				foreach (TimePointF p in points)
				{
					writer.WriteStartElement("Point");
					writer.WriteAttributeString("X", XmlConvert.ToString(p.X));
					writer.WriteAttributeString("Y", XmlConvert.ToString(p.Y));
                    writer.WriteAttributeString("T", XmlConvert.ToString(p.Time));
					writer.WriteEndElement(); // <Point />
				}

				writer.WriteEndDocument(); // </Gesture>
			}
			catch (XmlException xex)
			{
				Console.Write(xex.Message);
				success = false;
			}
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                success = false;
            }
			finally
			{
				if (writer != null)
					writer.Close();
			}
			return success; // Xml file successfully written (or not)
		}

        //public bool LoadGesture(string filename)
        //{
        //    bool success = true;
        //    XmlTextReader reader = null;
        //    try
        //    {
        //        reader = new XmlTextReader(filename);
        //        reader.WhitespaceHandling = WhitespaceHandling.None;
        //        reader.MoveToContent();

        //        Unistroke p = ReadGesture(reader);

        //        // remove any with the same name and add the prototype gesture
        //        if (_gestures.ContainsKey(p.Name))
        //            _gestures.Remove(p.Name);
        //        _gestures.Add(p.Name, p);
        //    }
        //    catch (XmlException xex)
        //    {
        //        Console.Write(xex.Message);
        //        success = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Write(ex.Message);
        //        success = false;
        //    }
        //    finally
        //    {
        //        if (reader != null)
        //            reader.Close();
        //    }
        //    return success;
        //}

        // assumes the reader has been just moved to the head of the content.
        private Unistroke ReadGesture(XmlTextReader reader)
        {
            Debug.Assert(reader.LocalName == "Gesture");
            string name = reader.GetAttribute("Name");
            
            List<TimePointF> points = new List<TimePointF>(XmlConvert.ToInt32(reader.GetAttribute("NumPts")));
            
            reader.Read(); // advance to the first Point
            Debug.Assert(reader.LocalName == "Point");

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                TimePointF p = TimePointF.Empty;
                p.X = XmlConvert.ToSingle(reader.GetAttribute("X"));
                p.Y = XmlConvert.ToSingle(reader.GetAttribute("Y"));
                p.Time = XmlConvert.ToInt64(reader.GetAttribute("T"));
                points.Add(p);
                reader.ReadStartElement("Point");
            }

            return new Unistroke(name, points);
        }

        #endregion

    }
}