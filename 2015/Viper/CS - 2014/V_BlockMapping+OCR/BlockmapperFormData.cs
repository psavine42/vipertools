using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
namespace Revit.SDK.Samples.UIAPI.CS
{
    public class BlockmapperFormData
    {
        public Category Vcategory { get; set; }
        public FamilySymbol Vfamily { get; set; }
        public List<Category> Lcategory { get; set; }
        public Document doc { get; set; }
        public FormtoRevitObject hostclass { get; set; }
        public string abovebelow { get; set; }
        public FormtoRevitObject cadlevel { get; set; }
        public FormtoRevitObject toplevel { get; set; }
        public FormtoRevitObject Vtype { get; set; }
        public double topoffset { get; set; }
        public double bottomoffset { get; set; }

        public BlockmapperFormData(Document Doc, Level Lev)
        {
            doc = Doc;
            FormtoRevitObject lvl = new FormtoRevitObject(Lev, Lev.Name);
            cadlevel = lvl;


        }





    }
}
