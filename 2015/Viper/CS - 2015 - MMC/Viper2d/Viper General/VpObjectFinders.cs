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


        public List<Element> AllMEPCurves(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            ElementClassFilter pFilter = new ElementClassFilter(typeof(Pipe), false);
            ElementClassFilter mFilter = new ElementClassFilter(typeof(Duct), false);
            ElementClassFilter cobFilter = new ElementClassFilter(typeof(Conduit), false);
            List<ElementFilter> filterlistp = new List<ElementFilter>() { pFilter, mFilter, cobFilter };
            LogicalOrFilter mepfilter = new LogicalOrFilter(filterlistp);
            List<Element> pps = collector.WherePasses(mepfilter).ToElements().ToList();
            return pps;

        }


        /// <Return the level below an object point>
        /// Return the level below an object point - 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Level GetLevelBelow(XYZ point, Document doc)
        {
            List<Level> levels = new FilteredElementCollector(doc)
            .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();
            Level levout = levels.FirstOrDefault();

            foreach (Level e in levels)
            {
                Level lev = e as Level;
     
                if (point.Z > e.Elevation)
                {
                  levout = e;
                  break;
                 }
            }
            return levout;
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


        public FamilySymbol GetGenericfam(Document doc, string name)
        {
            FilteredElementCollector gFilter = new FilteredElementCollector(doc);
            ICollection<Element> z = gFilter
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
            List<Element> nl = new List<Element>();//= new FamilySymbol(); // 

            foreach (Element e in z)
            {
                FamilySymbol bl = e as FamilySymbol;
                if (bl.Name == name)
                {
                    nl.Add(e);
                }
            }
            FamilySymbol s = nl.ElementAt(0) as FamilySymbol;
            return s;
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


        public double GetParamAsDouble(Element e, string param)
        {
            return e.LookupParameter(param).AsDouble();
        }




        public PipeInsulation GetPipeInslationFromPipe(Pipe pipe)
        {
            if (pipe == null)
            {
                throw new ArgumentNullException("pipe");
            }
            Document doc = pipe.Document;
            FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(PipeInsulation));
            PipeInsulation pipeInsulation = null;

            foreach (PipeInsulation pi in fec)
            {
                if (pi.HostElementId == pipe.Id)
                    pipeInsulation = pi;
            }
            if (pipeInsulation != null)
                return pipeInsulation;
            else return null;
        }

        public PipeInsulation GetPipeInslationGeneral(Element pipe)
        {


            if (pipe == null)
            {
                throw new ArgumentNullException("pipe");
            }

            //InsulationLiningBase.
            Document doc = pipe.Document;
            FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(PipeInsulation));
            PipeInsulation pipeInsulation = null;

            foreach (PipeInsulation pi in fec)
            {
                if (pi.HostElementId == pipe.Id)
                    pipeInsulation = pi;
            }
            if (pipeInsulation != null)
                return pipeInsulation;
            else return null;
        }



        public DuctInsulation GetPipeInslationFromDuct(Duct pipe)
        {
            if (pipe == null)
            {
                throw new ArgumentNullException("pipe");
            }
            Document doc = pipe.Document;
            FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(PipeInsulation));
            DuctInsulation pipeInsulation = null;

            foreach (DuctInsulation pi in fec)
            {
                if (pi.HostElementId == pipe.Id)
                    pipeInsulation = pi;
            }

            if (pipeInsulation != null)
                return pipeInsulation;
            else return null;
        }


    }
}
