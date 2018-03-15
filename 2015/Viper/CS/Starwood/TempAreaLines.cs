using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk;
using System.IO;
using System.Windows;

namespace Revit.SDK.Samples.UIAPI.CS.Starwood
{
    public class TempAreaLines
    {
        public List<bLine> intersectinglines { get; set; }
        public List<bLine> outerlines { get; set; }
        public List<bLine> NewLines1 { get; set; }
        public List<bLine> NewLines2 { get; set; }
        public Project Proj { get; set; }
        public StringBuilder tempsb { get; set; }

        public TempAreaLines(Project proj)
        {
            this.intersectinglines = new List<bLine>();
            this.outerlines = new List<bLine>();
            this.NewLines1 = new List<bLine>();
            this.NewLines2 = new List<bLine>();
            this.Proj = proj;
            this.tempsb = new StringBuilder();
        }


        private XYZ pickpointnot(bLine known)
        {
            XYZ thisout = new XYZ(0, 0, 0);
             VpReporting vpr = new VpReporting();

             tempsb.AppendLine();
             tempsb.AppendLine(vpr.linereport(known.line));

            bool end1isfree = true;
            bool end2isfree = true;

            for (int i = 0; i < this.NewLines1.Count; ++i)
            {
                bLine bl = this.NewLines1.ElementAt(i);
                if (bl != known)
                {
                    tempsb.AppendLine(vpr.linereport(bl.line));

                    if (bl.line.GetEndPoint(0).DistanceTo(known.line.GetEndPoint(0)) < .001
                        || bl.line.GetEndPoint(1).DistanceTo(known.line.GetEndPoint(0)) < .001)
                    { end1isfree = false;   }
                    if (bl.line.GetEndPoint(0).DistanceTo(known.line.GetEndPoint(1)) < .001
                       || bl.line.GetEndPoint(1).DistanceTo(known.line.GetEndPoint(1)) < .001)
                    {  end2isfree = false;  }
                }
            }

            if (end1isfree == false && end2isfree == false)
            {
                tempsb.AppendLine("both false");
                return null;
            }
            
            else if (end1isfree == false && end2isfree == true)
            {
                tempsb.AppendLine(known.line.GetEndPoint(1).ToString());
                return known.line.GetEndPoint(1);
            }
            else if (end1isfree == true && end2isfree == false)
            {
                tempsb.AppendLine(known.line.GetEndPoint(0).ToString());
                return known.line.GetEndPoint(0); 
            }
            else
            {
                tempsb.AppendLine("something is wrong");
                return null;
            }
        }


        private bLine getcurvefrompoint(XYZ point)
        {
            for (int i = 0; i < this.outerlines.Count; ++i)
            {
                if (this.outerlines.ElementAt(i).line.GetEndPoint(0).DistanceTo(point) < .001
                    || this.outerlines.ElementAt(i).line.GetEndPoint(1).DistanceTo(point) < .001)
                {
                    return this.outerlines.ElementAt(i);
                }
            }
            return null;
        }

        public void sortcurves()
        {
            tempsb.AppendLine("SORT CURVES");
            //newlines has one item
             bLine blcurrent = this.NewLines1.Last();

            //find the point which does not intersect anotherouterline
            XYZ pointtocontinuewith = pickpointnot(blcurrent);
            if (pointtocontinuewith == null)
            {
                // done     
                tempsb.AppendLine(" DONE");

               // this.NewLines2.Add(blcurrent);
                this.outerlines.Remove(blcurrent);

                foreach (bLine bl in this.outerlines)
                {
                    this.NewLines2.Add(bl);
                }
                this.outerlines.Clear();

            }
            else
            {
                //find the line in outerlines which intersects in
                bLine blnew = getcurvefrompoint(pointtocontinuewith);
                if (blnew != null)
                {
                    this.NewLines1.Add(blnew);
                    this.outerlines.Remove(blnew);
                    
                    this.sortcurves();
                }
                else
                {
                    Proj.so.WriteLine("855 fail");
                }
            }
      //      MessageBox.Show(tempsb.ToString());

            this.Proj.so.WriteLine("************");
         //   this.Proj.so.WriteLine(tempsb.ToString());

            killdoubles();
           // MessageBox.Show(tempsb.ToString());
        }


        private void killdoubles()
        {
            this.intersectinglines = killdoublelist(this.intersectinglines);
            this.NewLines1 = killdoublelist(this.NewLines1);
            this.NewLines2 = killdoublelist(this.NewLines2);
        }

        private List<bLine> killdoublelist(List<bLine> blist)
        {
            for (int i = 0; i < blist.Count; ++i)
            {
                bLine bl1 = blist.ElementAt(i);
                for (int j = 0; j < blist.Count; ++j)
                 {
                    bLine bl2 = blist.ElementAt(j);
                    if (i != j)
                    {
                        if (bl1.line.GetEndPoint(0) == bl2.line.GetEndPoint(0) &&
                            bl1.line.GetEndPoint(1) == bl2.line.GetEndPoint(1))
                        {
                            blist.RemoveAt(j);
                            break;
                        }
                    }
                }
            }
            return blist;

        }

        public string report_temp()
        {
            StringBuilder sb = new StringBuilder();
             VpReporting vpr = new VpReporting();

             this.Proj.so.WriteLine("intersecting");
             sb.AppendLine("intersecting");
             foreach (bLine bl in this.intersectinglines)
             {
                 this.Proj.so.WriteLine(vpr.linereport(bl.line) + bl.masterline);
             sb.AppendLine(vpr.linereport(bl.line) + bl.masterline);
             }

             this.Proj.so.WriteLine("outerlines");
             sb.AppendLine("outerlines");
             foreach (bLine bl in this.outerlines)
             {
                 this.Proj.so.WriteLine(vpr.linereport(bl.line) + bl.masterline);
             sb.AppendLine(vpr.linereport(bl.line) + bl.masterline);
             }

             this.Proj.so.WriteLine("NewLines1");
             sb.AppendLine("NewLines1");
             foreach (bLine bl in this.NewLines1)
             {
                 this.Proj.so.WriteLine(vpr.linereport(bl.line) + bl.masterline);
             sb.AppendLine(vpr.linereport(bl.line) + bl.masterline);
             }

             this.Proj.so.WriteLine("NewLines2");
             sb.AppendLine("NewLines2");
             foreach (bLine bl in this.NewLines2)
             {
                 this.Proj.so.WriteLine(vpr.linereport(bl.line) + bl.masterline);
             sb.AppendLine(vpr.linereport(bl.line) + bl.masterline);
             }


             return sb.ToString();
        }
    }
}
