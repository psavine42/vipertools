using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI.Selection;
using Revit.SDK.Samples.UIAPI.CS.Viper2d;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RApplication = Autodesk.Revit.ApplicationServices.Application;

namespace Revit.SDK.Samples.UIAPI.CS
{
    class ViperUtils
    {

        //Basic import analysis
        public List<twopoint> analyzeimportSome(Document doc, ImportInstance dwg, XYZ point, ViperFormData vpdata, List<twopoint> geolist)
        {
            StringBuilder sb = new StringBuilder();
            Options opt = new Options();
            List<twopoint> pipesmade = new List<twopoint>();
            twopoint closestitemf = new twopoint();

            foreach (GeometryObject geoObj in dwg.get_Geometry(opt))
            {
                twopoint closestitem = new twopoint(new XYZ(), new XYZ(), 0, 1);

                if (geoObj is GeometryInstance)
                {
                 //   determinegeo(geoObj, geolist, point, closestitem);
                }
                else { }

                Makepipes mp = new Makepipes();
                List<twopoint> newlist = getlinesonlayer(geolist, closestitemf);
                //Llist organized such that the selected element is first
                List<twopoint> newlistt = mp.Onaxistree(newlist, doc, point, vpdata);
                List<twopoint> pipelistsort = mp.getfirstpipe(newlistt, point);

                List<twopoint> brout = new List<twopoint>();

                //Run recursive command to add to known tree
                
                List<twopoint> newlistt2 = mp.recursiveSome(pipelistsort.ElementAt(0), newlist, brout);

                ElementId level = dwg.LevelId;
                pipesmade = mp.MAKE_PIPES_new(newlistt2, doc, vpdata, level);
            }

            return pipesmade;
        }

        // NO
        public BlockObject readsingleblock(GeometryInstance inst, Autodesk.Revit.DB.Transform transform)
        {
            // GeometryInstance inst = geoObj2 as GeometryInstance;
            Autodesk.Revit.DB.Transform transf = inst.Transform;

            BlockObject bo = new BlockObject();

            foreach (GeometryObject geoObj2 in inst.SymbolGeometry)
            {

                if (geoObj2 is Autodesk.Revit.DB.Solid)
                {
                    //transf.o
                    bo.countSol.Add(geoObj2 as Autodesk.Revit.DB.Solid);
                }

                else if (geoObj2 is Autodesk.Revit.DB.Line)
                {
                    bo.countLine.Add(geoObj2 as Autodesk.Revit.DB.Line);
                }
                else if (geoObj2 is Autodesk.Revit.DB.Arc)
                {
                    bo.countArc.Add(geoObj2 as Autodesk.Revit.DB.Arc);
                }
                else if (geoObj2 is Autodesk.Revit.DB.PolyLine)
                {

                    PolyLine pl = geoObj2 as PolyLine;
                    IList<XYZ> plist = pl.GetCoordinates();

                    for (int i = 0; i < plist.Count - 1; i++)
                    {
                        Autodesk.Revit.DB.Line ll = Autodesk.Revit.DB.Line.CreateBound
                            (transf.OfPoint(plist[i]), transf.OfPoint(plist[i + 1]));
                        bo.countLine.Add(ll);
                    }

                }
                else { }

            }
            bo.cadlayer = inst.GraphicsStyleId.IntegerValue;
            bo.sortthis(bo);
           
            return bo;

        }


        //test the distance of a twopoint object to the current closest point to the 
        //point where the use clicked to select the main.
        private twopoint testdist(twopoint currentclosest, XYZ point, twopoint candidate)
        {
            //Test the which twopoint element is closest to the clicked point
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

                else { }

            }
            else
            {
                currentclosest = candidate;

            }
            return currentclosest;
        }

        //Determine Geo - MAIN METHOD
        public void determinegeo(GeometryObject geoObj, List<twopoint> geolist,  Autodesk.Revit.DB.Transform transform)
        {
            Autodesk.Revit.DB.Transform transf = null;
            int count = 0;
            StringBuilder sb = new StringBuilder();

            GeometryInstance inst = geoObj as GeometryInstance;
            transf = inst.Transform;

            foreach (GeometryObject geoObj2 in inst.SymbolGeometry)
            {

                sb.AppendLine(count.ToString() + "  " + geoObj2.GetType().ToString()
                    + " - " + geoObj2.IsElementGeometry.ToString());

                if (geoObj2 is Autodesk.Revit.DB.Line)
                {
                    //dealwithline(geoObj2, transf, )
                    Autodesk.Revit.DB.Line l = geoObj2 as Autodesk.Revit.DB.Line;
                    XYZ ptStartInRevit = transf.OfPoint(l.GetEndPoint(0));
                    XYZ ptEndInRevit = transf.OfPoint(l.GetEndPoint(1));
                    twopoint tp = new twopoint(ptEndInRevit, ptStartInRevit, l.GraphicsStyleId.IntegerValue, 1);
                    geolist.Add(tp);
                }

                else if (geoObj2 is PolyLine)
                {
                    //explode polyline
                    PolyLine pl = geoObj2 as PolyLine;
                    XYZ ptStartInRevit = pl.GetCoordinate(0); 
                    IList<XYZ> plist = pl.GetCoordinates();

                    for (int i = 0; i < plist.Count - 1; i++)
                    {
                        twopoint tp = new twopoint(transf.OfPoint(plist[i]), transf.OfPoint(plist[i + 1]), pl.GraphicsStyleId.IntegerValue, 1);
                        geolist.Add(tp);
                    }

                }
                else if (geoObj2 is Autodesk.Revit.DB.Arc)
                {
                    Autodesk.Revit.DB.Arc a = geoObj2 as Autodesk.Revit.DB.Arc;
                    XYZ centerpt = transf.OfPoint(a.Center);
                    XYZ pt2 = new XYZ(centerpt.X, centerpt.Y, centerpt.Z + 1);

                    if (a.IsBound)
                    {
                        twopoint tp = new twopoint(centerpt, pt2, a.GraphicsStyleId.IntegerValue, 2);
                        geolist.Add(tp);
                    }
                    else
                    {
                        twopoint tp = new twopoint(centerpt, pt2, a.GraphicsStyleId.IntegerValue, 3);
                        geolist.Add(tp);
                    }
                    
                    
                   // geolist.Add(tp);
                    #region test
 
                }

                //}
                    #endregion

                else if (geoObj2.GetType().ToString() == "Autodesk.Revit.DB.GeometryInstance")
                {
                    GeometryInstance inst2 = geoObj2 as GeometryInstance;
                    Autodesk.Revit.DB.Transform transft = inst2.Transform;
                    sb.AppendLine("Importinstance DETECTECTED" + inst2.GraphicsStyleId.IntegerValue.ToString());
                 //determinegeo(geoObj2, geolist, transf);
                    foreach (GeometryObject geoObj3 in inst2.SymbolGeometry)
                    {
                        if (geoObj3 is Autodesk.Revit.DB.Arc)
                        {
                            Autodesk.Revit.DB.Arc a = geoObj3 as Autodesk.Revit.DB.Arc;
                            //XYZ centerpt = transft.OfPoint(transf.OfPoint(a.Center));
                            XYZ centerpt = transft.OfPoint(a.Center);
                            XYZ pt2 = new XYZ(centerpt.X, centerpt.Y, centerpt.Z + 1);
                            if (a.IsBound)
                            {
                                twopoint tp = new twopoint(centerpt, pt2, a.GraphicsStyleId.IntegerValue, 3);
                                geolist.Add(tp);
                            }
                            else
                            {
                                twopoint tp = new twopoint(centerpt, pt2, a.GraphicsStyleId.IntegerValue, 2);
                                geolist.Add(tp);
                            }

                        }
                    }
                   
                }

                else
                {
                    sb.AppendLine("something else happened");
                }
            }
           //TaskDialog.Show("sb", sb.ToString());
        }

        public BlockObject mpBlockObject(BlockObject bo, StringBuilder sb, 
            GeometryInstance inst, Autodesk.Revit.DB.Transform ttransform)
        {

            Autodesk.Revit.DB.Transform transf = inst.Transform;

            foreach (GeometryObject geoObj2 in inst.SymbolGeometry)
            {

                if (geoObj2 is Autodesk.Revit.DB.Line)
                {
                    try
                        {
                    Autodesk.Revit.DB.Line l = geoObj2 as Autodesk.Revit.DB.Line;

                    bo.countLine.Add(Autodesk.Revit.DB.Line.CreateBound(transf.OfPoint(l.GetEndPoint(0)), transf.OfPoint(l.GetEndPoint(1))));
                    sb.AppendLine("L - " + transf.OfPoint(l.GetEndPoint(0)).ToString() + " " + transf.OfPoint(l.GetEndPoint(1)));
                    bo.cadlayer = geoObj2.GraphicsStyleId.IntegerValue;
                    }
                    catch (Exception) { }
                }
                else if (geoObj2 is Autodesk.Revit.DB.Arc)
                {
                    try
                        {
                        Autodesk.Revit.DB.Arc l = geoObj2 as Autodesk.Revit.DB.Arc;
                        Autodesk.Revit.DB.Arc n = l.CreateTransformed(transf) as Autodesk.Revit.DB.Arc; 
                        bo.countArc.Add(n);
                        bo.cadlayer = geoObj2.GraphicsStyleId.IntegerValue;
                        }
                    catch (Exception) { }
                    
                    
                }

                else if (geoObj2 is Autodesk.Revit.DB.PolyLine)
                {
                    try
                        {
                    PolyLine pl = geoObj2 as PolyLine;
                    IList<XYZ> plist = pl.GetCoordinates();

                    for (int i = 0; i < plist.Count - 1; i++)
                    {
                        Autodesk.Revit.DB.Line lll = Autodesk.Revit.DB.Line.CreateBound
                            (transf.OfPoint(plist[i]), transf.OfPoint(plist[i + 1]));
                        bo.countLine.Add(lll);
                        sb.AppendLine("pl -" + lll.GetEndPoint(0).ToString() + " " + lll.GetEndPoint(1));
                    }
                        }
                    catch (Exception) { }

                }      
                else { }
               
            }
            
            bo.transform = transf;
            bo.cadlayer = inst.GraphicsStyleId.IntegerValue;
            bo.sortthis(bo);
            return bo;

        }

        // TEST METHOD
       public List<BlockObject> determinegeoblock(GeometryObject geoObj, Autodesk.Revit.DB.Transform transform)
        {
            int allcount = 0;
            int plcount = 0;
            int arccount = 0;
            int solidcount = 0;
            int impotrtcount = 0;
            int facecount = 0;
            GeometryInstance inst = geoObj as GeometryInstance;

            StringBuilder sd = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            StringBuilder sa = new StringBuilder();

            List<BlockObject> allblocks = new List<BlockObject>();

            #region
            foreach (GeometryObject geoObj2 in inst.SymbolGeometry)
            {
                    
                    allcount++;
                    #region
                   
                    #endregion
                    if (geoObj2.GetType().ToString() == "Autodesk.Revit.DB.GeometryInstance")
                    {
                        #region
                        impotrtcount++;
                        GeometryInstance inst2 = geoObj2 as GeometryInstance;

                        Autodesk.Revit.DB.Transform transf = null;

                        if (transform == null) { transf = inst2.Transform; }
                        else { transf = inst.Transform.Multiply(transform); }

                        BlockObject bl = new BlockObject();
                        mpBlockObject(bl, sd, inst2, transf);
                        allblocks.Add(bl);

                        sa.AppendLine("line " +bl.countLine.Count.ToString() + "arc " + 
                        bl.countArc.Count.ToString() + "cent " + bl.getblockcentroid().ToString()
                        + " - " + bl.cadlayer.ToString()        
                        );
                        #endregion
                    }
                    else
                    {
                        //  sb.AppendLine("something else happened");
                    }
                    
            }
            #endregion

            int total = plcount+impotrtcount+arccount+facecount+solidcount;
            return allblocks;

        }


        //////////////////////////
        ///////BASIC METHODS///////
        ///////////////////////////

       public List<twopoint> getlinesonlayer(List<twopoint> listall, twopoint closestitem)
       {
           List<twopoint> newlist = new List<twopoint>();
           foreach (twopoint tp in listall)
           {
               if (tp.layer == closestitem.layer)
               {
                   newlist.Add(tp);
               }
           }
           return newlist;
       }
  
       public twopoint nearest(List<twopoint> listall, XYZ pt)
        {
            twopoint nearest = listall.ElementAt(0);
            double dist = 10000000;
            try
            {
                foreach (twopoint tp in listall)
                {
                    Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(tp.pt1, tp.pt2);
                    if (dist >= line.Distance(pt))
                    {
                        dist = line.Distance(pt);
                        nearest = tp;
                    }
                }
            }
            catch(Exception){}
            return nearest;
        }

       public BlockObject selectedblock(List<BlockObject> allblocks, XYZ point)
        {
            BlockObject nearest = new BlockObject();
            double dist = 10000;
            foreach (BlockObject blk in allblocks)
            {
             //   try
              //  {
                    XYZ center = blk.getblockcentroid();

                    if (center.DistanceTo(point) <= dist)
                    {
                        dist = center.DistanceTo(point);
                        nearest = blk;
                        //nearest.cadlayer = blk.cadlayer;
                        
                    }
               // }
               // catch (Exception) { }
            }


            return nearest;
        }


        //dir 1 if up, -1 if down, 
        //topbot - intersect top or bottom of the thing
       public Floor nearestfloor(View3D view, Document doc, XYZ pt, int dir, out XYZ locout)
       {        
           XYZ raydir = new XYZ(0, 0, dir);

           FilteredElementCollector filter = new FilteredElementCollector(doc, doc.ActiveView.Id);
           ICollection<Element> pps = filter.OfCategory(BuiltInCategory.OST_Floors).ToElements();

          // XYZ locout;// new XYZ() ;
           Element elout = Intersectutil(pps, view, pt, raydir, out locout);

           Floor f = elout as Floor;
           return f;
       }


       /// <summary>
       /// Find out the connector which the pipe's specified connector connected to.
       /// The pipe's specified connector is given by point conxyz.
       /// </summary>
       /// <param name="pipe">Pipe to find the connector</param>
       /// <param name="conXYZ">Specified point</param>
       /// <returns>Connector whose origin is conXYZ</returns>
       private Connector FindConnectedTo(MEPCurve pipe, XYZ conXYZ)
       {
           Connector connItself = FindConnector(pipe, conXYZ);
           ConnectorSet connSet = connItself.AllRefs;
           foreach (Connector conn in connSet)
           {
               if (conn.Owner.Id.IntegerValue != pipe.Id.IntegerValue &&
                   conn.ConnectorType == ConnectorType.End)
               {
                   return conn;
               }
           }
           return null;
       }


        public void connectrun(List<twopoint> run, Document doc)
        {
            ViperUtils vpu = new ViperUtils();
            Makepipes mp = new Makepipes();

            for (int i = 1; i < run.Count; i++)
            {
                // Get the end point from the previous section.
                Autodesk.Revit.DB.XYZ start = run[i - 1].pt2;

                // Get the start point from the current section.
                Autodesk.Revit.DB.XYZ end = run[i].pt1;

                // Create elbow fitting to connect previous section with tmpPipe.
                Connector conn1 = vpu.FindConnector(run[i - 1].Mepcurve as Pipe, start);
                Connector conn2 = vpu.FindConnector(run[i].Mepcurve as Pipe, start);
                try
                {
                    FamilyInstance fi = doc.Create.NewElbowFitting(conn1, conn2);
                    mp.Make_Insul(run[i].Mepcurve as Pipe, fi, doc);
                }
                catch (Exception) { }
             
            }
        }

       public Connector FindConnector(MEPCurve pipe, XYZ conXYZ)
       {
           ConnectorSet conns = pipe.ConnectorManager.Connectors;
           foreach (Connector conn in conns)
           {
               if (conn.Origin.IsAlmostEqualTo(conXYZ))
               {
                   return conn;
               }
           }
           return null;
       }

       public Ceiling nearestceiling(XYZ pt)
       {
           Ceiling e = null;

           return e;
       }

       public RoofBase nearestroof(View3D view, XYZ pt, Document doc, int dir, int topbot)
       {
           RoofBase r = null;

           XYZ raydir = new XYZ(0, 0, dir);
           FilteredElementCollector filter = new FilteredElementCollector(doc, doc.ActiveView.Id);
           ICollection<Element> pps = filter.OfCategory(BuiltInCategory.OST_Roofs).ToElements();
           XYZ ptout;
           Element elout = Intersectutil(pps, view, pt, raydir, out ptout);

           return r;
       }

       public List<FamilyInstance> GetGenericinsts(Document doc, string name)
       {
           FilteredElementCollector gFilter = new FilteredElementCollector(doc);
           ICollection<Element> z = gFilter
               .OfClass(typeof(FamilyInstance))
               .OfCategory(BuiltInCategory.OST_GenericModel).ToElements();

           List<FamilyInstance> nl = new List<FamilyInstance>();

           foreach (Element e in z)
           {
               Element elemtype = doc.GetElement(e.GetTypeId());
               if (elemtype.Name == name)
               {
                   nl.Add(e as FamilyInstance);
               }
           }

           return nl;
       }

       public FamilyInstance GetGenericinst(Document doc, string name)
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

           FamilyInstance s = nl.ElementAt(0) as FamilyInstance;
           return s;
       }

        // rebuildvector
       public XYZ pipevector(MEPCurve pipe, XYZ userpoint)
       {
           XYZ old1 = nearestpipepoints(pipe, userpoint);
           XYZ vector = userpoint  - old1 ;
          // XYZ nvector = new XYZ (vector.X, vector.Y, 0);
           return vector;
       }

 
      //  private void drawline(XYZ 

        //order a list of points in lines
        //if the pints are within a tolerence returns a pointlist represtning a polyline
        //if they are not a polyline, returns the parts that are polylines
       public List<XYZ> orderpointlist(List<Line> lines, XYZ startpoint)
       {
           List<XYZ> pointsinorder = new List<XYZ>();
           XYZ newstartpoint = lines.ElementAt(0).GetEndPoint(0);
           double disttostartpoint = 10000;

           foreach (Line l in lines)
           {
               XYZ p1 = l.GetEndPoint(0);
               XYZ p2 = l.GetEndPoint(1);

              // if(p1.DistanceTo(
           }



           return pointsinorder;
       }


       // nearest connector on a pipe to a point
       public XYZ nearestpipepoints(MEPCurve pipe, XYZ point)
       {
           LocationCurve lc = pipe.Location as LocationCurve;
           XYZ base1 = new XYZ();

           //lc.Curve.
           if (point.DistanceTo(lc.Curve.GetEndPoint(0)) > point.DistanceTo(lc.Curve.GetEndPoint(1)))
           {
               base1 = lc.Curve.GetEndPoint(1);
           }
           else
           {
               base1 = lc.Curve.GetEndPoint(0);
           }

          // TaskDialog.Show("as", base1.ToString());
           return base1;
       }



       public XYZ twopipepoint(MEPCurve p1, MEPCurve p2, out XYZ vector)
       {
            
           LocationCurve lc1 = p1.Location as LocationCurve;
           LocationCurve lc2 = p2.Location as LocationCurve;

           XYZ pt11 = lc1.Curve.GetEndPoint(0);
           XYZ pt12 = lc1.Curve.GetEndPoint(1);

           XYZ pt21 = lc2.Curve.GetEndPoint(0);
           XYZ pt22 = lc2.Curve.GetEndPoint(1);

           XYZ near1 = new XYZ();
           XYZ near2 = new XYZ();
           double dl = 10000;

           if (pt11.DistanceTo(pt21) < dl)
           {
               dl = pt11.DistanceTo(pt21);
               near1 = pt11;
               near2 = pt21;
           }
           if (pt12.DistanceTo(pt21) < dl)
           {
               dl = pt11.DistanceTo(pt21);
               near1 = pt12;
               near2 = pt21;
           }
           if (pt11.DistanceTo(pt22) < dl)
           {
               dl = pt11.DistanceTo(pt21);
               near1 = pt11;
               near2 = pt22;
           }
           if (pt12.DistanceTo(pt22) < dl)
           {
               dl = pt11.DistanceTo(pt21);
               near1 = pt12;
               near2 = pt22;
           }

           XYZ bas  = new XYZ((near2.X - near1.X), 
               (near2.Y - near1.Y), 
               (near2.Z - near1.Z));


           XYZ base1 = near2 - bas/2;
            vector = near2 - near1;

           // TaskDialog.Show("as", base1.ToString());
           return base1;
       }


       public XYZ MEPCurvetoVector(MEPCurve p1)
       {
           LocationCurve pc2 = p1.Location as LocationCurve;
           XYZ near1 = pc2.Curve.GetEndPoint(0);
           XYZ near2 = pc2.Curve.GetEndPoint(1);

           XYZ base1 = new XYZ((near1.X - near2.X), (near1.Y - near2.Y), (near1.Z - near2.Z));

           return base1;
       }

       public XYZ GetPerpVector( XYZ vec)
       {

           XYZ vecAbs = new XYZ(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));
           //avoid parallel  vector
          // XYZVEct
           XYZ perp = new XYZ();

           if (vecAbs.X < vecAbs.Y)
           {
               if (vecAbs.X < vecAbs.Z)
               {
                   perp = new XYZ(1, 0, 0);// Vector.unitX;
               }
               else
               {
                   perp = new XYZ(0, 0, 1);// Vector.UnitZ;
               }
           }
           else
           {
               if (vecAbs.Y < vecAbs.Z)
               {
                   perp = new XYZ(0, 1, 0); //Vector.UnitY;
               }
               else
               {
                   perp = new XYZ(0, 0, 1); // Vector.UnitZ;
               }
           }

         perp.Normalize();
           XYZ pt = vec.CrossProduct(perp);
           return pt; 
           //Vector3.Cross(ref vec, ref perp, out perp);
       }

       public Reference nearestref(Document doc,  IList<Reference> sheeps,   Line cc)
       {
           Reference mainpipe = sheeps.ElementAt(0);
           double closest = 1000;
           foreach (Reference sheep in sheeps)
           {
               Element elemp = doc.GetElement(sheep);
               MEPCurve leadp = elemp as MEPCurve;

               LocationCurve plc = leadp.Location as LocationCurve;
               XYZ p1 = plc.Curve.GetEndPoint(0);
               XYZ p2 = plc.Curve.GetEndPoint(1);
               if (p1.DistanceTo(cc.GetEndPoint(0)) < closest)
               {
                   closest = p1.DistanceTo(cc.GetEndPoint(0));
                   mainpipe = sheep;
               }
               if (p2.DistanceTo(cc.GetEndPoint(0)) < closest)
               {
                   closest = p2.DistanceTo(cc.GetEndPoint(0));
                   mainpipe = sheep;
               }
           }
           return mainpipe;

       }

       public XYZ farthestpipepoints(MEPCurve pipe, XYZ point)
       {
           LocationCurve lc = pipe.Location as LocationCurve;
           XYZ base1 = new XYZ();
           if (point.DistanceTo(lc.Curve.GetEndPoint(0)) > point.DistanceTo(lc.Curve.GetEndPoint(1)))
           {
               base1 = lc.Curve.GetEndPoint(0);
           }
           else
           {
               base1 = lc.Curve.GetEndPoint(1);
           }
           return base1;
       }


        //create object
       public void rebuildpipevector(Document doc, MEPCurve mpcrv, XYZ base1, XYZ old, XYZ vector)
       {
           Makepipes mp = new Makepipes();
           XYZ newbase = old + vector;
           twopoint tp = new twopoint(base1, newbase, mpcrv);

           Pipe pp = tp.Mepcurve as Pipe;
           if (pp != null)
           {
               twopoint npipe = mp.Make_Pipe(tp, doc);    
           }

           Conduit ct = tp.Mepcurve as Conduit;
           if (ct != null)
           {
               twopoint nconduit = mp.Make_Conduit(tp, doc);  
           }

            doc.Delete(mpcrv.Id);
       }


     public void buildpipevector(Document doc, MEPCurve pipe, XYZ base1, XYZ newbase)
       {
          // XYZ newbase = old + vector;

           Pipe pp = pipe as Pipe;
           if (pp != null)
           {
               Autodesk.Revit.DB.Parameter diamz = pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
               double diam = diamz.AsDouble();
               PipeType pt = pp.PipeType;

               Pipe pig2 = doc.Create.NewPipe(base1, newbase, pt);
               pig2.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(diam);
               doc.Delete(pipe.Id);
           }

           Conduit ct = pipe as Conduit;
           if (ct != null)
           {
               FilteredElementCollector collector = new FilteredElementCollector(doc);
               collector.OfClass(typeof(Autodesk.Revit.DB.Electrical.ConduitType));
               Autodesk.Revit.DB.Electrical.ConduitType type = collector.FirstElement() as Autodesk.Revit.DB.Electrical.ConduitType;
               //  pipe.Level

               Autodesk.Revit.DB.Parameter diamz = ct.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
               double diam = diamz.AsDouble();
               //  ConduitType condtype = ct.GetType() as ConduitType;

               Conduit pig2 = Autodesk.Revit.DB.Electrical.Conduit.Create(doc, type.Id, base1, newbase, pipe.LevelId);
               pig2.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(diam);
               doc.Delete(pipe.Id);
           }


       }


       private Element Intersectutil(ICollection<Element> pps, View3D view, XYZ pt, XYZ raydir, out XYZ intersection)
       {
           Element elout = null;
          // XYZ
           intersection = null;
           double distance = Double.PositiveInfinity;

           foreach (Element e in pps)
           {
               ReferenceIntersector referenceIntersector = new ReferenceIntersector(e.Id, FindReferenceTarget.Face, view);
               IList<ReferenceWithContext> references = referenceIntersector.Find(pt, raydir);
               foreach (ReferenceWithContext referenceWithContext in references)
               {
                   Reference reference = referenceWithContext.GetReference();
                   // Keep the closest matching reference (using the proximity parameter to determine closeness).
                   double proximity = referenceWithContext.Proximity;
                   if (proximity < distance)
                   {
                       distance = proximity;
                       intersection = reference.GlobalPoint;
                       elout = e;
                   }
               }

           }


           return elout;
       }


       public class TargetElementSelectionFilter : ISelectionFilter
        {


            public bool AllowElement(Element element)
            {
                // Element must have at least one usable solid
                IList<Solid> solids = GetTargetSolids(element);

                return solids.Count > 0;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return true;
            }
        }

       public static IList<Solid> GetTargetSolids(Element element)
        {
            List<Solid> solids = new List<Solid>();


            Options options = new Options();
            options.DetailLevel = ViewDetailLevel.Fine;
            GeometryElement geomElem = element.get_Geometry(options);
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is Solid)
                {
                    Solid solid = (Solid)geomObj;
                    if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                    {
                        solids.Add(solid);
                    }
                    // Single-level recursive check of instances. If viable solids are more than
                    // one level deep, this example ignores them.
                }
                else if (geomObj is GeometryInstance)
                {
                    GeometryInstance geomInst = (GeometryInstance)geomObj;
                    GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                    foreach (GeometryObject instGeomObj in instGeomElem)
                    {
                        if (instGeomObj is Solid)
                        {
                            Solid solid = (Solid)instGeomObj;
                            if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                            {
                                solids.Add(solid);
                            }
                        }
                    }
                }
            }
            return solids;
        }


    }
}
