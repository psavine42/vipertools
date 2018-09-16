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

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

namespace Viper.Forms
{
    public partial class PipeBlockMapForm : System.Windows.Forms.Form
    {

        public BlockmapperFormData bmfd;
        public RevitgetUtils rvtu;

        public PipeBlockMapForm(BlockmapperFormData Bmfd)
        {
            InitializeComponent();
            bmfd = Bmfd;
            
            //get the current and level above 
            List<Level> levels = new FilteredElementCollector(bmfd.doc)
            .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();

            List<PipeType> pipetypes = getpipetypes();
            List<FormtoRevitObject> fpipetypes = new List<FormtoRevitObject>();
            List<FormtoRevitObject> flevels = new List<FormtoRevitObject>();

            foreach (PipeType pt in pipetypes)
            {
                FormtoRevitObject obj= new FormtoRevitObject(pt, pt.Name);
                fpipetypes.Add(obj);
            }

            foreach (Level pt in levels)
            {
                FormtoRevitObject obj = new FormtoRevitObject(pt, pt.Name);
                flevels.Add(obj);
                if(bmfd.cadlevel.formname == obj.formname)
                {
                    int indx = levels.IndexOf(pt) + 1;
                    FormtoRevitObject obj2 =
                        new FormtoRevitObject(levels.ElementAt(indx), levels.ElementAt(indx).Name);
                    bmfd.toplevel = obj2; 
                }
            }

            BottomLevel.DataSource = flevels;
            BottomLevel.SelectedItem = bmfd.cadlevel;
            BottomLevel.DisplayMember = "formname";

            TopLevel.DataSource = flevels;
            TopLevel.SelectedItem = bmfd.toplevel;
            TopLevel.DisplayMember = "formname";

            Pipetype.DataSource = fpipetypes;
            Pipetype.SelectedIndex = 0;
            Pipetype.DisplayMember = "formname";

            bmfd.bottomoffset = 0;
            bmfd.topoffset = 0;
        }

        private List<PipeType> getpipetypes()
        {
            FilteredElementCollector col =
                new FilteredElementCollector(bmfd.doc).OfClass(typeof(PipeType));
            IEnumerable<PipeType> Types = col.ToElements().Cast<PipeType>();

            List<PipeType> pipetypeslist = new List<PipeType>();

            foreach (PipeType e in Types)
            {
                if (e == null)
                { }
                else
                {
                    pipetypeslist.Add(e);
                }
            }
            return pipetypeslist;
        }

        private void Pipetype_TextChanged(object sender, EventArgs e)
        {
            bmfd.Vtype = Pipetype.SelectedItem as FormtoRevitObject;
        }

        private void TopLevel_TextChanged(object sender, EventArgs e)
        {
            bmfd.toplevel = TopLevel.SelectedItem as FormtoRevitObject;
        }

        private void BottomLevel_TextChanged(object sender, EventArgs e)
        {
            bmfd.cadlevel = BottomLevel.SelectedItem as FormtoRevitObject;
        }

        private void TopOffset_TextChanged(object sender, EventArgs e)
        {
            double dbl = bmfd.topoffset;
            try
            { dbl = double.Parse(TopOffset.Text); }
            catch (Exception) { }
            bmfd.topoffset = dbl;
        }
        private void BottomOffset_TextChanged(object sender, EventArgs e)
        {
            double dbl = bmfd.bottomoffset;
            try
            {  dbl = double.Parse(BottomOffset.Text);  }
            catch (Exception){  }
            bmfd.bottomoffset = dbl;
        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
