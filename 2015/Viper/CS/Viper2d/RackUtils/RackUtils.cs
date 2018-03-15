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
    class RackRun
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
           // this.origin;
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
                    extended = vpg.ExtendLine(offset, false, true, 10);
                }
                else if (j == lines.Count - 1)
                {
                    extended = vpg.ExtendLine(offset, true, false, 10);
                }
                else
                { extended = vpg.ExtendLine(offset, true, true, 10); }

                twopoint tp = new twopoint(extended.GetEndPoint(0), extended.GetEndPoint(1),
                 run.origionalpipe.Mepcurve);

                sb.AppendLine(tp.Mepcurve.Diameter.ToString() + vpr.linereport(extended));

                run.templist.Add(tp);
            }
        }

 
        public List<twopoint> BisectingAngles(List<Line> lines, List<MEPCurve> curves,
            MEPCurve main)
        { 
            Line knownlocation = (main.Location as LocationCurve).Curve as Line;
            StringBuilder sb = new StringBuilder();
            List<RackRun> runs = new List<RackRun>();

            //Build the rackruns
            foreach (MEPCurve crv in curves)
            {
                Line testlocation = (crv.Location as LocationCurve).Curve as Line;
                XYZ midpt = testlocation.Evaluate(.5, true);
                double dist = testlocation.Distance(knownlocation.Evaluate(.5, true));
                int side = vpg.computeside(knownlocation, midpt);

                twopoint tp = new twopoint(testlocation.GetEndPoint(0), 
                    testlocation.GetEndPoint(1), crv);

                RackRun run = new RackRun(dist * side, tp); 
                runs.Add(run);
                sb.AppendLine(dist.ToString() + "   " + side.ToString());
            }

            sb.AppendLine(knownlocation.ToString());
            sb.AppendLine();

            foreach (RackRun run in runs)
            {
                //add the lines to each run with correct offset
                buildracks(lines, run);

                /////
                run.origin = run.templist.ElementAt(0).pt1;
                XYZ p1 = run.origionalpipe.pt1;
                XYZ p2 = run.origionalpipe.pt2;
                double d1 = p1.DistanceTo(run.origin);
                double d2 = p2.DistanceTo(run.origin);
                if (d1 > d2)
                { twopoint tpn = new twopoint(p2, p1, run.origionalpipe.Mepcurve); }
                else
                { twopoint tpn = new twopoint(p1, p2, run.origionalpipe.Mepcurve); }

                ///
                twopoint tp = run.templist.ElementAt(0);
                Line ext = vpg.ExtendLine(Line.CreateBound(tp.pt1, tp.pt2), false, true, 5);
                twopoint tpnew = new twopoint(ext.GetEndPoint(0), ext.GetEndPoint(1), tp.Mepcurve);
                run.templist.RemoveAt(0);// = tpnew;
                run.templist.Insert(0, tpnew);


                ///
                run.templist.Insert(0, run.origionalpipe);

                /////

                List<twopoint> tpr = rebuildlist(run.templist);
                run.templist = tpr;
                /////
            }
            

            List<twopoint> listout = new List<twopoint>();
                foreach (RackRun run in runs)
                {
                    foreach (twopoint tp in run.templist)
                    {
                        listout.Add(tp);
                    }
                }
            sb.Clear();
            return listout;  
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
