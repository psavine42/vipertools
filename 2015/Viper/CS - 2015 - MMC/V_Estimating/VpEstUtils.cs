using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI.Selection;
using Revit.SDK.Samples.UIAPI.CS.Viper2d;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace Revit.SDK.Samples.UIAPI.CS.V_Estimating
{
    class VpEstUtils
    {

        public void NetWallArea(Document doc)
        {
            foreach (Wall w in new FilteredElementCollector(doc).OfClass(typeof(Wall)).Cast<Wall>())
            {
                // get a reference to one of the wall's side faces
                Reference sideFaceRef = HostObjectUtils.GetSideFaces(w, ShellLayerType.Exterior).First();

                // get the geometry object associated with that reference
                Face netFace = w.GetGeometryObjectFromReference(sideFaceRef) as Face;

                // get the area of the face - this area does not include the area of the inserts that cut holes in the face
                double netArea = netFace.Area;

                double grossArea;
                using (Transaction t = new Transaction(doc, "delete inserts"))
                {
                    t.Start();

                    // delete all family inserts that are hosted by this wall
                    foreach (FamilyInstance fi in new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).Cast<FamilyInstance>().Where(q => q.Host != null && q.Host.Id == w.Id))
                    {
                        doc.Delete(fi.Id);
                    }
                    // regenerate the model to update the geometry with the inserts deleted
                    doc.Regenerate();

                    // get the gross area (area of the wall face now that the inserts are deleted)
                    Face grossFace = w.GetGeometryObjectFromReference(sideFaceRef) as Face;
                    grossArea = grossFace.Area;

                    // rollback the transaction to restore the model to its original state
                    t.RollBack();
                }
              //  TaskDialog.Show("Areas", "Net = " + netArea + "\nGross = " + grossArea);
            }
        }
    }
}
