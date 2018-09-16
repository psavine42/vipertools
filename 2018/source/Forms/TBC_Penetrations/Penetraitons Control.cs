using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB.Plumbing;
using GXYZ = Autodesk.Revit.DB.XYZ;
using Autodesk.Revit.UI.Selection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RView = Autodesk.Revit.DB.View;
using RApplication = Autodesk.Revit.ApplicationServices.Application;

namespace Viper.Forms
{
    public partial class Penetraitons_Control : System.Windows.Forms.Form
    {


        private static Autodesk.Revit.DB.Document doc;
       // public static Penetrations pnc;
        private Pendata pendata;
     

        public Penetraitons_Control(Pendata Pnc, Document Doc) //,  Penetrations Pnc)
        {
            InitializeComponent();
            pendata = Pnc;
            doc = Doc;
            updateListbox();


        }
    

     private void updateListbox()
      {

        //  ListView.ListViewItemCollection collection = listBox1.Items;
        //  collection.Clear();

        FilteredElementCollector links = 
         new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks);

        foreach (Element e in links)
        {
            RevitLinkInstance link = e as RevitLinkInstance;
            if (link == null)   {  }
            else
            {
                listBox1.Items.Add(link.Name.ToString());
            }

        }

      }


     private void button1_MouseClick(object sender, EventArgs e)
     {

         string filename = listBox1.SelectedItem.ToString();
         pendata.filename = filename;

     }



    }





}
