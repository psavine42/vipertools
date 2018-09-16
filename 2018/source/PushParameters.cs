


using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using Autodesk.Revit.DB.Plumbing;

namespace Viper
{
    public class ViParam
    {
        public static string TagDimension = "Tag Dimension";
        public static string SleeveRadius = "Sleeve Radius";
        public static string SleeveHeight = "Sleeve Height";
        public static string SleeveDepth = "Sleeve Depth";
        public static string SleeveWidth = "Sleeve Width";

        public static string ScheduleLevel = "Schedule Level";

        public static string MepCategory = "MEP Category";
        public static string MepType = "MEP Type";
        public static string MepElement = "MEP Element";

        public static string HostElementId = "Host Element Id";
        public static string HostElementType = "Host Element Type";
        public static string HostElementCat = "Host Element Category";

        public static PipeType DefaultPipeType(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(PipeType));
            return collector.FirstElement() as PipeType;

        }

        public static Level DefaultLevel(Document doc)
        {
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(Level));
            return col.FirstElement() as Level;
        }

        public static PipingSystemType DefaultMEPSystemType(Document doc)
        {
            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(PipingSystemType));
            return col.FirstElement() as PipingSystemType;
        }
        public static PipingSystemType DefaultMEPSystem(Document doc, MEPSystemType stype)
        {
            FilteredElementCollector col = new FilteredElementCollector(doc)
                .OfClass(typeof(PipingSystemType)).OfCategoryId(stype.Category.Id);
            return col.FirstElement() as PipingSystemType;
        }


        public static Pipe CompatPipe(Document doc, PipeType pt, XYZ p1, XYZ p2)
        {
            PipingSystemType sys = DefaultMEPSystemType(doc);
            Level level = DefaultLevel(doc);
            return Pipe.Create(doc, sys.Id, pt.Id, level.Id, p1, p2);
        }

        public static Pipe CompatPipe(Document doc, PipeType pt, Level level, XYZ p1, XYZ p2)
        {
            PipingSystemType sys = DefaultMEPSystemType(doc);
            return Pipe.Create(doc, sys.Id, pt.Id, level.Id, p1, p2);
        }


        public static string ToSleeveRadius(Parameter param)
        {
            return (param.AsDouble() * 12).ToString() + "\"ø ";
        }
        public static string ToSleeveRadius(double param)
        {
            return (param * 12).ToString() + "\"ø ";
        }

        public static void GetSet(FamilyInstance instance, string param, string value)
        {
            instance.ParametersMap.get_Item(param).Set(value);
        }
        public static void GetSet(FamilyInstance instance, string param, double value)
        {
            instance.ParametersMap.get_Item(param).Set(value);
        }
        public static void GetSet(FamilyInstance instance, string param, ElementId value)
        {
            instance.ParametersMap.get_Item(param).Set(value);
        }
        public static void GetSet(Element instance, string param, string value)
        {
            instance.ParametersMap.get_Item(param).Set(value);
        }
        public static void GetSet(Element instance, string param, double value)
        {
            instance.ParametersMap.get_Item(param).Set(value);
        }
        public static void GetSet(WallType instance, string param, string value)
        {
            instance.ParametersMap.get_Item(param).Set(value);
        }

    }
}
