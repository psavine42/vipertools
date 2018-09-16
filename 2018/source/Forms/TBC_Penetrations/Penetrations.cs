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
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;
using GXYZ = Autodesk.Revit.DB.XYZ;
//using Microsoft.Office.Interop.Excel;
using Autodesk.Revit.UI.Selection;



using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RApplication = Autodesk.Revit.ApplicationServices.Application;

namespace Viper.Forms
{
    public class Penetrations
    {

       // private static Autodesk.Revit.ApplicationServices.Application m_application;
       // private static UIDocument uidoc;
        public  UIApplication app { get; set;  }

        public void Sleeves_by_LinkedClash(UIApplication app, string filename, Document doc)
        {
            FilteredElementCollector links = new FilteredElementCollector(doc).OfCategory(BuiltInCategory.OST_RvtLinks);
            RevitLinkInstance pipesrvt = null;

            foreach (Element e in links)
            {
                try
                {
                    if (e is RevitLinkInstance)
                    {
                        if (e.Name.ToString().Contains(filename) == true)
                        {
                            pipesrvt = e as RevitLinkInstance;
                            break;
                        }
                    }
                }
                catch (Exception) { TaskDialog.Show("asdas", "link MISSING"); }
            }

            if (pipesrvt == null)
            {
                TaskDialog.Show("error", "Link Missing");
            }
            Autodesk.Revit.DB.Transform trans = pipesrvt.GetTotalTransform();

            List<MEPtoObjectLink> intersectinglist = new List<MEPtoObjectLink>();
            StringBuilder sb = new StringBuilder();

            //GET PIPE DATA OUT OF LINKED REVIT FILE *******
            IList<Element> pps = VpObjectFinders.AllMEPCurves(pipesrvt.GetLinkDocument());
            List<Element> forsl = VpObjectFinders.AllHostableHorizantal(doc);

            FilteredElementCollector gFilter = new FilteredElementCollector(doc);
            ICollection<Element> z = gFilter.OfClass(typeof(FamilySymbol)).ToElements();
            FamilySymbol sleevernd = z.FirstOrDefault(e => e.Name.Equals("TBC_Round Sleeve - Floor or Wall")) as FamilySymbol;
            FamilySymbol sleevesq = z.FirstOrDefault(e => e.Name.Equals("TBC_Square Sleeve - Floor or Wall")) as FamilySymbol;

            if (sleevernd == null && sleevesq == null)
            {
                TaskDialog.Show("2", "fail - Sleeves not loaded");
                //if fail, load fams, incl. tag
            }

            Makepipes mp = new Makepipes();

            // - ----------------------------------------GET INTERSECTIONS ------------------------
            foreach (Element host in forsl)
            {
                try
                {
                    if (host is Floor || host is RoofBase)
                    {
                        HostObject hostObj = host as HostObject;

                        Reference exterior = HostObjectUtils.GetTopFaces(hostObj).FirstOrDefault() as Reference;
                        Face face = host.GetGeometryObjectFromReference(exterior) as Face;

                        foreach (Element mepobj in pps)
                        {

                            IntersectionResultArray resar3 = new IntersectionResultArray();
                            LocationCurve lc = mepobj.Location as LocationCurve;

                            XYZ p11 = trans.OfPoint(lc.Curve.GetEndPoint(0));
                            XYZ p22 = trans.OfPoint(lc.Curve.GetEndPoint(1));

                            XYZ pb1 = mp.determinebot(p11, p22);
                            XYZ pt1 = mp.determinetop(p11, p22);

                            XYZ bot = new XYZ(pb1.X, pb1.Y, pb1.Z - 0.5);
                            XYZ top = new XYZ(pt1.X, pt1.Y, pt1.Z + 0.5);

                            Line c = Line.CreateBound(bot, top); //app.Application.Create.NewLine(bot, top, true);

                            // if (face.Intersect(lc.Curve).ToString().Equals("Overlap") ) //&& resar3.IsEmpty == false)
                            if (face.Intersect(c).ToString().Equals("Overlap")) //&& resar3.IsEmpty == false)
                            {
                                face.Intersect(c, out resar3);
                                XYZ p1 = resar3.get_Item(0).XYZPoint;
                                MEPtoObjectLink link = new MEPtoObjectLink(mepobj, host, exterior, p1);
                                intersectinglist.Add(link);
                            }
                        }
                    }

                }
                catch (Exception) { }

            }

            int countyes = 0;
            int countno = 0;
            foreach (MEPtoObjectLink link in intersectinglist)
            {
                try
                {
                    string size = link.Mepobjid.LookupParameter("Size").AsString();

                    if (size.Split('x').Length == 2 && link.Mepobjid is Duct)
                    {
                        placePenetration(doc, sleevesq, link);
                    }
                    else if (size.Split('/').Length == 2 && link.Mepobjid is Duct)
                    { }   //    return "oval";
                    else // is a PIpe
                    {
                        placePenetration(doc, sleevernd, link);
                    }
                    countyes = countyes + 1;
                }
                catch (Exception) { countno = countno + 1; }
            }
            TaskDialog.Show("asd", countyes.ToString() + " Penetrations placed");
        }

        private void intersectinglinks(List<MEPtoObjectLink> intersectinglist, Face face,
            ICollection<Element> pps , Reference exterior, Element floor  )
        {

            foreach (Element mepobj in pps)
            {
                //Autodesk.Revit.DB.Transform trans;
                IntersectionResultArray resar3 = new IntersectionResultArray();
                LocationCurve lc = mepobj.Location as LocationCurve;

                if (face.Intersect(lc.Curve).ToString().Equals("Overlap") && resar3.IsEmpty == false)
                {
                    face.Intersect(lc.Curve, out resar3);
                    
                    XYZ p1 = resar3.get_Item(0).XYZPoint;
                    MEPtoObjectLink link = new MEPtoObjectLink(mepobj, floor, exterior, p1);
                    intersectinglist.Add(link);
                }
            }
        }

        public void placePenetration(Document doc, FamilySymbol sleeve, MEPtoObjectLink link)
        {
            VpObjectFinders vfo = new VpObjectFinders();
            XYZ direction = new XYZ(0, 1, 0);
            LocationCurve lc = link.Mepobjid.Location as LocationCurve;
            Pipe pp = link.Mepobjid as Pipe;
            Duct dt = link.Mepobjid as Duct;
            Conduit cn = link.Mepobjid as Conduit;

            FamilyInstance slv = doc.Create.NewFamilyInstance(link.hostobj, link.location, direction, sleeve);
            string type;
            double thickness;
            if (link.floorid is Floor){
                Floor elem = link.floorid as Floor;
                type = elem.FloorType.Name.ToString();
                thickness = elem.ParametersMap.get_Item("Thickness").AsDouble();
            }
            else if(link.floorid is RoofBase){
                RoofBase elem = link.floorid as RoofBase;
                type = elem.RoofType.Name.ToString();
                thickness = elem.ParametersMap.get_Item("Thickness").AsDouble();
            }
            else if(link.floorid is Wall){
                Wall elem = link.floorid as Wall;
                type = elem.WallType.Name.ToString();
                thickness = elem.ParametersMap.get_Item("Width").AsDouble();
            }
            else
            {
                type = "";
                thickness = 0;
            }
            
            ViParam.GetSet(slv, ViParam.MepElement, link.Mepobjid.Id.ToString());
            ViParam.GetSet(slv, ViParam.HostElementCat, link.floorid.Category.Name.ToString());
            ViParam.GetSet(slv, ViParam.HostElementType, type);
            ViParam.GetSet(slv, ViParam.SleeveDepth, thickness);
            ViParam.GetSet(slv, ViParam.ScheduleLevel, link.floorid.LevelId);
           
            if (pp != null)
            {
                ViParam.GetSet(slv, ViParam.MepCategory, "Pipe");
                ViParam.GetSet(slv, ViParam.MepType, pp.PipeType.Name.ToString());
                PipeInsulation insul = vfo.GetPipeInslationFromPipe(pp);
                if (insul == null)
                {
                    ViParam.GetSet(slv, ViParam.SleeveRadius, pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble());
                    ViParam.GetSet(slv, ViParam.TagDimension, ViParam.ToSleeveRadius(pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM)));
                }
                else
                {
                    double thick = insul.Thickness;
                    double diam = pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
                    double ddd = Math.Round((thick + thick + diam)*12, 2) / 12;
                    // slv.get_Parameter("Sleeve Radius").Set(ddd/12);
                    ViParam.GetSet(slv, ViParam.SleeveRadius, ddd );
                    ViParam.GetSet(slv, ViParam.TagDimension, ViParam.ToSleeveRadius(ddd));
                }
               
            }
            else if (dt != null)
            {
                ViParam.GetSet(slv, ViParam.MepType, dt.DuctType.Name.ToString());
                ViParam.GetSet(slv, ViParam.MepCategory, "Duct");

                // round duct sleeve
                if (slv.Name.Equals("TBC_Round Sleeve - Floor or Wall"))
                {
                    string sleeve_rad = ViParam.ToSleeveRadius(dt.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM));
                    ViParam.GetSet(slv, ViParam.SleeveRadius, dt.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble());
                    ViParam.GetSet(slv, ViParam.TagDimension, sleeve_rad);
                }
                //Rectangular sleeve
                else if (slv.Name.Equals("TBC_Square Sleeve - Floor or Wall"))
                {
                    string st1 = (dt.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble() *12).ToString();
                    string st2 = (dt.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble() * 12).ToString();
                    ViParam.GetSet(slv, ViParam.SleeveHeight, dt.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble());
                    ViParam.GetSet(slv, ViParam.SleeveWidth, dt.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble());
                    ViParam.GetSet(slv, ViParam.TagDimension, st1 + "\" x " + st2 + "\" ");
                }
            }
            else if (cn != null)
            {
                double conduit_diam = cn.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).AsDouble();
                string tag_dim = ViParam.ToSleeveRadius(dt.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM));

                ViParam.GetSet(slv, ViParam.MepCategory, "Conduit");
                ViParam.GetSet(slv, ViParam.MepType, cn.Name.ToString());
                ViParam.GetSet(slv, ViParam.SleeveRadius, conduit_diam);
                ViParam.GetSet(slv, ViParam.TagDimension, tag_dim);
            }
        }
    }

    public class MEPtoObjectLink
    {
        public Element Mepobjid { get; set; }
        public Element floorid { get; set; }
        public Reference hostobj { get; set; }
        public XYZ location { get; set; }


        public int diameter { get; set; }


        public ElementId penetration { get; set; }


        public  MEPtoObjectLink (Element mepobjId, Element FloorId, Reference Host, XYZ Location )
        {
            Mepobjid = mepobjId;
            floorid = FloorId;
            hostobj = Host;
            location = Location;

        }

        public MEPtoObjectLink (Element mepobjId, Element FloorId )
        {
            Mepobjid = mepobjId;
            floorid = FloorId;

        }

    }

}
