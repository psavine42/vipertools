using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;


using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
namespace Revit.SDK.Samples.UIAPI.CS
{
    public class VPipeEndFormData
    {

        public double diameter { get; set; }
        public double topoffset { get; set; }
        public double bottomoffset { get; set; }

        public string bottomlev { get; set; }
        public string toplev { get; set; }

        public Level toplevel { get; set; }
        public Level bottomlevel { get; set; }
        public List<Level> toplevels { get; set; }
        public List<Level> bottomlevels { get; set; }
        public Document doc { get; set; }


        public VPipeEndFormData(Document Doc)
        {
            doc = Doc;
            getlevels();
  

        }

        public void getlevels()
        {
            FilteredElementCollector col =
             new FilteredElementCollector(doc);
            col.OfClass(typeof(Level));

            IEnumerable<Level> Types = col.ToElements().Cast<Level>();
            List<Level> TypesList = new List<Level>();

            List<Level> levels = new FilteredElementCollector(doc)
            .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();

            toplevels = new List<Level>();
            bottomlevels = new List<Level>();


            foreach (Level lev in levels)
            {
                toplevels.Add(lev);
                bottomlevels.Add(lev);
            }


        }

    }
}
