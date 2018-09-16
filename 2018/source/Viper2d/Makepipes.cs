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
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure;
using System.Linq;
using Viper.Forms;
using Creation = Autodesk.Revit.Creation;
using Document = Autodesk.Revit.DB.Document;

namespace Viper
{


    class Makepipes
    {
        public List<TwoPoint> MAKE_PIPES_new(List<TwoPoint> listends, Document doc, ViperFormData vpdata, ElementId levelid)
        {
            PipeType pipeType = vpdata.Rpipetype;
            Level lev = doc.GetElement(levelid) as Level;

            if (null != pipeType)
            {
                foreach (TwoPoint tp in listends)
                {
                    // XYZ ptwo;
                    XYZ ptone;
                    XYZ ptwo;
                    if (tp.pipefunction == 2)
                    {
                        //drop found
                        ptwo = new XYZ(tp.pt2.X, tp.pt2.Y, lev.Elevation);
                        ptone = new XYZ(tp.pt1.X, tp.pt1.Y, lev.Elevation + vpdata.height);
                    }
                    else if (tp.pipefunction == 3)
                    {
                        //riser
                       double dest = getLevelabovelevel( levelid, doc);
                       ptwo = new XYZ(tp.pt2.X, tp.pt2.Y, dest);
                       ptone = new XYZ(tp.pt1.X, tp.pt1.Y, lev.Elevation + vpdata.height);  
                    }
                    else
                    {
                        ptwo = new XYZ(tp.pt2.X, tp.pt2.Y, tp.pt2.Z + vpdata.height);
                        ptone = new XYZ(tp.pt1.X, tp.pt1.Y, tp.pt1.Z + vpdata.height);
                    }
                    if (ptone.IsAlmostEqualTo(ptwo) == false)
                    {
                        Pipe pipe = ViParam.CompatPipe(doc, pipeType, ptone, ptwo);
                        pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(vpdata.diameter);
                        tp.pipe = pipe;
                        pipe.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).Set(levelid);
                    }
                }
            }
            else { TaskDialog.Show("error", "The pipetype is not valid - Makepipes line 67"); }
            return listends;
        }


        public static void MakeOrdered(Document doc, ViperFormData formData, SystemData systemData)
        {
            List<List<Connector>> conns = MakePipesProc(doc, formData, systemData);
            ConnectOrdered(doc, conns, systemData);
            MakeHeads(doc, conns, formData, systemData);
        }

        public static List<List<Connector>> MakePipesProc(Document doc, ViperFormData formData, SystemData data)
        {
            List<List<Connector>> conns = new List<List<Connector>>();
            var pipeType = formData.Rpipetype.Id;
            var sysid = ViParam.DefaultMEPSystemType(doc).Id;
            var levid = formData.level;

            using (Transaction tran2 = new Transaction(doc, "Viper"))
            {
                tran2.Start();
                foreach (var pp in data.geom)
                {
                    if (pp.Count == 6)
                    {
                        var xyz1 = new XYZ(pp[0], pp[1], pp[2]);
                        var xyz2 = new XYZ(pp[3], pp[4], pp[5]);
                        Pipe pipe = Pipe.Create(doc, sysid, pipeType, levid, xyz1, xyz2);
                        pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(formData.diameter);
                        Connector c1 = pipe.ConnectorManager.Lookup(0);
                        Connector c2 = pipe.ConnectorManager.Lookup(1);
                        conns.Add(new List<Connector>() { c1, c2 });
                    }
                }
                tran2.Commit();
            }
            return conns;
        }

        public static void ConnectOrdered(Document doc, List<List<Connector>> conns, SystemData data)
        {
            // transact to build pipe
            foreach (var toconn in data.indicies)
            {
                using (Transaction tran2 = new Transaction(doc, "Vipe2r"))
                {
                    tran2.Start();
                    try
                    {
                        if (toconn.Count == 4)
                            doc.Create.NewElbowFitting(conns[toconn[0]][toconn[1]], conns[toconn[2]][toconn[3]]);

                        else if (toconn.Count == 6)
                            doc.Create.NewTeeFitting(conns[toconn[0]][toconn[1]], conns[toconn[2]][toconn[3]], conns[toconn[4]][toconn[5]]);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(toconn.ToString() + e.ToString());
                    }
                    tran2.Commit();
                }
            }
        }

        public static PlanarFace FaceFromConnector(Connector conn)
        {
            Options opts = new Options();
            opts.ComputeReferences = true;
            GeometryElement obj = conn.Owner.get_Geometry(opts);
            
            var faces = new List<PlanarFace>();
            foreach (GeometryObject geom in obj)
            {
                if (null != geom as Solid)
                    foreach (Face geomFace in (geom as Solid).Faces)
                    {
                        if (geomFace as PlanarFace != null) 
                            faces.Add(geomFace as PlanarFace);
                    }
                
            }
            XYZ trs = conn.Origin;
            var result = faces.OrderBy(x => x.Project(trs).Distance);
            return result.FirstOrDefault();
        }

        public static void MakeHeads(Document doc, List<List<Connector>> conns, ViperFormData fd, SystemData data)
        {
            foreach (var toconn in data.symbols)
            {
                using (Transaction tran2 = new Transaction(doc, "Vip3r"))
                {
                    tran2.Start();
                    try
                    {
                        FamilySymbol sym = fd.GetHeadType((HeadType)toconn[0]);
                        sym.Activate();

                        Connector conn = conns[toconn[1]][toconn[2]];
                        PlanarFace fc = FaceFromConnector(conn);
                        doc.Create.NewFamilyInstance(fc, conn.Origin, fc.XVector,  sym);
                        tran2.Commit();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(toconn.ToString() + e.ToString());
                        tran2.RollBack();
                    }
                   
                }
            }
        }

        public List<TwoPoint> MAKE_Ducts(List<TwoPoint> listends, Document doc)
        {
            Transaction tran = new Transaction(doc, "writeparam");
            tran.Start();

            FilteredElementCollector ptypes = new FilteredElementCollector(doc);
            ptypes.OfClass(typeof(DuctType));
            DuctType rectduct = ptypes.FirstOrDefault(e => e.Name.Equals("Rect")) as DuctType;
            DuctType roundduct = ptypes.FirstOrDefault(e => e.Name.Equals("Round")) as DuctType;

            foreach (TwoPoint tp in listends)
            {
                if (tp.dimensionlist.Count == 2 || tp.dimensionlist.Count == 4)
                {
                    DuctType dtd = ptypes.FirstOrDefault(x => x.Name.Equals(tp.Revittypename)) as DuctType;
                    if (dtd == null)
                        rectduct.Duplicate(tp.Revittypename); 
                }
                else if (tp.dimensionlist.Count == 1)
                {
                    DuctType dtd = ptypes.FirstOrDefault(x => x.Name.Equals(tp.Revittypename)) as DuctType;
                    if (dtd == null)
                        roundduct.Duplicate(tp.Revittypename);
                }
            }
            tran.Commit();
            return listends;
        }

        // Get level above the levels on which the pipes (twopoint) are on
        private double getLevelabovelevel( ElementId levid, Document doc)
        {
            List<Level> levels = new FilteredElementCollector(doc)
             .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();
            Level known = doc.GetElement(levid) as Level;
            double levout = 1;//= ElementId levid;
            int ctr = 0;

            foreach (Level e in levels)
            {
                Level lev = e as Level;
                int n = levels.IndexOf(e);
                try
                {
                    if (known.Elevation == e.Elevation)
                    {
                        Level l = levels.ElementAt(n + 1);
                        levout = l.ProjectElevation;
                        break;
                    }
                    ctr++;
                }
                catch (Exception) { }
            }
            return levout;
        }

        // Get level above the levels on which the pipes (twopoint) are on
        private double getLevelabovepipe(TwoPoint tp, ElementId levid, Document doc)
        {
            List<Level> levels = new FilteredElementCollector(doc)
             .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();

            Level known = doc.GetElement(levid) as Level;
            double levout = 1;
            double pipehieght = tp.pt1.Z;
            int ctr = 0;

            foreach (Level e in levels)
            {
                Level lev = e as Level;
                int n = levels.IndexOf(e);
                try
                {
                    if (known.Elevation == e.Elevation)
                    {
                        Level l = levels.ElementAt(n + 1);
                        levout = l.Elevation;
                        break;
                    }
                    ctr++;
                }
                catch (Exception) { }
            }
            return levout;
        }

        public void rebuildpipes(Document doc, IList<Reference> targets, VPipeEndFormData vpdata)
        {
            foreach (Reference pr in targets)
            {
                Element elem = doc.GetElement(pr);
                Pipe pp = elem as Pipe;
                LocationCurve lc = pp.Location as LocationCurve;
                XYZ p1 = lc.Curve.GetEndPoint(0);
        		XYZ p2 = lc.Curve.GetEndPoint(1);

                GXYZ pbot = determinebot(p1, p2);
                GXYZ ptop = determinetop(p1, p2);

                GXYZ pboto;
                double zbot = pbot.Z;
                double ztop = ptop.Z;

                if (vpdata.bottomlevel != null)
                    zbot =  vpdata.bottomlevel.ProjectElevation;
                
                if (vpdata.toplevel != null)
                    ztop = vpdata.toplevel.ProjectElevation;
                
                if (vpdata.topoffset != 0)
                    zbot = zbot + vpdata.topoffset;
                
                if (vpdata.bottomoffset != 0)
                    ztop = ztop + vpdata.bottomoffset;

                XYZ bot = new XYZ(pbot.X, pbot.Y, zbot);
                XYZ top = new XYZ(pbot.X, pbot.Y, ztop);

                Parameter diamz = pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                double diam = diamz.AsDouble();
                PipeType pt = pp.PipeType;

                Pipe pig2 = Pipe.Create(doc, pp.MEPSystem.Id, pp.PipeType.Id, pp.LevelId, bot, top);
                pig2.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(diam);
                doc.Delete(pp.Id);
            }
        }

        public XYZ determinebot(XYZ p1, XYZ p2)
        {
            if (p1.Z > p2.Z)
                return p2;
            else
                return p1; 
        }

        public XYZ determinetop(XYZ p1, XYZ p2)
        {
            if (p2.Z > p1.Z)
                return p2;
            else
                return p1;
        }

        public void MAKE_PIPES_simple(List<TwoPoint> listends, Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));
            PipeType pipeType = collector.FirstElement() as PipeType;
            if (null != pipeType)
            {
                foreach (TwoPoint tp in listends)
                {
                    if (tp.Revittypename != null)
                    {
                        PipeType ptt = collector.FirstOrDefault(x => x.Name.Equals(tp.Revittypename)) as PipeType;
                        if (ptt != null)
                            pipemake(tp, doc, ptt);

                        else if (ptt == null)
                        {
                            PipeType ptn = pipeType.Duplicate(tp.Revittypename) as PipeType;
                            pipemake(tp, doc, ptn);
                        }
                    }
                    else
                        pipemake(tp, doc, pipeType);
                }
            }
        }

        public void pipemake(TwoPoint tp, Document doc, PipeType pipeType)
        {
            GXYZ np = new GXYZ(tp.pt1.X, tp.pt1.Y, tp.pt1.Z);
            GXYZ nz = new GXYZ(tp.pt2.X, tp.pt2.Y, tp.pt2.Z);
            Pipe pipe = ViParam.CompatPipe(doc, pipeType, np, nz);
            pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(tp.dimensionlist.ElementAt(0));
        }

        public static List<TwoPoint> MAKE_PIPES_general(List<TwoPoint> listends, Document doc)
        {
            List<TwoPoint> geolist = new List<TwoPoint>();
            foreach (TwoPoint tp in listends)
            {
                var tpnew = tp.BuildElement(doc);
                if (tpnew != null)
                    geolist.Add(tpnew);   
            }
            return geolist;
        }

        //adds the insulation to a new fitting or pipe from an existing fitting or pipe
        public static void Make_Insul(Pipe existing, Element newcurve, Document doc)
        {
            ICollection<ElementId> insulationIds = InsulationLiningBase.GetInsulationIds(doc, existing.Id);
            ElementId insulid = insulationIds.FirstOrDefault();

            if (insulid != null)
            {
                Element oldinsul = doc.GetElement(insulid);
                PipeInsulation oldinsulation = oldinsul as PipeInsulation;
                PipeInsulation.Create(doc, newcurve.Id, oldinsulation.GetTypeId(), oldinsulation.Thickness);
            }
        }


        public static TwoPoint Make_Pipe(TwoPoint tp, Document doc)
        {
            Pipe pp = tp.Mepcurve as Pipe;
            ElementId typid = tp.getTypeId();
            ElementId typlv = tp.getLevelId();
            ElementId typsys = tp.get_systemTypeId();
            Pipe pipe = Pipe.Create(doc, typsys, typid, typlv, tp.pt1, tp.pt2);
            pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble());
            Make_Insul(pp, pipe, doc);
            TwoPoint tpout = new TwoPoint(tp.pt1, tp.pt2, pipe as MEPCurve);
            return tpout;
        }

        public static TwoPoint Make_Conduit(TwoPoint tp, Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ConduitType));
            ConduitType type = collector.FirstElement() as ConduitType;
      
            Conduit ct = tp.Mepcurve as Conduit;
            GXYZ np = new GXYZ(tp.pt1.X, tp.pt1.Y, tp.pt1.Z);
            GXYZ nz = new GXYZ(tp.pt2.X, tp.pt2.Y, tp.pt2.Z);

            Parameter diamz = ct.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
            double diam = diamz.AsDouble();
      
            Conduit pig2 = Conduit.Create(doc, type.Id, np, nz, ct.LevelId);
            pig2.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(diam);

            TwoPoint tpout = new TwoPoint(np, nz, pig2);

            return tpout;
        }


        //Pipe connection Module - MAIN
        /// /////////////////////////////////////////////////
        public void Connectsystemtree(List<TwoPoint> pipelist, Document doc, XYZ point)
        {
            List<TwoPoint> pipelistsort = getfirstpipe(pipelist, point);
            TwoPoint tpl = pipelist.ElementAt(0);
            twopointlist tplist = new twopointlist(pipelist);
            List<Connector> allcons = allconnectors(pipelistsort);

            //cycle through the main
            List<Branch> branches = new List<Branch>();
            conectorsall(doc, allcons);
        }

        public void Connectsystemtree2(List<TwoPoint> pipelist, Document doc, XYZ point)
        {
            List<Connector> allcons = allconnectors(pipelist);
            conectorsall(doc, allcons);
        }

        public void RecursiveConnectsystemtree(List<TwoPoint> pipelist, Document doc, XYZ point)
        {
            List<TwoPoint> pipelistsort = getfirstpipe(pipelist, point);
            TwoPoint tpl = pipelist.ElementAt(0);
            twopointlist tplist = new twopointlist(pipelist);
            List<Connector> allcons = allconnectors(pipelistsort);
            List<Branch> branches = new List<Branch>();
        }

        public void conectorsall(Document doc, List<Connector> allcons)
        {
            try
            {
                for (int i = 0; i < allcons.Count; i++)
                {
                    Connector knowncon = allcons.ElementAt(i);
                    try
                    {
                        for (int j = 0; j < allcons.Count; j++)
                        {
                            Connector candidatecon = allcons.ElementAt(j);
                            bool iswithindistance = TESTdistancepointpoint(knowncon, candidatecon);

                            if (iswithindistance == true 
                                && knowncon.IsConnected == false 
                                && candidatecon.IsConnected == false
                                && candidatecon.Owner != knowncon.Owner)
                            {
                                Transaction trans = new Transaction(doc, "makeconnector");
                                trans.Start();
                                try
                                {
                                    FamilyInstance fi = doc.Create.NewElbowFitting(knowncon, candidatecon);
                                    trans.Commit();
                                    allcons.Remove(candidatecon);
                                    allcons.Remove(knowncon);
                                }
                                catch (Exception)
                                {
                                    trans.RollBack();
                                    trans.Dispose();
                                    continue;
                                }
                            }
                        }
                        allcons.Remove(knowncon);
                    }


                    catch (Exception) {  }
                }
            }
            catch (Exception) {  }

        }

        public List<TwoPoint> Onaxistree(List<TwoPoint> pipelist, Document doc, XYZ point, ViperFormData vpdata)
        {
            List<TwoPoint> pipelistsort = getfirstpipe(pipelist, point);
            TwoPoint tpl = pipelist.ElementAt(0);
            twopointlist tplist = new twopointlist(pipelistsort);
            tplist.todo = pipelistsort;
            ephialtes(pipelistsort, vpdata);
            return tplist.mainlist;
        }
    
        public List<TwoPoint> recursiveSome(TwoPoint first, List<TwoPoint> inputlist,  List<TwoPoint> brout)
        {
             foreach (TwoPoint test in inputlist)
             {
                 if (test != first)
                 {
                     if (TESTdistance2point2point(first, test, 1) == true)
                     {
                         inputlist.Remove(test);
                         brout.Add(test);
                         brout = recursiveSome(test, inputlist, brout);

                     }
                 }
             }
             return brout;
         }       

        #region Helper Methods

        public void ephialtes(List<TwoPoint> tplist, ViperFormData vpdata)
        {
            //int i = 0;
            int tplength = tplist.Count;

            for (int i = 0; i < tplist.Count; i++)
            {
                try
                {
                    bool bl = false;
                    TwoPoint known = tplist.ElementAt(i);

                    for (int j = 0; j < tplist.Count; j++)
                    {
                        TwoPoint candidate = tplist.ElementAt(j);
                        if (candidate != known)
                        {
                            bool iswithindistance = TESTdistance2point2point(known, candidate, vpdata.Thrsh_straight); //vpdata.Thrsh_elbow);
                            bool isonaxis = TESTtwopointOnSameAxis(known, candidate);
                            if (isonaxis == true && iswithindistance == true)
                            {
                                TwoPoint tpnew = TwoPointRebuild(known, candidate);
                                tplist.Remove(candidate);
                                tplist.Remove(known);
                                tplist.Insert(0, tpnew);
                                bl = true;
                                break;
                            }
                            else   { }
                        }
                    }
                    if (bl == true)
                    {break; }
                }
               catch (Exception) { }
            }

            if (tplist.Count == tplength)
            { }
            else
            { ephialtes(tplist, vpdata); }
        }


        //which connector on a line is closest to the given connector
        public Connector thiscon(Pipe tp, Connector cn)
        {
            Connector cnout;
            ConnectorSetIterator csi = tp.ConnectorManager.Connectors.ForwardIterator();
            csi.MoveNext();
            Connector con1 = csi.Current as Connector;
            csi.MoveNext();
            Connector con2 = csi.Current as Connector;

            if (cn.Origin.DistanceTo(con1.Origin) < cn.Origin.DistanceTo(con2.Origin))
                return con1;
            else
                return con2;
        }

  
        //TEST TWO POINTS
        ////////////////////////////////////////////////////////////////////
        //test if the pipes are on the same axis and pepedicular
        public bool TESTtwopointOnSameAxis(TwoPoint known, TwoPoint test)
        {

            bool bl = false;
            ///http://math.stackexchange.com/questions/103065/how-to-test-any-2-line-segments-3d-are-collinear-or-not
            //AB || CD
            // FUCKING LINEAR ALGEBRA

            XYZ A = known.pt1 ;
            XYZ B = known.pt2 ;
            XYZ C =  test.pt1;
            XYZ D =  test.pt2 ;

            double dx1 = B.X - A.X;
            double dy1 = B.Y - A.Y;
            double dx2 = D.X - C.X;
            double dy2 = D.Y - C.Y;

            double Dx1 = C.X - A.X;
            double Dy1 = C.Y - A.Y;
            double Dx2 = D.X - B.X;
            double Dy2 = D.Y - B.Y;

            double Ddx1 = D.X - A.X;
            double Ddy1 = D.Y - A.Y;
            double Ddx2 = C.X - B.X;
            double Ddy2 = C.Y - B.Y;

            double cosAngle = Math.Abs((dx1 * dx2 + dy1 * dy2) 
                / Math.Sqrt((dx1 * dx1 + dy1 * dy1)
                * (dx2 * dx2 + dy2 * dy2)));

            double cosAngle2 = Math.Abs((Dx1 * Dx2 + Dy1 * Dy2)
                          / Math.Sqrt((Dx1 * Dx1 + Dy1 * Dy1)
                          * (Dx2 * Dx2 + Dy2 * Dy2)));

            double cosAngle3 = Math.Abs((Ddx1 * Ddx2 + Ddy1 * Ddy2)
                                    / Math.Sqrt((Ddx1 * Ddx1 + Ddy1 * Ddy1)
                                    * (Ddx2 * Ddx2 + Ddy2 * Ddy2)));

            if (cosAngle > 0.999 && cosAngle2 > 0.999 && cosAngle3 > 0.999) 
            {
                bl = true;
            }
            return bl;
        }

        public TwoPoint TwoPointRebuild (TwoPoint known, TwoPoint candidate)
        {
            doublenum dist11 = new doublenum(known.pt1.DistanceTo(candidate.pt1), "Dist11",
               known.pt1, candidate.pt1 );
            doublenum dist12 = new doublenum(known.pt1.DistanceTo(candidate.pt2), "Dist12",
                known.pt1, candidate.pt2 );
            doublenum dist21 = new doublenum(known.pt2.DistanceTo(candidate.pt1), "Dist21",
                known.pt2, candidate.pt1 );
            doublenum dist22 = new doublenum(known.pt2.DistanceTo(candidate.pt2), "Dist22",
                known.pt2, candidate.pt2 );

            List<doublenum> dl = new List<doublenum>() { dist11, dist12, dist21, dist22 };
            dl.Sort((emp1, emp2) => emp1.num.CompareTo(emp2.num));
            TwoPoint newtp = new TwoPoint(dl.ElementAt(3).pt1, dl.ElementAt(3).pt2, known.layer , 1);
            return newtp;
        }


        public bool TESTdistance2point2point(TwoPoint known, TwoPoint candidate, double thresDist)
        {
            if (known.MinEndpointDistance(candidate) < thresDist)
                return true;
            else
                return false;
        }


        //test if the distance of the current 
        public List<Connector> TESTdistancepointtoline(List<Connector> points, Pipe pp, double threshold)
        {
            LocationCurve lc = pp.Location as LocationCurve;
            Line pc = lc.Curve as Line;
            //List<Connector> conlist = GetPipeconnectors(pp);

            List<Connector> candidateconnectors = new List<Connector>();
            foreach (Connector cc in points)
            {
                if (pc.Distance(cc.Origin) <= threshold
                    && cc.Owner != pp

                    )
                {

                    candidateconnectors.Add(cc);
                }
            }
            return candidateconnectors;
        }

        //test if connectors are within the right distance
        public bool TESTdistancepointpoint(Connector c1, Connector c2)
        {
            bool bl = false;
            if (c2.Origin.DistanceTo(c1.Origin) < .1)
            { bl = true; }

            return bl;
        }


        /// <summary>
        /// Pipe Creation Helper Methods
        /// </summary>
        /// <param name="pipelist"></param>
        /// <param name="point"></param>
        /// <returns></returns>
    
        #endregion


        #region Done Methods

        public List<TwoPoint> getfirstpipe(List<TwoPoint> pipelist, XYZ point)
        {
            TwoPoint closest = pipelist.ElementAt(0);

            foreach (TwoPoint tp in pipelist)
            {
                closest = HelperMethods.testdist(closest, point, tp);
            }
            pipelist.Remove(closest);
            pipelist.Insert(0, closest);

            return pipelist;
        }

        //xxxxxxxxxxxxxxxxx
        public List<Connector> allends(List<TwoPoint> pipelist)
        {
            List<Connector> allconector = new List<Connector>();
            foreach (TwoPoint tp in pipelist)
            {
                allconector.AddRange(GetPipeconnectors(tp.pipe));
            }

            return allconector;
        }

        /// <summary>
        /// get all the connectors in the newly created twopoint set
        /// </summary>
        /// <param name="pipelist"></param>
        /// <returns></returns>
        public List<Connector> allconnectors(List<TwoPoint> pipelist)
        {
            List<Connector> allconector = new List<Connector>();
            foreach (TwoPoint tp in pipelist)
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

        #endregion



    }

    //class to carry lists of twopoints
    public class twopointlist
    {
        public List<TwoPoint> mainlist { get; set; }
        public List<TwoPoint> todo { get; set; }
        public List<TwoPoint> done { get; set; }

        public twopointlist(List<TwoPoint> Mainlist)
        {
            mainlist = Mainlist;
        }

    }


    // misc helper classes
    class doublenum
    {
        public double num {get; set;}
       public string name { get; set; }
       public XYZ pt1 { get; set; }
       public XYZ pt2 { get; set; }

        public doublenum(double Num, string Name, XYZ point1, XYZ point2)
        {
            num = Num;
            name = Name;
            pt1 = point1;
            pt2 = point2;
        }

    }
}
