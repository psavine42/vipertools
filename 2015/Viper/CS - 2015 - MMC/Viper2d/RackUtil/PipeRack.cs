using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Forms;


using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;

using Autodesk.Revit.UI.Selection;
using Revit.SDK.Samples.UIAPI.CS;


using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit.SDK.Samples.UIAPI.CS.Viper2d.RackUtil
{

    //class which holds data for rack object
    public class PipeRack
    {
        public ElementId RackElement { get; set; }
        public List<twopoint> MEPObjects { get; set; }
        public List<TwoPointTree> MEPTrees { get; set; }

        //Initialilze the object
        public PipeRack()
        {


        }


        //
        public void GetPipesSupported()
        {


        }

        //Get the objects hangar is directly supporting
        private void GetDirectlySupportedObjects()
        {
            //GetSupportedbyClash();
            //GetSupportedbyHost();

        }

        ////getobjects by hosting
        //private List<twopoint> GetSupportedbyHost()
        //{



        //}


        ////get objects
        //private List<twopoint> GetSupportedbyClash()
        //{



        //}


    }
}
