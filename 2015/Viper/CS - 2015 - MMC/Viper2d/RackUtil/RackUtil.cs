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

namespace Revit.SDK.Samples.UIAPI.CS.Viper2d
{



    public class RackRun
    {
        public List<twopoint> templist { get; set; }
        public double offset { get; set; }
        public twopoint origionalpipe { get; set; }
        public XYZ origin { get; set; }

        public RackRun(double dist, twopoint OrigionalMepCurve)
        {
            this.templist = new List<twopoint>();
            this.offset = dist;
            this.origionalpipe = OrigionalMepCurve;
        }




    }


    




    class RackUtils
    {
        private ViperUtils vpu = new ViperUtils();
        private VpGeoUtils vpg = new VpGeoUtils();
        private VpReporting vpr = new VpReporting();
        private Makepipes mp = new Makepipes();
        private StringBuilder sb = new StringBuilder();

        public RackUtils()
        {


        }

 
        private void buildracks(List<Line> lines, RackRun run)
        {
            for (int j = 0; j < lines.Count; j++)
            {
                double dl = run.offset;
                Line offset = vpg.Offsetline(lines.ElementAt(j), dl);
                Line extended = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(1, 0, 0));

                if (j == 0)
                {  
                   // Line testlocation = (run.origionalpipe.Mepcurve.Location as LocationCurve).Curve as Line;
                    Line ll = Line.CreateBound(run.origionalpipe.pt1, run.origionalpipe.pt2);
                    extended = vpg.ExtendLine(ll, false, true, 7); 
                }

                else if (j == lines.Count - 1)
                { extended = vpg.ExtendLine(offset, true, false, 7);  }

                else
                { extended = vpg.ExtendLine(offset, true, true,7); }

               Line l = (run.origionalpipe.Mepcurve.Location as LocationCurve).Curve as Line;


                twopoint tp = new twopoint(new XYZ(
                    extended.GetEndPoint(0).X,
                    extended.GetEndPoint(0).Y,
                    l.GetEndPoint(0).Z),
                    new XYZ(
                    extended.GetEndPoint(1).X,
                    extended.GetEndPoint(1).Y,
                    l.GetEndPoint(0).Z),
                run.origionalpipe.Mepcurve);

                run.templist.Add(tp);
            }
        }
       // public List<twopoint> BisectingAngles(List<Line> lines, List<MEPCurve> curves, MEPCurve main)
      
 
        public List<RackRun> BisectingAngles(List<Line> lines, List<MEPCurve> curves, MEPCurve main)
        { 
            Line knownlocation = (main.Location as LocationCurve).Curve as Line;
            StringBuilder sb = new StringBuilder();
            List<RackRun> runs = new List<RackRun>();
            XYZ MainPoint = new XYZ();
            XYZ knownpoint = lines.ElementAt(0).GetEndPoint(0);
            double dist0 = knownpoint.DistanceTo(knownlocation.GetEndPoint(0));
            double dist1 = knownpoint.DistanceTo(knownlocation.GetEndPoint(1));
            if (dist0 > dist1)
            {
                MainPoint = knownlocation.GetEndPoint(1);
                knownlocation = Line.CreateBound(knownlocation.GetEndPoint(0), knownlocation.GetEndPoint(1));
            }
            else
            {
                MainPoint = knownlocation.GetEndPoint(0);
                knownlocation = Line.CreateBound(knownlocation.GetEndPoint(1), knownlocation.GetEndPoint(0));
            }

            lines.Insert(0, knownlocation);
            sb.AppendLine("mainpoint = " + MainPoint.ToString());

            //Build the rackruns
            foreach (MEPCurve crv in curves)
            {
                Line testlocation = (crv.Location as LocationCurve).Curve as Line;
                double dist = testlocation.Distance(knownlocation.Evaluate(.5, true));
                //
                XYZ closestendpoint = vpu.nearestpipepoints(crv, MainPoint);
                XYZ farthestendpoint = vpu.farthestpipepoints(crv, MainPoint);
                testlocation = Line.CreateBound(farthestendpoint, closestendpoint);
                //
                Line offset1 = vpg.Offsetline(testlocation, dist);
                Line offset2 = vpg.Offsetline(testlocation, -dist );

                double d1 = offset1.Distance(MainPoint);
                double d2 = offset2.Distance(MainPoint);

                int side;
                if (d1 <= d2) { side = -1; }
                else { side = 1; }


                twopoint tp = new twopoint(testlocation.GetEndPoint(0),   testlocation.GetEndPoint(1), crv);
                RackRun run = new RackRun(dist * side, tp); 
                runs.Add(run);
            }

            foreach (RackRun run in runs)
            {
                //add the lines to each run with correct offset
                buildracks(lines, run);
                List<twopoint> tpr = rebuildlist(run.templist);
                run.templist = tpr;
            }
            

            List<twopoint> listout = new List<twopoint>();
                foreach (RackRun run in runs)
                {

                    foreach (twopoint tp in run.templist)
                    {
                        listout.Add(tp);
                    }
                }
            curves.Clear();
           // runs.Clear();
            sb.Clear();
            return runs;
          //  return listout;  
        }

        private List<twopoint> rebuildlist(List<twopoint> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {

                twopoint tpcur = list.ElementAt(i);
                Line lcur = Line.CreateBound(tpcur.pt1, tpcur.pt2);

                twopoint tpnext = list.ElementAt(i + 1);
                Line lnext = Line.CreateBound(tpnext.pt1, tpnext.pt2);

                //if the two are perpendicular
                bool bl = mp.TESTtwopointOnSameAxis(tpcur, tpnext);
                if (bl == true)
                {
                    twopoint tpnew = mp.TwoPointRebuild(tpcur, tpnext);
                    tpnew.Mepcurve = tpcur.Mepcurve;
                    list.Insert(i, tpnew);
                    list.Remove(tpcur);
                    list.Remove(tpnext);
                    list = rebuildlist(list);
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
                    else { }
                }
            }
            return list;

        }

    }


}
