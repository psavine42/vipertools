using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using GXYZ = Autodesk.Revit.DB.XYZ;
using System.Windows.Forms;
using Autodesk.Revit.DB.Structure;



namespace Revit.SDK.Samples.UIAPI.CS.Hangers
{
    class Dimensionista
    {

    //    public void diminsionpipes(Document doc, List<twopoint> pipes)
    //    {

    //      //  XYZ location1 = GeomUtils.kOrigin;
    //        XYZ location2 = new XYZ(20.0, 0.0, 0.0);
    //        XYZ location3 = new XYZ(0.0, 20.0, 0.0);
    //        XYZ location4 = new XYZ(20.0, 20.0, 0.0);


    //        DetailCurve dCurve1 = null;
    //        DetailCurve dCurve2 = null;

     
    //        if (null != doc.OwnerFamily
    //              && null != doc.OwnerFamily.FamilyCategory)
    //            {
    //                if (!doc.OwnerFamily.FamilyCategory.Name
    //                  .Contains("Detail"))
    //                {
    //                    MessageBox.Show(
    //                      "Please make sure you open a detail based family template.",
    //                      "RevitLookup", MessageBoxButtons.OK,
    //                      MessageBoxIcon.Information);

    //                    return;
    //                }
    //            }

    //            dCurve1 = doc.FamilyCreate.NewDetailCurve(doc.ActiveView, curve1);
    //            dCurve2 = doc.FamilyCreate.NewDetailCurve( doc.ActiveView, curve2);
            

    //      //  Line line = m_revitApp.Application.Create.NewLine(location2, location4, true);

    //        ReferenceArray refArray = new ReferenceArray();

    //        refArray.Append(dCurve1.GeometryCurve.Reference);
    //        refArray.Append(dCurve2.GeometryCurve.Reference);

    //        if (!doc.IsFamilyDocument)
    //        {
    //            doc.Create.NewDimension(
    //            doc.ActiveView, line, refArray);
    //        }
    //        else
    //        {
    //            doc.FamilyCreate.NewDimension(
    //              doc.ActiveView, line, refArray);
    //        }
    //    }



    }
}
