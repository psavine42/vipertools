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
using Viper.Viper2d;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace Viper
{

    class ViperUtils
    {

        public static List<TwoPoint> analyzeimport(ImportInstance dwg, List<TwoPoint> geolist)
        {
            Options opt = new Options();
            ViperUtils vpu = new ViperUtils();
            Transform transf = null;

            foreach (GeometryObject geoObj in dwg.get_Geometry(opt))
            {
                if (geoObj is GeometryInstance)
                {
                    //convert all elements in the cad to the two point class
                    vpu.determinegeo(geoObj, geolist, transf);
                }
            }
            return geolist;
        }

        public static List<MEPCurve> RefsToCurve(Document doc, IList<Reference> selected)
        {
            List<MEPCurve> meps = new List<MEPCurve>();
            foreach (Reference pp in selected)
            {
                Element em = doc.GetElement(pp);
                MEPCurve crv = em as MEPCurve;
                if (crv != null)
                {
                    meps.Add(crv);
                }
            }
            return meps;
        }

        public static List<MEPCurve> RefsToCurve(Document doc, ICollection<ElementId> selected)
        {
            List<MEPCurve> meps = new List<MEPCurve>();
            foreach (ElementId pp in selected)
            {
                Element em = doc.GetElement(pp);
                MEPCurve crv = em as MEPCurve;
                if (crv != null)
                {
                    meps.Add(crv);
                }
            }
            return meps;
        }

     

        //Determine Geo - MAIN METHOD
        public void determinegeo(GeometryObject geoObj, List<TwoPoint> geolist, Transform transform)
        {
            int count = 0;
            GeometryInstance inst = geoObj as GeometryInstance;
            Transform transf = inst.Transform;

            foreach (GeometryObject geoObj2 in inst.SymbolGeometry)
            {

                Debug.WriteLine(count.ToString() + "  " + geoObj2.GetType().ToString() + " - " + geoObj2.IsElementGeometry.ToString());

                if (geoObj2 is Line)
                {
                    //dealwithline(geoObj2, transf, )
                    Line l = geoObj2 as Line;
                    XYZ ptStartInRevit = transf.OfPoint(l.GetEndPoint(0));
                    XYZ ptEndInRevit = transf.OfPoint(l.GetEndPoint(1));
                    geolist.Add(new TwoPoint(ptEndInRevit, ptStartInRevit, l.GraphicsStyleId.IntegerValue, 1));

                }

                else if (geoObj2 is PolyLine)
                {
                    //explode polyline
                    PolyLine pl = geoObj2 as PolyLine;
                    XYZ ptStartInRevit = pl.GetCoordinate(0); 
                    IList<XYZ> plist = pl.GetCoordinates();
                    for (int i = 0; i < plist.Count - 1; i++)
                    {
                        geolist.Add(new TwoPoint(transf.OfPoint(plist[i]), transf.OfPoint(plist[i + 1]), pl.GraphicsStyleId.IntegerValue, 1));
                    }

                }
                else if (geoObj2 is Arc)
                {
                    Arc a = geoObj2 as Arc;
                    XYZ centerpt = transf.OfPoint(a.Center);
                    XYZ pt2 = new XYZ(centerpt.X, centerpt.Y, centerpt.Z + 1);

                    if (a.IsBound)
                        geolist.Add(new TwoPoint(centerpt, pt2, a.GraphicsStyleId.IntegerValue, 3));
                    else
                        geolist.Add(new TwoPoint(centerpt, pt2, a.GraphicsStyleId.IntegerValue, 2));
                }
                else if (geoObj2.GetType().ToString() == "Autodesk.Revit.DB.GeometryInstance")
                {
                    GeometryInstance inst2 = geoObj2 as GeometryInstance;
                    Transform transft = inst2.Transform;
                    Debug.WriteLine("Importinstance DETECTECTED" + inst2.GraphicsStyleId.IntegerValue.ToString());
                    //determinegeo(geoObj2, geolist, transf);
                    foreach (GeometryObject geoObj3 in inst2.SymbolGeometry)
                    {
                        if (geoObj3 is Arc)
                        {
                            Arc a = geoObj3 as Arc;
                            //XYZ centerpt = transft.OfPoint(transf.OfPoint(a.Center));
                            XYZ centerpt = transft.OfPoint(a.Center);
                            XYZ pt2 = new XYZ(centerpt.X, centerpt.Y, centerpt.Z + 1);
                            if (a.IsBound)
                                geolist.Add( new TwoPoint(centerpt, pt2, a.GraphicsStyleId.IntegerValue, 3));
                            else
                                geolist.Add(new TwoPoint(centerpt, pt2, a.GraphicsStyleId.IntegerValue, 2));
                        }
                    }     
                }
                else
                {
                    Debug.WriteLine("something else happened");
                }
            }

        }


        public BlockObject mpBlockObject(BlockObject bo, GeometryInstance inst, Transform ttransform)
        {
            Transform transf = inst.Transform;
            foreach (GeometryObject geoObj2 in inst.SymbolGeometry)
            {
                if (geoObj2 is Line)
                {
                    try
                    {
                        Line l = geoObj2 as Line;
                        bo.countLine.Add(Line.CreateBound(transf.OfPoint(l.GetEndPoint(0)), transf.OfPoint(l.GetEndPoint(1))));
                        bo.cadlayer = geoObj2.GraphicsStyleId.IntegerValue;
                    }
                    catch (Exception) { }
                }
                else if (geoObj2 is Arc)
                {
                    try
                    {
                        Arc l = geoObj2 as Arc;
                        bo.Add(l.CreateTransformed(transf) as Arc);
                        bo.cadlayer = geoObj2.GraphicsStyleId.IntegerValue;
                    }
                    catch (Exception) { }
                }

                else if (geoObj2 is PolyLine)
                {
                    try
                    {
                        PolyLine pl = geoObj2 as PolyLine;
                        IList<XYZ> plist = pl.GetCoordinates();

                        for (int i = 0; i < plist.Count - 1; i++)
                        {
                            bo.Add(Line.CreateBound(transf.OfPoint(plist[i]), transf.OfPoint(plist[i + 1])));
                        }
                    }
                    catch (Exception) { }

                }
            }

            bo.transform = transf;
            bo.cadlayer = inst.GraphicsStyleId.IntegerValue;
            bo.Sort();
            return bo;
        }

        // TEST METHOD
        public List<BlockObject> determinegeoblock(GeometryObject geoObj, Transform transform)
        {
            int allcount = 0;
            int plcount = 0;
            int arccount = 0;
            int solidcount = 0;
            int impotrtcount = 0;
            int facecount = 0;
            GeometryInstance inst = geoObj as GeometryInstance;
            List<BlockObject> allblocks = new List<BlockObject>();

            #region
            foreach (GeometryObject geoObj2 in inst.SymbolGeometry)
            {

                allcount++;

                if (geoObj2.GetType().ToString() == "Autodesk.Revit.DB.GeometryInstance")
                {
                    #region
                    impotrtcount++;
                    GeometryInstance inst2 = geoObj2 as GeometryInstance;

                    Transform transf = null;
                    if (transform == null) 
                        transf = inst2.Transform;
                    else
                        transf = inst.Transform.Multiply(transform);

                    BlockObject bl = new BlockObject();
                    mpBlockObject(bl, inst2, transf);
                    allblocks.Add(bl);

                    Debug.WriteLine("line " + bl.countLine.Count.ToString() + "arc " +
                        bl.countArc.Count.ToString() + "cent " + bl.Centroid().ToString()
                        + " - " + bl.cadlayer.ToString()
                    );
                    #endregion
                }


            }
            #endregion

            int total = plcount + impotrtcount + arccount + facecount + solidcount;
            return allblocks;

        }


        //////////////////////////
        ///////BASIC METHODS///////
        ///////////////////////////
        public List<TwoPoint> GetLinesOnLayer(List<TwoPoint> listall, TwoPoint closestitem)
       {
           List<TwoPoint> newlist = new List<TwoPoint>();
           foreach (TwoPoint tp in listall)
           {
               if (tp.layer == closestitem.layer)
               {
                   newlist.Add(tp);
               }
           }
           return newlist;
       }
  
       public TwoPoint nearest(List<TwoPoint> listall, XYZ pt)
        {
            TwoPoint nearest = listall.ElementAt(0);
            double dist = 10000000;
            try
            {
                foreach (TwoPoint tp in listall)
                {
                    Line line = Line.CreateBound(tp.pt1, tp.pt2);
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
                XYZ center = blk.Centroid();
                if (center.DistanceTo(point) <= dist)
                {
                    dist = center.DistanceTo(point);
                    nearest = blk;
                }
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


        public void connectrun(List<TwoPoint> run, Document doc)
        {
            ViperUtils vpu = new ViperUtils();
          
            for (int i = 1; i < run.Count; i++)
            {
                XYZ start = run[i - 1].pt2;  // Get the end point from the previous section.
                XYZ end = run[i].pt1;   // Get the start point from the current section.

                // Create elbow fitting to connect previous section with tmpPipe.
                Connector conn1 = vpu.FindConnector(run[i - 1].Mepcurve as Pipe, start);
                Connector conn2 = vpu.FindConnector(run[i].Mepcurve as Pipe, start);
                try
                {
                    FamilyInstance fi = doc.Create.NewElbowFitting(conn1, conn2);
                    Makepipes.Make_Insul(run[i].Mepcurve as Pipe, fi, doc);
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
           ICollection<Element> generic_models = gFilter
               .OfClass(typeof(FamilyInstance))
               .OfCategory(BuiltInCategory.OST_GenericModel).ToElements();

           List<FamilyInstance> nl = new List<FamilyInstance>();

           foreach (Element e in generic_models)
           {
               Element elemtype = doc.GetElement(e.GetTypeId());
               if (elemtype.Name == name)
               {
                   nl.Add(e as FamilyInstance);
               }
           }
           return nl;
       }


        public static LogicalOrFilter CategoriesFilter(List<BuiltInCategory> categories)
        {
            List<ElementFilter> elemFilter = new List<ElementFilter>();
            foreach (BuiltInCategory cat in categories)
            {
                elemFilter.Add(new ElementCategoryFilter(cat));
            }
            return new LogicalOrFilter(elemFilter);
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
           return vector;
       }

       // nearest connector on a pipe to a point
       public static XYZ nearestpipepoints(MEPCurve pipe, XYZ point)
       {
           LocationCurve lc = pipe.Location as LocationCurve;
           XYZ base1 = new XYZ();

           if (point.DistanceTo(lc.Curve.GetEndPoint(0)) > point.DistanceTo(lc.Curve.GetEndPoint(1)))
                return lc.Curve.GetEndPoint(1);
           else
                return  lc.Curve.GetEndPoint(0);
   
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

       public static MEPCurve nearestref(Document doc,  IList<Reference> sheeps,   Line cc)
       {
            List<MEPCurve> curves = new List<MEPCurve>();
            foreach (Reference sheep in sheeps)
            {
                Element elemp = doc.GetElement(sheep);
                curves.Add(elemp as MEPCurve);
            }
            return nearestref(curves, cc);
       }

        public static MEPCurve nearestref(List<MEPCurve> sheeps, Line cc)
        {
            MEPCurve mainpipe = sheeps.ElementAt(0);
            double closest = 1000;
            foreach (MEPCurve sheep in sheeps)
            {
                LocationCurve plc = sheep.Location as LocationCurve;
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

       public static XYZ farthestpipepoints(MEPCurve pipe, XYZ point)
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
           // Makepipes mp = new Makepipes();
           XYZ newbase = old + vector;
           TwoPoint tp = new TwoPoint(base1, newbase, mpcrv);
           Pipe pp = tp.Mepcurve as Pipe;
           if (pp != null)
           {
               TwoPoint npipe = Makepipes.Make_Pipe(tp, doc);    
           }
           Conduit ct = tp.Mepcurve as Conduit;
           if (ct != null)
           {
               TwoPoint nconduit = Makepipes.Make_Conduit(tp, doc);  
           }
            doc.Delete(mpcrv.Id);
       }


        public void buildpipevector(Document doc, MEPCurve pipe, XYZ base1, XYZ newbase)
        {
            // XYZ newbase = old + vector;

            Pipe pp = pipe as Pipe;
            if (pp != null)
            {
                Parameter diamz = pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                double diam = diamz.AsDouble();
                PipeType pt = pp.PipeType;
                Pipe pig2 = Pipe.Create(doc, pp.MEPSystem.Id, pp.PipeType.Id, pipe.LevelId, base1, newbase);
                pig2.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(diam);
                doc.Delete(pipe.Id);
            }

            Conduit ct = pipe as Conduit;
            if (ct != null)
            {
                FilteredElementCollector collector = new FilteredElementCollector(doc);
                collector.OfClass(typeof(ConduitType));
                ConduitType type = collector.FirstElement() as ConduitType;
                //  pipe.Level

                Parameter diamz = ct.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
                double diam = diamz.AsDouble();
                //  ConduitType condtype = ct.GetType() as ConduitType;

                Conduit pig2 = Conduit.Create(doc, type.Id, base1, newbase, pipe.LevelId);
                pig2.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(diam);
                doc.Delete(pipe.Id);
            }
        }

        private Element Intersectutil(ICollection<Element> pps, View3D view, XYZ pt, XYZ raydir, out XYZ intersection)
        {
            Element elout = null;
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
