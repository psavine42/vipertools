using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI.Selection;


using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Viper
{

    public enum HeadType { UP, Down, Horizantal }

    class DisplayMember
    {
        ElementId eid;
        string Name;
        public DisplayMember(PipingSystemType systemtype)
        {
            Name = systemtype.SystemClassification.ToString();
            eid = systemtype.Id;
        }

        public Element Recover(Document doc)
        {
            return doc.GetElement(eid);
        }
    }

    public class ViperFormData
    {
        public double diameter { get; set; }
        public double height { get; set; }
        public double Thrsh_elbow { get; set; }
        public double Thrsh_straight { get; set; }
        // public string pipetype { get; set; }

        
        public FamilySymbol upHead { get; set;  }
        public FamilySymbol vertHead { get; set; }
        public FamilySymbol downHead { get; set; }

        public PipingSystemType pipeSystem { get; set; }
        public PipeType Rpipetype { get; set; }

        public List<PipeType> pipeTypes { get; set; }
        public List<PipingSystemType> pipeSystems { get; set; }
        public List<FamilySymbol> sprinklerHeads { get; set; }
        public Document doc { get; set; }
        public List<XYZ> points = new List<XYZ>();
        public ElementId level { get; set; }

        public ViperFormData(Document Doc)
        {
            doc = Doc;
            getpipetypes();

            diameter = .0166666;
            height = 10;
            Thrsh_elbow = .5;
            Thrsh_straight= 2.5;
        }

        public Dictionary<string, string> serialize()
        {
            var dict = new Dictionary<string, string>();
            dict["diameter"] = diameter.ToString();
            return dict;
        }
        
        public void getpipetypes()
        {
            var col = new FilteredElementCollector(doc).OfClass(typeof(PipeType));
            pipeTypes = col.ToElements().Cast<PipeType>().ToList();

            var col2 = new FilteredElementCollector(doc).OfClass(typeof(PipingSystemType));
            pipeSystems = col2.ToElements().Cast<PipingSystemType>().ToList();

            sprinklerHeads = VpObjectFinders.FindFamilyTypes(doc, BuiltInCategory.OST_Sprinklers);
            downHead = sprinklerHeads.FirstOrDefault();
            vertHead = sprinklerHeads.FirstOrDefault();
            upHead = sprinklerHeads.FirstOrDefault();
        }

        public FamilySymbol GetHeadType(HeadType ht)
        {
            if (ht == HeadType.Down)
                return downHead;
            else if (ht == HeadType.UP)
                return upHead;
            else
                return vertHead;
        }

    }
}
