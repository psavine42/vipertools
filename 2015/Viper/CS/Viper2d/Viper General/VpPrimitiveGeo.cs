using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI.Selection;
//using Microsoft.X

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RApplication = Autodesk.Revit.ApplicationServices.Application;
namespace Revit.SDK.Samples.UIAPI.CS
{
    class VpGeoUtils
    {

        /// <summary>
        /// Tests which side of the line a point is on
        /// Returns 1 or -1 (use for multiple tests)
        /// </summary>
        /// <param name="known"></param>
        /// <param name="t1"></param>
        /// <returns></returns>
        public int computeside(Line known, XYZ t1)
        {
            XYZ k1 = known.GetEndPoint(0);
            XYZ k2 = known.GetEndPoint(1);
            double dl = (k2.X - k1.X) * (t1.Y - k1.Y) - (k2.Y - k1.Y) * (t1.X - k1.X);
            //    return dl;
            if (dl > 0) return 1;
            else return -1;
        }

        /// <summary>
        /// offset a line, dist is the distance of the offset. if negative, offset is negative
        /// </summary>
        /// <param name="line"></param>
        /// <param name="dist"></param>
        /// <returns></returns>
        public Line Offsetline(Line line, double dist)
        {
            Transform tr = Transform.CreateRotation(XYZ.BasisZ, Math.PI / 2);
            Line l2 = line.Clone().CreateTransformed(tr) as Line;
            //if (dist > 0)
            //{
            Transform mv = Transform.CreateTranslation(l2.Direction * dist);
            return line.Clone().CreateTransformed(mv) as Line;
            //}
            //else
            //{

            //    Transform mv = Transform.CreateTranslation(-l2.Direction * dist);
            //    return line.Clone().CreateTransformed(mv) as Line;
            //}
        }

        public Line ExtendLine(Line l, bool e1, bool e2, double dist)
        {
            XYZ p1 = new XYZ();
            XYZ p2 = new XYZ();

            if (e1 == true)
            { p1 = l.GetEndPoint(0) - l.Direction * dist; }
            else
            { p1 = l.GetEndPoint(0); }


            if (e2 == true)
            { p2 = l.GetEndPoint(1) + l.Direction * dist; }
            else
            { p2 = l.GetEndPoint(1); }

            Line extend = Line.CreateBound(p1, p2);
            return extend;
        }


        public XYZ IntersectLines(Line l1, Line l2)
        {

           double x1 = l1.GetEndPoint(0).X;
           double x2 = l1.GetEndPoint(1).X;
           double x3 = l2.GetEndPoint(0).X;
           double x4 = l2.GetEndPoint(1).X;

           double y1 = l1.GetEndPoint(0).Y;
           double y2 = l1.GetEndPoint(1).Y;
           double y3 = l2.GetEndPoint(0).Y;
           double y4 = l2.GetEndPoint(1).Y;


           double x12 = x1 - x2;
           double x34 = x3 - x4;
           double y12 = y1 - y2;
           double y34 = y3 - y4;

           double c = x12 * y34 - y12 * x34;

            if (Math.Abs(c) < 0.01)
            {
                // No intersection
                return null;
            }
            else
            {
                // Intersection
                double a = x1 * y2 - y1 * x2;
                double b = x3 * y4 - y3 * x4;

                double x = (a * x34 - b * x12) / c;
                double y = (a * y34 - b * y12) / c;

                return new XYZ(x , y , l2.GetEndPoint(0).Z);
            }
        }


    }
}
