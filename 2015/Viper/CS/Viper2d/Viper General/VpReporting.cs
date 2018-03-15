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
using Revit.SDK.Samples.UIAPI.CS.Starwood;
namespace Revit.SDK.Samples.UIAPI.CS
{
    class VpReporting
    {
        public string linereport(Line line)
        {
            string s1 = pointreport(line.GetEndPoint(0));
            string s2 = pointreport(line.GetEndPoint(1));
            StringBuilder sb = new StringBuilder();
            sb.Append(s1 + "  -  " + s2);
            return sb.ToString();
        }

        public string pointreport(XYZ pt)
        {
            string p1x = Math.Round(pt.X, 2).ToString();
            string p1y = Math.Round(pt.Y, 2).ToString();
            string p1z = Math.Round(pt.Z, 2).ToString();

            StringBuilder sb = new StringBuilder();
            sb.Append("point : (" + p1x + " , " + p1y + " , " + p1z + ") ");
            return sb.ToString();
        }

        public string listreport_Bline(List<bLine> blines)
        {
            StringBuilder sb = new StringBuilder();
            foreach (bLine bl in blines)
            {
                sb.AppendLine(linereport(bl.line) + bl.masterline);
            }
            return sb.ToString();
        }

    }
}
