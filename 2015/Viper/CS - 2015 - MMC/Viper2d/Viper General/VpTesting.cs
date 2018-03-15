using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;
using GXYZ = Autodesk.Revit.DB.XYZ;
using System.Windows.Forms;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using System.Linq;


namespace Revit.SDK.Samples.UIAPI.CS
{
    public class VpTesting
    {

        public void LineDistanceToPoint(Element element , XYZ pt, Document doc,
            List<Element> lines, string paramtofill1, string paramtofill2)
        {
           // sb.AppendLine();
            double mindistance = 1000;
            string controlline = " ";
            string ptlevel = ptlevelname(pt, doc);

            foreach (Element line in lines)
            {
                LocationCurve lineloc = line.Location as LocationCurve;
                XYZ ptzright = new GXYZ(pt.X, pt.Y, lineloc.Curve.GetEndPoint(0).Z);
                string linelev = ptlevelname(lineloc.Curve.GetEndPoint(0), doc);
                double dl = lineloc.Curve.Distance(ptzright);

                if (mindistance > dl ) // && ptlevel == linelev)
                {
                    mindistance = dl;
                    controlline = line.get_Parameter("Grid").AsString();
                }
            }

            if (mindistance != 1000) //&& st != "lol")
            {
                double rounded = Math.Round(mindistance, 2);
                element.get_Parameter(paramtofill2).Set(controlline);
                element.get_Parameter(paramtofill1).Set(rounded);
            }

        }



        private string ptlevelname(XYZ pt, Document doc)
        {
            List<Level> levels = new FilteredElementCollector(doc)
             .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();
            string levout = "null";
            double dl = 1000000;
            foreach (Level lev in levels)
            {
                if (Math.Abs(lev.Elevation - pt.Z) <= dl)
                {
                    dl = Math.Abs(lev.Elevation - pt.Z);
                    levout = lev.Name;
                }
            }
            return levout;
        }


    }
}
