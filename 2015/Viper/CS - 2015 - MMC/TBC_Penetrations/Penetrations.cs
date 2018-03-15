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
using Microsoft.Office.Interop.Excel;
using Autodesk.Revit.UI.Selection;



using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RApplication = Autodesk.Revit.ApplicationServices.Application;

namespace Revit.SDK.Samples.UIAPI.CS
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
                    catch (Exception) {TaskDialog.Show("asdas" , "link MISSING");    }
                }

                if (pipesrvt == null)
                { TaskDialog.Show("error", "Link Missing"); }
                Autodesk.Revit.DB.Transform trans = pipesrvt.GetTotalTransform();

               List<MEPtoObjectLink> intersectinglist = new List<MEPtoObjectLink>();
                StringBuilder sb = new StringBuilder();

                //GET PIPE DATA OUT OF LINKED REVIT FILE *******
                VpObjectFinders vpo = new VpObjectFinders();
                IList<Element> pps = vpo.AllMEPCurves(pipesrvt.GetLinkDocument());

                //GET FLOORS and other DATA*******	    
                FilteredElementCollector fFilter = new FilteredElementCollector(doc, doc.ActiveView.Id);
                ElementCategoryFilter flrs = new ElementCategoryFilter(BuiltInCategory.OST_Floors);
                ElementCategoryFilter roof = new ElementCategoryFilter(BuiltInCategory.OST_Roofs);
                ElementCategoryFilter fnds = new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation);
               // ElementCategoryFilter walls = new ElementCategoryFilter(BuiltInCategory.OST_Walls);
                List<ElementFilter> filterlist = new List<ElementFilter>(){flrs, fnds, roof};
                LogicalOrFilter f1 = new LogicalOrFilter(filterlist);
                ICollection<Element> forsl = fFilter.WherePasses(f1).ToElements();
         
                
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
                        if (host is Autodesk.Revit.DB.Floor || host is Autodesk.Revit.DB.RoofBase)
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

                                XYZ bot = new XYZ(pb1.X, pb1.Y, pb1.Z - 0.5 );
                                XYZ top = new XYZ(pt1.X, pt1.Y, pt1.Z + 0.5);

                                Autodesk.Revit.DB.Line c = Autodesk.Revit.DB.Line.CreateBound(bot, top); //app.Application.Create.NewLine(bot, top, true);

                              // if (face.Intersect(lc.Curve).ToString().Equals("Overlap") ) //&& resar3.IsEmpty == false)
                                if (face.Intersect(c).ToString().Equals("Overlap") ) //&& resar3.IsEmpty == false)
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
                            string size = link.Mepobjid.LookupParameter("Size") .AsString();

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

            Autodesk.Revit.DB.Floor floor = link.floorid as Autodesk.Revit.DB.Floor;
            Autodesk.Revit.DB.RoofBase roof = link.floorid as Autodesk.Revit.DB.RoofBase;
            Autodesk.Revit.DB.Wall wall = link.floorid as Autodesk.Revit.DB.Wall;
            FamilyInstance slv = doc.Create.NewFamilyInstance(link.hostobj, link.location, direction, sleeve);

            slv.get_Parameter("MEP Element").Set(link.Mepobjid.Id.ToString());

            if (link.floorid is Autodesk.Revit.DB.Floor)
            {
                slv.get_Parameter("Host Element Category").Set(floor.Category.Name.ToString());
                slv.get_Parameter("Host Element Id").Set(floor.Id.ToString());
                slv.get_Parameter("Host Element Type").Set(floor.FloorType.Name.ToString());
                slv.get_Parameter("Sleeve Depth").Set(floor.get_Parameter("Thickness").AsDouble());
                slv.get_Parameter("Schedule Level").Set(floor.LevelId);

            }
            if (link.floorid is Autodesk.Revit.DB.RoofBase)
            {
                slv.get_Parameter("Host Element Category").Set(roof.Category.Name.ToString());
                slv.get_Parameter("Host Element Id").Set(roof.Id.ToString());
        
                slv.get_Parameter("Host Element Type").Set(roof.RoofType.Name.ToString());
                slv.get_Parameter("Sleeve Depth").Set(roof.get_Parameter("Thickness").AsDouble());
                slv.get_Parameter("Schedule Level").Set(roof.LevelId);

            }
            if (link.floorid is Autodesk.Revit.DB.Wall)
            {
                slv.get_Parameter("Host Element Category").Set(wall.Category.Name.ToString());
                slv.get_Parameter("Host Element Id").Set(wall.Id.ToString());
                slv.get_Parameter("Host Element Type").Set(wall.WallType.Name.ToString());
                slv.get_Parameter("Sleeve Depth").Set(wall.get_Parameter("Width").AsDouble());
                slv.get_Parameter("Schedule Level").Set(wall.LevelId);
            }



            if (pp != null)
            {

                slv.get_Parameter("MEP Category").Set("Pipe");
                slv.get_Parameter("MEP Type").Set(pp.PipeType.Name.ToString());

                PipeInsulation insul = vfo.GetPipeInslationFromPipe(pp);
                if (insul == null)
                {
                    slv.get_Parameter("Sleeve Radius").Set(pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble());
                    string st = (pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble() * 12).ToString();
                    slv.get_Parameter("Tag Dimension").Set(st + "\"ø ");
                }
                else
                {
                    
                    double thick = insul.Thickness;
                    double diam = pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
                    double ddd = Math.Round((thick + thick + diam)*12, 2);
                    slv.get_Parameter("Sleeve Radius").Set(ddd/12);
                    string st = ddd.ToString();
                    slv.get_Parameter("Tag Dimension").Set(st + "\"ø ");
//
                }
               
            }
            else if (dt != null)
            {

                slv.get_Parameter("MEP Type").Set(dt.DuctType.Name.ToString());
                slv.get_Parameter("MEP Category").Set("Duct");

                // round duct sleeve
                if(slv.Name.Equals("TBC_Round Sleeve - Floor or Wall"))
                {
                    slv.get_Parameter("Sleeve Radius").Set(dt.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble());
                    string st = (dt.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble() * 12).ToString();
                    slv.get_Parameter("Tag Dimension").Set(st + "\"ø ");
                }

                //Rectangular sleeve
                else if (slv.Name.Equals("TBC_Square Sleeve - Floor or Wall"))
                {
                    string st1 = (dt.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble() *12).ToString();
                    string st2 = (dt.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble() * 12).ToString();
                    slv.get_Parameter("Sleeve Height").Set(dt.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).AsDouble());
                    slv.get_Parameter("Sleeve Width").Set(dt.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).AsDouble());
                    slv.get_Parameter("Tag Dimension").Set(st1 + "\" x " + st2 + "\" ");
                }

            }

            else if (cn != null)
            {
                slv.get_Parameter("MEP Category").Set("Conduit");
                slv.get_Parameter("MEP Type").Set(cn.Name.ToString());
                slv.get_Parameter("Sleeve Radius").Set(cn.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).AsDouble());
                string st = (cn.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).AsDouble() * 12).ToString();
                slv.get_Parameter("Tag Dimension").Set(st + "\"ø ");
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
