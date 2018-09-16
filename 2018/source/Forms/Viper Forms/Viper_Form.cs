﻿using System;
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
    public partial class Viper_Form : System.Windows.Forms.Form
    {
        private ViperFormData vpdata;

 

        public Viper_Form(ViperFormData Vpdata)
        {
            InitializeComponent();
            vpdata = Vpdata;
            this.typeSelection.DataSource = vpdata.pipeTypes;
            this.typeSelection.DisplayMember = "Name";
            // this.typeSelection.SelectedIndex = 0;

            this.systemSelection.DataSource = vpdata.pipeSystems;
            this.systemSelection.DisplayMember = "Name";
            // #this.systemSelection.SelectedIndex = 0;
        }


        private void typeSelected(object sender, EventArgs e)
        {
            vpdata.Rpipetype = (PipeType)typeSelection.SelectedItem;
        }

        private void systemSelected(object sender, EventArgs e)
        {
            vpdata.pipeSystem = (PipingSystemType)typeSelection.SelectedItem;  
        }

        //////////////////////////////////////////////////////////////
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double dbl = 10;
            try
            {
             dbl = Convert.ToDouble(heightBox.Text);
            }
            catch (Exception) {TaskDialog.Show("asd" , "Invalid Height");}
            vpdata.height = dbl;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

            double dbl = 2/12;
            try
            {
                dbl = Convert.ToDouble(diameterBox.Text);
            }
            catch (Exception) { TaskDialog.Show("asd", "Invalid Diameter"); }
            vpdata.diameter = dbl/12;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

            double dbl = 2;
            try
            {
                dbl = Convert.ToDouble(straightThresh.Text);
            }
            catch (Exception) { TaskDialog.Show("asd", "Invalid Diameter"); }
            vpdata.Thrsh_straight = dbl;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

            double dbl = 0.5;
            try
            {
                dbl = Convert.ToDouble(elbowThreshold.Text);
            }
            catch (Exception) { TaskDialog.Show("asd", "Invalid Diameter"); }
            vpdata.Thrsh_elbow = dbl;
        }
    }
}
