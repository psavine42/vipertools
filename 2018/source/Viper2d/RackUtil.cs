using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Diagnostics;

namespace Viper.Viper2d
{

    public class PipeRun
    {
        // single run of a pipe. 
        public List<TwoPoint> templist { get; set; }
        public double offset { get; set; }
        public TwoPoint origionalpipe { get; set; }
        public XYZ origin { get; set; }
        private double Z { get; set; }
        private Makepipes mp = new Makepipes();

        public PipeRun(double offset_dist, TwoPoint OrigionalMepCurve)
        {
            // distance to offset each line
            this.templist = new List<TwoPoint>();
            this.offset = offset_dist;
            this.origionalpipe = OrigionalMepCurve;
            this.templist.Add(OrigionalMepCurve);
            Line l = (this.origionalpipe.Mepcurve.Location as LocationCurve).Curve as Line;
            this.Z = l.GetEndPoint(0).Z;
        }

        public PipeRun(double dist, TwoPoint OrigionalMepCurve, List<Line> lines)
        {
            this.templist = new List<TwoPoint>();
            this.offset = dist;
            this.origionalpipe = OrigionalMepCurve;
            this.templist.Add(OrigionalMepCurve);
            Line l = (this.origionalpipe.Mepcurve.Location as LocationCurve).Curve as Line;
            this.Z = l.GetEndPoint(0).Z;
            Build(lines);
        }

        public void Build(List<Line> lines)
        {
            BuildExtendedOffsets(lines);
            IntersectAndTrim();
        }

        public void Transact(Document doc)
        {
            ViperUtils vputil = new ViperUtils();
            List<TwoPoint> geolist = Makepipes.MAKE_PIPES_general(this.templist, doc);
            vputil.connectrun(geolist, doc);
        }

        public void BuildExtendedOffsets(List<Line> lines)
        {
            int ext = 10;
            for (int j = 0; j < lines.Count; j++)
            {
                double dl = this.offset;
                Line offset = lines.ElementAt(j).CreateOffset(this.offset, XYZ.BasisZ) as Line;
                Line extended;
                if (j == lines.Count - 1)
                {
                     extended = HelperMethods.ExtendLine(offset, ext, 0);
                }
                else
                {
                     extended = HelperMethods.ExtendLine(offset, ext, ext);
                }
                XYZ p1 = new XYZ(extended.GetEndPoint(0).X, extended.GetEndPoint(0).Y, this.Z);
                XYZ p2 = new XYZ(extended.GetEndPoint(1).X, extended.GetEndPoint(1).Y, this.Z);
                TwoPoint tp = new TwoPoint(p1, p2, this.origionalpipe.Mepcurve);
                this.templist.Add(tp);
            }
        }

        private void IntersectAndTrim()
        {
            for (int i = 0; i < templist.Count - 1; i++)
            {
                TwoPoint tpcur = templist.ElementAt(i);
                Line lcur = Line.CreateBound(tpcur.pt1, tpcur.pt2);

                TwoPoint tpnext = templist.ElementAt(i + 1);
                Line lnext = Line.CreateBound(tpnext.pt1, tpnext.pt2);

                //if the two are perpendicular
                bool bl = mp.TESTtwopointOnSameAxis(tpcur, tpnext);
                if (bl == true)
                {
                    TwoPoint tpnew = mp.TwoPointRebuild(tpcur, tpnext);
                    tpnew.Mepcurve = tpcur.Mepcurve;
                    templist.Insert(i, tpnew);
                    templist.Remove(tpcur);
                    templist.Remove(tpnext);
                    IntersectAndTrim();
                }
                else
                {
                    IntersectionResultArray intres;
                    lcur.Intersect(lnext, out intres);
                    if (intres != null)
                    {
                        IntersectionResult inters = intres.get_Item(0);
                        XYZ intpoint = inters.XYZPoint;
                        tpcur.pt2 = intpoint;
                        tpnext.pt1 = intpoint;
                    }
                }
            }
        }
    }

    class RackUtils
    {
        private VpReporting vpr = new VpReporting();
        private StringBuilder sb = new StringBuilder();

        private static Line realignMain(MEPCurve main, Line tempLine)
        {
            Line knownlocation = (main.Location as LocationCurve).Curve as Line;
            XYZ MainPoint = new XYZ();
            XYZ knownpoint = tempLine.GetEndPoint(0);
            double dist0 = knownpoint.DistanceTo(knownlocation.GetEndPoint(0));
            double dist1 = knownpoint.DistanceTo(knownlocation.GetEndPoint(1));
            if (dist0 > dist1)
            {
                return Line.CreateBound(knownlocation.GetEndPoint(0), knownlocation.GetEndPoint(1));
            }
            else
            {
                return Line.CreateBound(knownlocation.GetEndPoint(1), knownlocation.GetEndPoint(0));
            }
        }

        private static PolyLine toPolyline(List<Line> lines)
        {
            List<XYZ> points = new List<XYZ>();
            for (int i = 0; i < lines.Count - 1; i++)
            {
                Line tpcur = lines.ElementAt(i);
                if (i == 0)
                {
                    points.Add(lines.ElementAt(i).GetEndPoint(0));
                }
                points.Add(lines.ElementAt(i).GetEndPoint(1));
            }
            return PolyLine.Create(points);
        }
 
        public static List<PipeRun> BisectingAngles(List<Line> lines, List<MEPCurve> original_meps, MEPCurve main)
        {
            List<PipeRun> runs = new List<PipeRun>();
            
            Line knownlocation = realignMain(main, lines.ElementAt(0));
            XYZ mainPoint = knownlocation.GetEndPoint(0);
            //lines.Insert(0, knownlocation);
           
            Debug.WriteLine("  ");
            foreach (MEPCurve crv in original_meps)
            {
                Line testlocation = (crv.Location as LocationCurve).Curve as Line;
                double dist = testlocation.Distance(knownlocation.Evaluate(.5, true));
                Debug.WriteLine("MEP");
                Debug.WriteLine( testlocation.GetEndPoint(0).ToString() + testlocation.GetEndPoint(1).ToString());
 
                // get 
                XYZ closestendpoint = ViperUtils.nearestpipepoints(crv, mainPoint);
                XYZ farthstendpoint = ViperUtils.farthestpipepoints(crv, mainPoint);

                Debug.WriteLine("p1 " + closestendpoint.ToString() +   " p2 " + farthstendpoint.ToString());
        
                Line updated = Line.CreateBound(farthstendpoint, closestendpoint);
                Line offset1 = updated.CreateOffset(dist, XYZ.BasisZ) as Line;
                Line offset2 = updated.CreateOffset(-dist, XYZ.BasisZ) as Line;

                double d1 = offset1.Distance(mainPoint);
                double d2 = offset2.Distance(mainPoint);
                int side = (d1 <= d2) ? 1 :-1;

                double offset = dist * side;

                //todo - if it is on start of pipe, build forward,
                // otherwise build should be reveresed.

                TwoPoint original_curve = new TwoPoint(crv);
                PipeRun run = new PipeRun(offset, original_curve, lines);
                Debug.WriteLine("Temp : ");
                runs.Add(run);
                Debug.WriteLine("end\n");
            }
            original_meps.Clear();
            return runs;
        }
    }


}
