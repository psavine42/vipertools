using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Mechanical;
using GXYZ = Autodesk.Revit.DB.XYZ;
using System.Windows.Forms;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using System.Linq;


namespace Revit.SDK.Samples.UIAPI.CS
{
    class VpObjectFinders
    {
        public List<Connector> allconnectors(List<twopoint> pipelist)
        {
            List<Connector> allconector = new List<Connector>();
            foreach (twopoint tp in pipelist)
            {
                if (tp.Mepcurve != null)
                {
                    allconector.AddRange(GetPipeconnectors(tp.Mepcurve));
                }
                else if (tp.pipe != null)
                {
                    allconector.AddRange(GetPipeconnectors(tp.pipe));
                }
            }

            return allconector;
        }

        //Get connectors from a pipe
        public List<Connector> GetPipeconnectors(MEPCurve pp)
        {
            List<Connector> allconector = new List<Connector>();
            ConnectorSetIterator csi = pp.ConnectorManager.Connectors.ForwardIterator();
            csi.MoveNext();
            Connector con1 = csi.Current as Connector;
            csi.MoveNext();
            Connector con2 = csi.Current as Connector;
            allconector.Add(con1);
            allconector.Add(con2);
            return allconector;
        }



        public List<Element> GetGenericfams(Document doc, string name)
        {
            FilteredElementCollector gFilter = new FilteredElementCollector(doc);
            ICollection<Element> z = gFilter
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
            List<Element> nl = new List<Element>();

            foreach (Element e in z)
            {
                Element elemtype = doc.GetElement(e.GetTypeId());
                if (elemtype.Name == name)
                {
                    nl.Add(e);
                }
            }
            return nl;
        }

    }
}
