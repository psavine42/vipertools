using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Revit.SDK.Samples.UIAPI.CS.V_PipeSplit
{
    public partial class PipeSplitControl : Form
    {
        private PipeSplitData Vpdata;

        public PipeSplitControl(PipeSplitData vpdata)
        {
            InitializeComponent();
            Vpdata = vpdata;

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            double dbl = 0;
            try
            {
                dbl = double.Parse(textBox1.Text);

            }
            catch (Exception)
            {
                //  TaskDialog.Show("asd", "Invalid Height");
            }
            Vpdata.distance = dbl;
        }

      


    }
}
