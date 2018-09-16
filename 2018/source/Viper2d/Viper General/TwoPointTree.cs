using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Data;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;

using Autodesk.Revit.UI.Selection;


using Autodesk.Revit.DB;
using Autodesk.Revit.UI;



namespace Viper
{


    /// <summary>
    /// Project tree is a class that is more flexible than the MEPsystem class.
    /// The purpose is to 
    /// </summary>
    public class ProjectTree
    {
        public List<TwoPointTree> ProjectMEPTree { get; set; }
        private VpObjectFinders vfo = new VpObjectFinders();
        public List<TwoPoint> unclassifiedtps { get; set; }
        public StringBuilder sb = new StringBuilder();
        public Document doc { get; set; }
        //private var st = Dictionary<XYZ, HashSet<ElementId>>();

        // Build project Tree From Sratch
        public ProjectTree(Document Doc)
        {
            this.doc = Doc;
            this.ProjectMEPTree = new List<TwoPointTree>();

           // VpObjectFinders vfo = new VpObjectFinders();
            List<Element> allmepcurves = VpObjectFinders.AllMEPCurves(doc);
            this.unclassifiedtps = new List<TwoPoint>();

            foreach (Element e in allmepcurves)
            {
                MEPCurve mep = e as MEPCurve;
                // allmeps.Add(mep);
                Curve lc = (mep.Location as LocationCurve).Curve;
                TwoPoint tp = new TwoPoint(lc.GetEndPoint(0), lc.GetEndPoint(1), mep);
                this.unclassifiedtps.Add(tp);
            }
        }
        // Build project Tree with Existing twopointtrees
        public ProjectTree(List<TwoPointTree> tree)
        {
            this.ProjectMEPTree = tree;
        }


        public void Add(TwoPoint twopnt)
        {

        }


    }

    //Class to hold a startpoint of tree
    public class TwoPointTree
    {
        public TwoPoint StartObject { get; set; }
        public Document doc { get; set; }
        public ProjectTree projtree { get; set; }
        public LinkedList<TwoPoint> treelist { get; set; } 
 

        public TwoPointTree(TwoPoint start,  ProjectTree Projtree)
        {
            this.StartObject = start;
            this.doc = Projtree.doc;
            this.projtree = Projtree;
            projtree.unclassifiedtps.Remove(start);
        }

        public void BuildTraverse()
        {
            projtree.sb.AppendLine("NEW Tree");
            StartObject.tp_BuildTraverse(projtree);
        }

        public void LookupTraverse()
        {
            while (StartObject.tp_child.Count > 0)
            {
                //Do Stuff
            }
            //StartObject.tp_child();

        }

    }
}
