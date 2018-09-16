using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Viper
{
    class HelperMethods
    {

        public static TwoPoint testdist(TwoPoint currentclosest, XYZ point, TwoPoint candidate)
        {
            if (currentclosest != null)
            {
                XYZ avgptcurrent = new XYZ((currentclosest.pt1.X + currentclosest.pt2.X) / 2,
                                            (currentclosest.pt1.Y + currentclosest.pt2.Y) / 2,
                                            (currentclosest.pt1.Z + currentclosest.pt2.Z) / 2);
                XYZ avgptcand = new XYZ((candidate.pt1.X + candidate.pt2.X) / 2,
                                        (candidate.pt1.Y + candidate.pt2.Y) / 2,
                                        (candidate.pt1.Z + candidate.pt2.Z) / 2);
                if (avgptcurrent.DistanceTo(point) >= avgptcand.DistanceTo(point))
                {
                    currentclosest = candidate;
                }
            }
            else
            {
                currentclosest = candidate;
            }
            return currentclosest;
        }


        public static Line ExtendLine(Line l, double dists, double diste)
        {
            XYZ p1 = l.GetEndPoint(0) - l.Direction * dists;
            XYZ p2 = l.GetEndPoint(1) + l.Direction * diste;
            Line extend = Line.CreateBound(p1, p2);
            return extend;
        }

        public static SketchPlane SetSketchplance(Document doc, Element elem, UIDocument uidoc)
        {
            Transaction tran = new Transaction(doc, "Viper");
            tran.Start();
            MEPCurve leadpipe = elem as MEPCurve;
            LocationCurve lc = leadpipe.Location as LocationCurve;
            XYZ base1 = lc.Curve.GetEndPoint(1);
            Plane plane = Plane.CreateByNormalAndOrigin(XYZ.BasisZ, XYZ.Zero);
            SketchPlane sp = SketchPlane.Create(doc, plane);
            
            uidoc.ActiveView.SketchPlane = sp;
            // uidoc.ActiveView.HideActiveWorkPlane();
            tran.Commit();
            return sp;
        }

    }

}
