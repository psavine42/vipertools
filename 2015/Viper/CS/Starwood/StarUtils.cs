using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk;
using System.Diagnostics;
//using Rhino.Geometry;

namespace Revit.SDK.Samples.UIAPI.CS.Starwood
{
    class StarUtils
    {

        public double calculateidealarea(List<UnitType> unittypes)
        {
            double totalarea = 0;
            foreach (UnitType ut in unittypes)
            {
                double dl = ut.numberofunits * ut.ideallength * ut.idealwidth;
                totalarea = totalarea + dl;
            }
            return totalarea;
        }

        public double calculateminarea(List<UnitType> unittypes)
        {
            double totalarea = 0;
            foreach (UnitType ut in unittypes)
            {
                double dl = ut.numberofunits * ut.idealarea;
                totalarea = totalarea + dl;
            }
            return totalarea;
        }

        public double calculatemaxarea(List<UnitType> unittypes)
        {
            double totalarea = 0;
            foreach (UnitType ut in unittypes)
            {
                double dl = ut.numberofunits * ut.idealarea;
                totalarea = totalarea + dl;
            }
            return totalarea;
        }

        public double calculatetotalhall_length(List<UnitType> unittypes)
        {
            double total = 0;
            foreach (UnitType ut in unittypes)
            {
                double dl = ut.numberofunits * ut.idealwidth;
                total = total + dl;
            }
            return total;
        }


        /// <summary>
        /// Sort a list of curves to make them correctly 
        /// ordered and oriented to form a closed loop.
        /// </summary>
        public List<XYZ> SortCurvesContiguous( IList<Line> curves,
          bool debug_output, StringBuilder sb)
        {
            List<XYZ> listout = new List<XYZ>();
            int n = curves.Count;
            const double _inch = 1.0 / 12.0;
            const double _sixteenth = _inch / 16.0;
            // Walk through each curve (after the first) 
            // to match up the curves in order

            for (int i = 0; i < n; ++i)
            {
                Autodesk.Revit.DB.Curve curve = curves[i];
                XYZ endPoint = curve.GetEndPoint(1);

                if (debug_output)
                {
                    sb.Append( "{0} endPoint {1} " + i + " " + endPoint.ToString());
                   listout.Add(endPoint);
                }

                XYZ p;

                // Find curve with start point = end point
                bool found = (i + 1 >= n);

                for (int j = i + 1; j < n; ++j)
                {
                    p = curves[j].GetEndPoint(0);

                    // If there is a match end->start, 
                    // this is the next curve
                    if (_sixteenth > p.DistanceTo(endPoint))
                    {
                        if (debug_output)
                        {
                            sb.Append(
                              "{0} start point, swap with {1}" + 
                              j + " " +  i + 1);
                        }

                        if (i + 1 != j)
                        {
                            Line tmp = curves[i + 1];
                            curves[i + 1] = curves[j];
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }

                    p = curves[j].GetEndPoint(1);

                    // If there is a match end->end, 
                    // reverse the next curve

                    if (_sixteenth > p.DistanceTo(endPoint))
                    {
                        if (i + 1 == j)
                        {
                            if (debug_output)
                            {
                                sb.AppendLine(
                                  "{0} end point, reverse {1} " + 
                                  j + " " +  i + 1);
                            }

                            curves[i + 1] = CreateReversedCurve(curves[j]);
                        }
                        else
                        {
                            if (debug_output)
                            {
                                sb.Append(
                                  "{0} end point, swap with reverse {1} " + 
                                  j + " " +  i + 1);
                                //listout.Add(new XYZ(
                            }

                            Line tmp = curves[i + 1];
                            curves[i + 1] = CreateReversedCurve(curves[j]);
                            curves[j] = tmp;
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                   // TaskDialog.Show("f" , sb.ToString());
                  //  throw new Exception("SortCurvesContiguous:"
                  //    + " non-contiguous input curves" );
                }
            }
            return listout;
        }




        public Line Linenearestopoint (List<Line> lns, XYZ point)
        {
            Line base1 = lns.ElementAt(0);
            double dl = 100000;

            foreach (Line ln in lns)
            {

                if (point.DistanceTo(ln.GetEndPoint(0)) < dl)
                {
                    dl = point.DistanceTo(ln.GetEndPoint(0));
                        base1 = ln;
                }
            }
            // TaskDialog.Show("as", base1.ToString());
            return base1;
        }




        /// <summary>
        /// Create a new curve with the same 
        /// geometry in the reverse direction.
        /// </summary>
        /// <param name="orig">The original curve.</param>
        /// <returns>The reversed curve.</returns>
        /// <throws cref="NotImplementedException">If the 
        /// curve type is not supported by this utility.</throws>
       public Line CreateReversedCurve( Line orig)
        {
            if (orig == null)
            {
                //throw new NotImplementedException(
                //  "CreateReversedCurve for type "
                //  + orig.GetType().Name);
            }

            if (orig is Line)
            {
                return Line.CreateBound(
                  orig.GetEndPoint(1),
                  orig.GetEndPoint(0));
            }
                //else if (orig is Arc)
                //{
                //    return Arc.Create(orig.GetEndPoint(1),
                //      orig.GetEndPoint(0),
                //      orig.Evaluate(0.5, true));
                //}
            else
            {
                throw new Exception(
                  "CreateReversedCurve - Unreachable");
            }
        }

       // build list of lines in revit as an area floor
       public void BuildAreaInRevit(List<Line> rcurves, Document doc)
       {
           
            //   List<Autodesk.Revit.DB.Line> Revcrvs = new List<Autodesk.Revit.DB.Line>();
               CurveArray floorCurveArray = new CurveArray();
               foreach (Autodesk.Revit.DB.Line crv in rcurves)
               {
                   //   Autodesk.Revit.DB.Line l = Autodesk.Revit.DB.Line.CreateBound(new XYZ(crv.PointAtStart.X, crv.PointAtStart.Y, crv.PointAtStart.Z),
                   //        new XYZ(crv.PointAtStart.X, crv.PointAtStart.Y, crv.PointAtStart.Z));
                   floorCurveArray.Append(crv);
               }

               Transaction tr = new Transaction(doc, "thsi");
               tr.Start();
             //  try
            //   {
           
                   doc.Create.NewFloor(floorCurveArray, false);
                   tr.Commit();
              // }
             //  catch (Exception) { tr.RollBack(); }
       }


       public void BuildLinesAreaInRevit(List<Line> rcurves, Document doc, double xoffset, double yoffset, double zoffset)
       {

           CurveArray floorCurveArray = new CurveArray();
           foreach (Autodesk.Revit.DB.Line crv in rcurves)
           {
               Autodesk.Revit.DB.Line l = Autodesk.Revit.DB.Line.CreateBound(
                   new XYZ(crv.GetEndPoint(0).X + xoffset,
                       crv.GetEndPoint(0).Y + yoffset,
                       crv.GetEndPoint(0).Z + zoffset),
                   new XYZ(crv.GetEndPoint(1).X + xoffset,
                       crv.GetEndPoint(1).Y + yoffset,
                       crv.GetEndPoint(1).Z + zoffset));

               floorCurveArray.Append(l);
                   
                  
           }
           
            Plane plane = new Plane(doc.ActiveView.ViewDirection, rcurves.ElementAt(0).Origin );
            

           Transaction tr = new Transaction(doc, "thsi");
           tr.Start();
           SketchPlane sp = SketchPlane.Create(doc, plane);
           doc.Create.NewModelCurveArray(floorCurveArray, sp);
           tr.Commit();
 
       }





        //public Line Offsetline(Line line, double dist, Plane plane)
        //{
           


        //}

        ////Convert to rhinocommon
        //public Rhino.Geometry.Line ConverttoRC (Autodesk.Revit.DB.Line line)
        //{
        //    Point3d p1 = new Point3d(line.GetEndPoint(0).X, line.GetEndPoint(0).Y, line.GetEndPoint(0).Z);
        //    Point3d p2 = new Point3d(line.GetEndPoint(1).X, line.GetEndPoint(1).Y, line.GetEndPoint(1).Z);
        //    Rhino.Geometry.Line cr = new Rhino.Geometry.Line(p1, p2);
        //    //  Rhino.Geometry.Line mline = new Rhino.Geometry.Line(p1, p2);
        //  //  Rhino.Geometry.Curve cr = mline.ToNurbsCurve();
        //    return cr;
        //}


        //given a line test which side a testing poitn is on of that line
        public double computeside(Line known, XYZ t1)
         {
            XYZ k1 = known.GetEndPoint(0);
            XYZ k2 = known.GetEndPoint(1);
            double dl = (k2.X - k1.X)*(t1.Y - k1.Y) - (k2.Y - k1.Y)*(t1.X - k1.X);
            return dl;
            //if (dl > 0.001)
          //   { return dl; }
         //   else 
          //   { return 0; }
         }

     

    }



}
