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
    public class ViperFormData
    {
        public double diameter { get; set; }
        public double height { get; set; }
        public double Thrsh_elbow { get; set; }
        public double Thrsh_straight { get; set; }
        public string pipetype { get; set; }
        public PipeType Rpipetype { get; set; }
        public List<PipeType> pipetypeslist { get; set; }
        public Document doc { get; set; }


        public ViperFormData(Document Doc)
        {
            doc = Doc;
            getpipetypes();


            diameter = .0166666;
            height = 10;
            Thrsh_elbow = .5;
            Thrsh_straight= 2.5;
        }

        public void getpipetypes ()
        {
            FilteredElementCollector col =
                new FilteredElementCollector(doc);
            col.OfClass(typeof(PipeType));

            IEnumerable<PipeType> Types = col.ToElements().Cast<PipeType>();
            List<PipeType> TypesList = new List<PipeType>();

            pipetypeslist = new List<PipeType>();

            foreach (PipeType e in Types)
            {
                if (e == null)
                { }
                else
                {
                    pipetypeslist.Add(e);
                }
            }
        }

    }
}
