using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Forms;
using Autodesk.Revit.DB.Plumbing;
using GXYZ = Autodesk.Revit.DB.XYZ;
using Autodesk.Revit.UI.Selection;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit.SDK.Samples.UIAPI.CS
{
    public class RevitgetUtils
    {

       public List<Category> Vgetcategorieslist(Document doc)
        {
            List<Category> list = new FilteredElementCollector(doc)
            .OfClass(typeof(BuiltInCategory)).Cast<Category>().OrderBy(l => l.Name).ToList();
            return list;
       }

       public List<FamilySymbol> Vgettypeslist(Document doc)// , string s)
        {
            //List<FamilySymbol> list = new List<FamilySymbol>();

            //List<FamilySymbol> list = new FilteredElementCollector(doc)
            //    .WherePasses(new ElementClassFilter(typeof(FamilySymbol)))
            //    //.WherePasses(new ElementCategoryFilter(cat))
            //      .Cast<FamilySymbol>().ToList();
                 // .OrderBy(e => e.Name).ToList();

            List<Element> list = new List<Element>();
            FilteredElementCollector collector = 
                new FilteredElementCollector(doc);

            ICollection<Element> collection =
            collector.OfClass(typeof(Family)).ToElements();

           // ElementClassFilter FamilyFilter = new ElementClassFilter(typeof(Family));
            FilteredElementCollector FamilyCollector 
                = new FilteredElementCollector(doc).OfClass(typeof(FamilySymbol));

            ICollection<ElementId> iter = FamilyCollector.ToElementIds();

            List<FamilySymbol> famsyms = new List<FamilySymbol>();

           
                foreach (ElementId e in iter)
                {
                     try
                    {
                    FamilySymbol f = doc.GetElement(e) as FamilySymbol;
                    if (f != null)
                    {
                        famsyms.Add(f);
                    }
                    }
                     catch (Exception) { }
                }
            
            //return listout;
            return famsyms;

        }

       public FamilySymbol Vgetsymbol(Document doc, string s)
       {
           List<FamilySymbol> list = new FilteredElementCollector(doc)
           .OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
           .Where(l => l.Name.Equals(s)).ToList();
           FamilySymbol fs = list.FirstOrDefault();
           return fs;

       }

    }

    public class FormtoRevitObject
    {
        public Element revitobj { get; set; }
        public string formname { get; set; }
        public Category revitcategory { get; set; }

        public FormtoRevitObject(Element Revitobj, string Formname)
        {
            revitobj = Revitobj;
            formname = Formname;
        }
        

        public FormtoRevitObject(Category Revitcategory, string Formname)
        {
            revitcategory = Revitcategory;
            formname = Formname;

        }



    }




}
