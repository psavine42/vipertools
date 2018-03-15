using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using GXYZ = Autodesk.Revit.DB.XYZ;
using System.Windows.Forms;
using Autodesk.Revit.UI.Selection;

namespace Revit.SDK.Samples.UIAPI.CS
{

    class HelperMethods
    {
        /// <summary>
        /// Test the which twopoint element is closest to the clicked point
        /// </summary>
        /// <param name="currentclosest"></param>
        /// <param name="point"></param>
        /// <param name="candidate"></param>
        /// <returns></returns>
        public twopoint testdist(twopoint currentclosest, XYZ point, twopoint candidate)
        {
            if (currentclosest != null)
            {
                XYZ avgptcurrent = new GXYZ((currentclosest.pt1.X + currentclosest.pt2.X) / 2,
                                            (currentclosest.pt1.Y + currentclosest.pt2.Y) / 2,
                                            (currentclosest.pt1.Z + currentclosest.pt2.Z) / 2);

                XYZ avgptcand = new GXYZ((candidate.pt1.X + candidate.pt2.X) / 2,
                                        (candidate.pt1.Y + candidate.pt2.Y) / 2,
                                        (candidate.pt1.Z + candidate.pt2.Z) / 2);

                if (avgptcurrent.DistanceTo(point) >= avgptcand.DistanceTo(point))
                {
                    currentclosest = candidate;

                }

                else { }

            }
            else
            {
                currentclosest = candidate;

            }
            return currentclosest;
        }


        public SketchPlane SetSketchplance(Document doc, Element elem, UIDocument uidoc)
        {
            Transaction tran = new Transaction(doc, "Viper");
            tran.Start();
           // Element elem = doc.GetElement(sheeps.ElementAt(0));
            MEPCurve leadpipe = elem as MEPCurve;
            LocationCurve lc = leadpipe.Location as LocationCurve;
            XYZ base1 = lc.Curve.GetEndPoint(1);
            Plane plane = new Plane(uidoc.ActiveView.ViewDirection, base1);
            SketchPlane sp = SketchPlane.Create(doc, plane);
            uidoc.ActiveView.SketchPlane = sp;
            uidoc.ActiveView.HideActiveWorkPlane();
            tran.Commit();
            return sp;
        }

    }

}
