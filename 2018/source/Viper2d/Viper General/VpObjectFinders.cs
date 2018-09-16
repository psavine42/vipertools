using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using System.Linq;
using System.Diagnostics;

namespace Viper
{
    class VpObjectFinders
    {
        public List<Connector> allconnectors(List<TwoPoint> pipelist)
        {
            List<Connector> allconector = new List<Connector>();
            foreach (TwoPoint tp in pipelist)
            {
                Connector c1;
                Connector c2;
                if (tp.Mepcurve != null)
                {
                    GetPipeconnectors(tp.Mepcurve, out c1, out c2);
                    allconector.Add(c1); allconector.Add(c1);
                }
                else if (tp.pipe != null)
                {
                    GetPipeconnectors(tp.pipe, out c1, out c2);
                    allconector.Add(c1); allconector.Add(c1);
               
                }
            }
            return allconector;
        }

        public static List<Element> AllMEPCurves(Document doc)
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

        public static List<Element> AllHostableHorizantal(Document doc)
        {
            //GET FLOORS and other DATA*******	    
            FilteredElementCollector fFilter = new FilteredElementCollector(doc, doc.ActiveView.Id);
            ElementCategoryFilter flrs = new ElementCategoryFilter(BuiltInCategory.OST_Floors);
            ElementCategoryFilter roof = new ElementCategoryFilter(BuiltInCategory.OST_Roofs);
            ElementCategoryFilter fnds = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation);
            // ElementCategoryFilter walls = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
            List<ElementFilter> filterlist = new List<ElementFilter>() { flrs, fnds, roof };
            LogicalOrFilter f1 = new LogicalOrFilter(filterlist);
            List<Element> forsl = fFilter.WherePasses(f1).ToElements().ToList();
            return forsl;
        }

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
        public static void GetPipeconnectors(MEPCurve pp, out Connector o1, out Connector o2)
        {
            o1 = pp.ConnectorManager.Lookup(0);
            o2 = pp.ConnectorManager.Lookup(1);
        }

        public static FamilySymbol GetGenericfam(Document doc, string name)
        {
            //FilteredElementCollector gFilter = new FilteredElementCollector(doc);
            //ICollection<Element> z = gFilter
            //    .OfClass(typeof(FamilySymbol))
            //    .OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
            //List<Element> nl = new List<Element>();
            //foreach (Element e in z)
            //{
            //    FamilySymbol bl = e as FamilySymbol;
            //    if (bl.Name == name)
            //    {
            //        nl.Add(e);
            //    }
            //}
            //FamilySymbol s = nl.ElementAt(0) as FamilySymbol;
            //return s;
            return GetGenericfam(doc, BuiltInCategory.OST_GenericModel, name);
        }

        public static FamilySymbol GetGenericfam(Document doc, BuiltInCategory category, string name)
        {
            var gFilter = (new FilteredElementCollector(doc))
                .WhereElementIsElementType()
                .OfCategory(category)
                .Cast<FamilySymbol>()
                .Where(x => x.Name == name)
                .ToList();
              
            FamilySymbol s = gFilter.ElementAt(0) as FamilySymbol;
            return s;
        }

        private static FilteredElementCollector CategoryCol(Document doc, BuiltInCategory cat)
        {
            return new FilteredElementCollector(doc)
                        .WhereElementIsElementType()
                        .OfCategory(cat);
                       
        }

        private static FilteredElementCollector CategoryCol<T>(Document doc)
        {
            return new FilteredElementCollector(doc)
                        .WhereElementIsElementType()
                        .OfClass(typeof(T));

        }

        public static List<FamilySymbol> FindFamilyTypes(Document doc, BuiltInCategory cat)
        {
            return CategoryCol(doc, cat).Cast<FamilySymbol>().ToList();
        }

        public static List<T> FindFamilyTypes<T>(Document doc)
        {
            return CategoryCol<T>(doc).ToElements().Cast<T>().ToList();
        }

        public static List<FamilySymbol> FindFamilyTypes(Document doc, BuiltInCategory cat, string name)
        {
            var items = FindFamilyTypes(doc, cat);
            Debug.WriteLine(items[0].FamilyName);   /// testing
            Debug.WriteLine(items[0].Name);
            Debug.WriteLine(items[0].Family.Name);
            return   items.Where(x => x.Name.Contains(name)).ToList();
        }

        public static List<Element> GetGenericfams(Document doc, string name)
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
            
            var fec = new FilteredElementCollector(doc)
                .OfClass(typeof(PipeInsulation))
                .Cast<PipeInsulation>()
                .Where(x => x.HostElementId == pipe.Id);
            return fec.FirstOrDefault();
        }

        public PipeInsulation GetPipeInslationGeneral(Element pipe)
        {
            return GetPipeInslationFromPipe((Pipe)pipe);
        }


        public DuctInsulation GetPipeInslationFromDuct(Duct pipe)
        {
            if (pipe == null)
            {
                throw new ArgumentNullException("pipe");
            }
            Document doc = pipe.Document;
            FilteredElementCollector fec = new FilteredElementCollector(doc).OfClass(typeof(DuctInsulation));
            DuctInsulation pipeInsulation = null;

            foreach (DuctInsulation pi in fec)
            {
                if (pi.HostElementId == pipe.Id)
                    pipeInsulation = pi;
            }

            if (pipeInsulation != null)
                return pipeInsulation;
            else
                return null;
        }
    }
}
