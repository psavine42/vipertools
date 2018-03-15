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
    public partial class Viper_Form : System.Windows.Forms.Form
    {
        private ViperFormData vpdata;

        public Viper_Form(ViperFormData Vpdata)
        {
            InitializeComponent();
            vpdata = Vpdata;
            

            foreach(PipeType pt in vpdata.pipetypeslist)
            {
                comboBox1.Items.Add(pt.Name);
            }
            comboBox1.SelectedIndex = 0;
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double dbl = 10;
            try
            {
             dbl = Convert.ToDouble(textBox1.Text);
            }
            catch (Exception) {TaskDialog.Show("asd" , "Invalid Height");}
            vpdata.height = dbl;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

            double dbl = 2/12;
            try
            {
                dbl = Convert.ToDouble(textBox2.Text);
            }
            catch (Exception) { TaskDialog.Show("asd", "Invalid Diameter"); }
            vpdata.diameter = dbl/12;
        }


        private void textBox3_TextChanged(object sender, EventArgs e)
        {

            double dbl = 2;
            try
            {
                dbl = Convert.ToDouble(textBox3.Text);
            }
            catch (Exception) { TaskDialog.Show("asd", "Invalid Diameter"); }
            vpdata.Thrsh_straight = dbl;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

            double dbl = 0.5;
            try
            {
                dbl = Convert.ToDouble(textBox4.Text);
            }
            catch (Exception) { TaskDialog.Show("asd", "Invalid Diameter"); }
            vpdata.Thrsh_elbow = dbl;
        }



        private void comboBox1_DisplayMemberChanged(object sender, EventArgs e)
        {
          //  TaskDialog.Show("asd", comboBox1.Text);
            vpdata.pipetype = comboBox1.Text;

            foreach(PipeType pt in vpdata.pipetypeslist)
            {
                if (pt.Name == vpdata.pipetype)
                {
                    vpdata.Rpipetype = pt;
                }
                else { }
            }
           

        }

   
    }
}
