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
    public partial class BlockMapForm : System.Windows.Forms.Form
    {
        public BlockmapperFormData bmfd;
        public RevitgetUtils rvtu;

        public BlockMapForm(BlockmapperFormData Bmfd)
        {
            InitializeComponent();
            bmfd = Bmfd;

            List<FormtoRevitObject> cats = new List<FormtoRevitObject>();
            List<FormtoRevitObject> hostcats = new List<FormtoRevitObject>();

            FilteredElementCollector FamilyCollector
               = new FilteredElementCollector(bmfd.doc)
               .OfClass(typeof(FamilySymbol));
            List<ElementId> iter = FamilyCollector.ToElementIds().Distinct().ToList();
            

            List<Category> catlist = new List<Category>();

            List<ElementId> iter2 = iter.Distinct().ToList();

            foreach (ElementId fe in iter2)
            {
                Category cr = bmfd.doc.GetElement(fe).Category;
                catlist.Add(cr); 
            }


            foreach (Category cat in catlist.Distinct().ToList())
            {

                FormtoRevitObject objlev = new FormtoRevitObject(cat, cat.Name);
                if (cat.Name == "Ceilings" || cat.Name == "Walls" || cat.Name == "Floors")
                { hostcats.Add(objlev); }
                else
                {
                    cats.Add(objlev);
                }
            }
            
            BoxCategory.DataSource = cats;
            BoxCategory.SelectedIndex.Equals(0);
            BoxCategory.DisplayMember = "formname";

            //set thing to host on
            BoxHostedBy.Items.Add("Unhosted");
            BoxHostedBy.DataSource = hostcats;
            BoxHostedBy.DisplayMember = "formname";

            //set the base level and populate the level box
            List<Level> levels = new FilteredElementCollector(bmfd.doc)
             .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();
            List<FormtoRevitObject> Levels = new List<FormtoRevitObject>();

            foreach (Level lev in levels)
            {
                FormtoRevitObject objlev = new FormtoRevitObject(lev, lev.Name);
                Levels.Add(objlev);
            }

            BoxBaseLevel.DataSource = Levels;
            BoxBaseLevel.DisplayMember = "formname";
            BoxBaseLevel.SelectedIndex.Equals(0);

            //set direction  to cast vector : above/below
            BoxAboveBelow.Items.Add("above");
            BoxAboveBelow.Items.Add("below");

            //

        }


        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            FormtoRevitObject fcat = BoxCategory.SelectedItem as FormtoRevitObject;
            Category rcat = fcat.revitcategory;
            
            List<FormtoRevitObject> famsl = new List<FormtoRevitObject>();
            FilteredElementCollector FamilyCollector
               = new FilteredElementCollector(bmfd.doc)
               .OfClass(typeof(FamilySymbol))
               .WherePasses(new ElementCategoryFilter(rcat.Id));
            ICollection<ElementId> iter = FamilyCollector.ToElementIds();

            foreach (ElementId id in iter)
            {
                FamilySymbol f = bmfd.doc.GetElement(id) as FamilySymbol;
                if (f != null)
                {

                    FormtoRevitObject objlev = new FormtoRevitObject
                        (f, f.Family.Name + " : " + f.Name);
                    famsl.Add(objlev);
                }
            }

            BoxFamily.DataSource = famsl;
            BoxFamily.DisplayMember = "formname";
            BoxFamily.SelectedIndex.Equals(0);
            bmfd.Vcategory = rcat;

        }

        private void comboBox2_TextChanged(object sender, EventArgs e)
        {
            FormtoRevitObject fsym = BoxFamily.SelectedItem as FormtoRevitObject;
            bmfd.Vfamily = fsym.revitobj as FamilySymbol;
        }


        private void BlockMapForm_Load(object sender, EventArgs e)
        {

        }


        private void BoxBaseLevel_TextChanged(object sender, EventArgs e)
        {
            //set new level on level changed event;
           string dbl = BoxBaseLevel.SelectedText;
           List<Level> levels = new FilteredElementCollector(bmfd.doc)
          .OfClass(typeof(Level)).Cast<Level>().Where(l => l.Name.Equals(dbl)).ToList();
           FormtoRevitObject cadlev = 
               new FormtoRevitObject(levels.FirstOrDefault(), levels.FirstOrDefault().Name);

           bmfd.cadlevel = cadlev;
        }

        private void BoxAboveBelow_TextChanged(object sender, EventArgs e)
        {
            //set new level on level changed event;
          
         //   bmfd.cadlevel = cadlev;
        }

        private void BoxHostedBy_TextChanged(object sender, EventArgs e)
        {
            //set new level on level changed event;


            //   bmfd.cadlevel = cadlev;
        }



    }


}
