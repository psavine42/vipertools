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


namespace Viper.Forms
{
    public partial class PipeEnds : System.Windows.Forms.Form
    {
        private VPipeEndFormData vpdata;

        public PipeEnds(VPipeEndFormData Vpdata)
        {
            InitializeComponent();
            vpdata = Vpdata;

            foreach (Level pt in vpdata.bottomlevels)
            {
                comboBox1.Items.Add(pt.Name);

            }
            foreach (Level pt2 in vpdata.bottomlevels)
            {
                comboBox2.Items.Add(pt2.Name);

            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double dbl = 0;
            try
            {
                 dbl = double.Parse(textBox1.Text);
                
            }
            catch (Exception) { 
              //  TaskDialog.Show("asd", "Invalid Height");
            }
            vpdata.topoffset = dbl;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            double dbl = 0;
            try
            {
                dbl = double.Parse(textBox2.Text);
            }
            catch (Exception) { 
               // TaskDialog.Show("asd", "Invalid Height"); 
            }
            vpdata.bottomoffset = dbl;
        }

        private void comboBox1_DisplayMemberChanged(object sender, EventArgs e)
        {
            vpdata.bottomlev = comboBox1.Text;

            foreach (Level pt in vpdata.bottomlevels)
            {
                if (pt.Name == vpdata.bottomlev)
                {
                    vpdata.bottomlevel = pt;
                }
                else { }
            }
           
        }


        private void comboBox2_DisplayMemberChanged(object sender, EventArgs e)
        {
            vpdata.toplev = comboBox2.Text;

            foreach (Level pt in vpdata.toplevels)
            {
                if (pt.Name == vpdata.toplev)
                {
                    vpdata.toplevel = pt;
                }
                else { }
            }
        }





    }
}
