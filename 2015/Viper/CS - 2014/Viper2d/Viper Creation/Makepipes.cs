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
using System.Linq;

namespace Revit.SDK.Samples.UIAPI.CS
{
   

    class Makepipes
    {
        #region Make Pipes
        /// <MAKE PIPES Module>
        /// simple makepipes module
        /// </simple makepipes moduley>
        /// <param name="listends"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        public List<twopoint> MAKE_PIPES_new(List<twopoint> listends,Document doc, ViperFormData vpdata, ElementId levelid)
        {
            PipeType pipeType = vpdata.Rpipetype;
            Level lev = doc.GetElement(levelid) as Level;

            if (null != pipeType)
            {
                foreach (twopoint tp in listends)
                {
                   // XYZ ptwo;

                    if (tp.pipefunction == 2)
                    {
                        //TaskDialog.Show("SD", "drop found");
                      //  double dest = getLevelabovepipe(tp, levelid, doc);
                        XYZ ptwo = new XYZ(tp.pt2.X, tp.pt2.Y, lev.Elevation);
                        XYZ ptone = new XYZ(tp.pt1.X, tp.pt1.Y, lev.Elevation + vpdata.height);
                        Pipe pipe = doc.Create.NewPipe(ptone, ptwo, pipeType);
                        pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(vpdata.diameter);
                        tp.pipe = pipe;
                        pipe.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).Set(levelid);
                    }


                    if (tp.pipefunction == 3)
                    {
                        //riser
                        //TaskDialog.Show("SD", "riser found");
                       double dest = getLevelabovelevel( levelid, doc);
                       XYZ ptwo = new XYZ(tp.pt2.X, tp.pt2.Y, dest);
                        XYZ ptone = new XYZ(tp.pt1.X, tp.pt1.Y, lev.Elevation + vpdata.height);
                        Pipe pipe = doc.Create.NewPipe(ptone, ptwo, pipeType);
                        pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(vpdata.diameter);
                        tp.pipe = pipe;
                        pipe.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).Set(levelid);
                    }
                    else
                    {
                        XYZ ptwo = new XYZ(tp.pt2.X, tp.pt2.Y, tp.pt2.Z + vpdata.height);
                        XYZ ptone = new XYZ(tp.pt1.X, tp.pt1.Y, tp.pt1.Z + vpdata.height);
                        Pipe pipe = doc.Create.NewPipe(ptone, ptwo, pipeType);
                        pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(vpdata.diameter);
                        tp.pipe = pipe;
                        pipe.get_Parameter(BuiltInParameter.RBS_START_LEVEL_PARAM).Set(levelid);
                    }
                }
            }
            else { TaskDialog.Show("error", "The pipetype is not valid - Makepipes line 67"); }
            return listends;
        }

        public List<twopoint> MAKE_Ducts (List<twopoint> listends, Document doc)
        {
            Transaction tran = new Transaction(doc, "writeparam");
            tran.Start();

            Autodesk.Revit.DB.FilteredElementCollector ptypes = new Autodesk.Revit.DB.FilteredElementCollector(doc);
            ptypes.OfClass(typeof(DuctType));
          //  DuctType dtt = ptypes.FirstOrDefault() as DuctType;
            DuctType rectduct = ptypes.FirstOrDefault(e => e.Name.Equals("Rect")) as DuctType;
		    DuctType roundduct = ptypes.FirstOrDefault(e => e.Name.Equals("Round")) as DuctType;

       
                foreach (twopoint tp in listends)
                {

                    if (tp.dimensionlist.Count == 2 || tp.dimensionlist.Count == 4)
                    {
                        DuctType dtd = ptypes.FirstOrDefault(x => x.Name.Equals(tp.Revittypename)) as DuctType;
                        if (dtd == null)
                        {  rectduct.Duplicate(tp.Revittypename);    }
                        Duct dt = doc.Create.NewDuct(tp.pt1, tp.pt2, dtd);
                        dt.get_Parameter(BuiltInParameter.RBS_CURVE_HEIGHT_PARAM).Set(tp.dimensionlist.ElementAt(1));
                        dt.get_Parameter(BuiltInParameter.RBS_CURVE_WIDTH_PARAM).Set(tp.dimensionlist.ElementAt(0));
                        tp.duct = dt;
                    }


                   else if (tp.dimensionlist.Count == 1)
                    {
                        DuctType dtd = ptypes.FirstOrDefault(x => x.Name.Equals(tp.Revittypename)) as DuctType;
                        if (dtd == null)
                        { roundduct.Duplicate(tp.Revittypename); }
                        Duct dt = doc.Create.NewDuct(tp.pt1, tp.pt2, dtd);
                        dt.get_Parameter(BuiltInParameter.RBS_CURVE_DIAMETER_PARAM).Set(tp.dimensionlist.ElementAt(0));
                        tp.duct = dt;
                    }
                }
            
            //else { TaskDialog.Show("error", "The pipetype is not valid - Makepipes line 67"); }
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
                // TaskDialog.Show("90", n.ToString() + " - " + e.Elevation.ToString() + "  " + known.Elevation.ToString());

                try
                {

                    if (known.Elevation == e.Elevation)
                    {

                        Level l = levels.ElementAt(n + 1);
                        levout = l.ProjectElevation;
                        // TaskDialog.Show("Elevation", levout.ToString());
                        break;
                    }

                    ctr++;
                }
                catch (Exception) { }
            }

            return levout;
        }

        // Get level above the levels on which the pipes (twopoint) are on
        private double getLevelabovepipe(twopoint tp, ElementId levid, Document doc)
        {
            List<Level> levels = new FilteredElementCollector(doc)
             .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();

            Level known = doc.GetElement(levid) as Level;

             double levout = 1;//= ElementId levid;
             double pipehieght = tp.pt1.Z;
             int ctr = 0;

             foreach (Level e in levels)
                {
                      Level lev = e as Level;
                      int n = levels.IndexOf(e);
                   // TaskDialog.Show("90", n.ToString() + " - " + e.Elevation.ToString() + "  " + known.Elevation.ToString());

                    try
                    {

                    if (known.Elevation == e.Elevation)
                    {
                        
                        Level l = levels.ElementAt(n + 1);
                        levout = l.Elevation;
                       // TaskDialog.Show("Elevation", levout.ToString());
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
                {
                    zbot =  vpdata.bottomlevel.ProjectElevation;
                }
                if (vpdata.toplevel != null)
                {
                    ztop = vpdata.toplevel.ProjectElevation;
                }
                if (vpdata.topoffset != 0)
                {
                    zbot = zbot + vpdata.topoffset;
                }
                if (vpdata.bottomoffset != 0)
                {
                    ztop = ztop + vpdata.bottomoffset;
                }
              
                XYZ bot = new XYZ(pbot.X, pbot.Y, zbot);
                XYZ top = new XYZ(pbot.X, pbot.Y, ztop);

                Autodesk.Revit.DB.Parameter diamz = pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);
                double diam = diamz.AsDouble();
                PipeType pt = pp.PipeType;

                Pipe pig2 = doc.Create.NewPipe(bot, top, pt);
                pig2.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(diam);
                pig2.SetSystemType(pp.MEPSystem.GetTypeId());

                doc.Delete(pp.Id);

            }
        }

     //   public void 


        public XYZ determinebot(XYZ p1, XYZ p2)
        {
            XYZ bottom ;//= new XYZ();

            if (p1.Z > p2.Z)
            {
                bottom = p2;
            }
            else { bottom = p1; }

            return bottom;
        }

        public XYZ determinetop(XYZ p1, XYZ p2)
        {
            XYZ top;//= new XYZ();

            if (p2.Z > p1.Z)
            {
                top = p2;
            }
            else { top = p1; }

            return top;
        }



        // make pipes from a list of points
        public void MAKE_PIPES_nsimple(List<GXYZ> listends, Document doc)
        {

            Autodesk.Revit.DB.FilteredElementCollector collector
                = new Autodesk.Revit.DB.FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));
            PipeType pipeType = collector.FirstElement() as PipeType;

            ElementId id = pipeType.Id;
            Autodesk.Revit.DB.FilteredElementCollector levs =
                new Autodesk.Revit.DB.FilteredElementCollector(doc);
            levs.OfClass(typeof(Level));

            Level lev = levs.FirstElement() as Level;
            ElementId levid = lev.Id;



            Transaction trans = new Transaction(doc, "tt");
            trans.Start();
            if (null != pipeType)
            {
                foreach (GXYZ tp in listends)
                {
                    GXYZ np = new GXYZ(tp.X, tp.Y, tp.Z + 2);

                    Pipe pipe = doc.Create.NewPipe(tp, np, pipeType);
                    pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(0.16667);

                }
            }
            trans.Commit();

        }


        public void MAKE_PIPES_simple(List<twopoint> listends, Document doc)
        {

            Autodesk.Revit.DB.FilteredElementCollector collector = new Autodesk.Revit.DB.FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));
            PipeType pipeType = collector.FirstElement() as PipeType;

            // Transaction trans = new Transaction(doc, "tt");
            //  trans.Start();
            if (null != pipeType)
            {
                foreach (twopoint tp in listends)
                {
                    if (tp.Revittypename != null)
                    {
                        PipeType ptt = collector.FirstOrDefault(x => x.Name.Equals(tp.Revittypename)) as PipeType;
                        if (ptt != null)
                        {
                            pipemake(tp, doc, ptt);
                        }
                        else if (ptt == null)
                        {
                            PipeType ptn = pipeType.Duplicate(tp.Revittypename) as PipeType;
                            pipemake(tp, doc, ptn);
                        }
                    }
                    else
                    {
                        pipemake(tp, doc, pipeType);
                    }
                }
            }
            //  trans.Commit();
        }

        public void pipemake(twopoint tp, Document doc, PipeType pipeType)
        {
            GXYZ np = new GXYZ(tp.pt1.X, tp.pt1.Y, tp.pt1.Z);
            GXYZ nz = new GXYZ(tp.pt2.X, tp.pt2.Y, tp.pt2.Z);

            Pipe pipe = doc.Create.NewPipe(np, nz, pipeType);
            pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(tp.dimensionlist.ElementAt(0));

        }

        public List<twopoint> MAKE_PIPES_general(List<twopoint> listends, Document doc)
        {
            List<twopoint> geolist = new List<twopoint>();

            foreach (twopoint tp in listends)
                {
                    if ( tp.Mepcurve != null)
                    {
                    MEPCurve mepcurve = tp.Mepcurve;

                    Pipe testp = mepcurve as Pipe;
                    if (testp != null)
                    {
                      twopoint tpnew =  Make_Pipe(tp, doc);
                      geolist.Add(tpnew);
                    }

                    Conduit testc = mepcurve as Conduit;
                    if (testc != null)
                    {
                        twopoint tpnew = Make_Conduit(tp, doc);
                        geolist.Add(tpnew);
                    }

                    Duct testd= mepcurve as Duct;
                    if (testd != null)
                    {
                        twopoint tpnew = Make_Duct(tp, doc);
                        geolist.Add(tpnew);
                    }
                }
            }
            return geolist;
        }

        //adds the insulation to a new fitting or pipe from an existing fitting or pipe
        public void Make_Insul(Pipe existing, Element newcurve, Document doc)
        {
            ICollection<ElementId> insulationIds = 
                InsulationLiningBase.GetInsulationIds(doc, existing.Id);
            ElementId insulid = insulationIds.FirstOrDefault();

            if (insulid != null)
            {
                Element oldinsul = doc.GetElement(insulid);
                PipeInsulation oldinsulation = oldinsul as PipeInsulation;
                PipeInsulation.Create(doc, newcurve.Id, 
                    oldinsulation.GetTypeId(), oldinsulation.Thickness);
            }
        }

        public twopoint Make_Pipe(twopoint tp, Document doc)
        {
                Pipe pp = tp.Mepcurve as Pipe;
                MEPSystem mep = pp.MEPSystem;
         
                Pipe pipe = doc.Create.NewPipe( tp.pt1, tp.pt2, pp.PipeType);
                pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble());
                Make_Insul(pp, pipe, doc);

                pipe.SetSystemType(mep.GetTypeId());
                twopoint tpout = new twopoint(tp.pt1, tp.pt2, pipe as MEPCurve);
                return tpout;
        }

        public twopoint Make_Conduit (twopoint tp, Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Autodesk.Revit.DB.Electrical.ConduitType));
            Autodesk.Revit.DB.Electrical.ConduitType type = collector.FirstElement() as Autodesk.Revit.DB.Electrical.ConduitType;
      
            Conduit ct = tp.Mepcurve as Conduit;
            GXYZ np = new GXYZ(tp.pt1.X, tp.pt1.Y, tp.pt1.Z);
            GXYZ nz = new GXYZ(tp.pt2.X, tp.pt2.Y, tp.pt2.Z);

            Autodesk.Revit.DB.Parameter diamz = ct.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
            double diam = diamz.AsDouble();
      
            Conduit pig2 = Autodesk.Revit.DB.Electrical.Conduit.Create(doc, type.Id, np, nz, ct.LevelId);
            pig2.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(diam);

            
            twopoint tpout = new twopoint(np, nz, pig2);

            return tpout;
        }

        /// <summary>
        /// Creates and returns mepcurve containing a duct
        /// </summary>
        /// <param name="tp"></param>
        /// <param name="doc"></param>
        /// <param name="pipeType"></param>
        /// <returns></returns>
        public twopoint Make_Duct(twopoint tp, Document doc)
        {


            //GXYZ np = new GXYZ(tp.pt1.X, tp.pt1.Y, tp.pt1.Z);
            //GXYZ nz = new GXYZ(tp.pt2.X, tp.pt2.Y, tp.pt2.Z);

            //Pipe pipe = doc.Create.NewPipe(np, nz, tp.);
            //pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(tp.dimensionlist.ElementAt(0));
            //twopoint tpout = new twopoint(np, nz, pipe);

            return tp;
        }


        

        public void MakeBranchTee(Branch branch, Document doc)
        {
            // TaskDialog.Show("sad", "Type 1 branch");
            // branch.firstconnector

            // branch.mainpipe.

            // Find the nearest connector on the owner to the branches first connector
            //  if (.First().Origin.DistanceTo(branch.firstconnector.Origin) >=
            //           conlist.Last().Origin.DistanceTo(branch.firstconnector.Origin))


        }

        public void MakeBranchRaised()
        {


        }

        public void MakeBranchRolled()
        {


        }

        #endregion


        //Pipe connection Module - MAIN
        /// /////////////////////////////////////////////////
   
        public void Connectsystemtree(List<twopoint> pipelist, Document doc, XYZ point)
        {
            List<twopoint> pipelistsort = getfirstpipe(pipelist, point);
            twopoint tpl = pipelist.ElementAt(0);
            twopointlist tplist = new twopointlist(pipelist);
            //-----------------------------
            List<Connector> allcons = allconnectors(pipelistsort);

            //cycle through the main
            List<Branch> branches = new List<Branch>();
          //  tpl.MakeTreeNode(allcons.ElementAt(0), doc, allcons, branches, pipelist);
            conectorsall(doc, allcons);

            #region branchhandling
            //cycle through each branch
            //foreach (Branch branch in branches)
            //{

            //    List<Branch> subbranches = new List<Branch>();
            //    tpl.MakeTreeNode(branch.firstpipe, doc, allcons, subbranches);

            //    //create connection type

            //    // Connection 1 = regular T 
            //    if (branch.branchconnection == 1)
            //    {

            //        MakeBranchTee(branch, doc);
            //    }
            //    // Connection 2 = T with top thing 
            //    if (branch.branchconnection == 2)
            //    {

            //    }
            //    // Connection 3 = 45 Rolled T
            //    if (branch.branchconnection == 3)
            //    {

            //    }

            //    //MakeTreeNode(/branch.pipe, doc, allcons)
            //}
            #endregion
        }

        public void Connectsystemtree2(List<twopoint> pipelist, Document doc, XYZ point)
        {
            List<Connector> allcons = allconnectors(pipelist);
            conectorsall(doc, allcons);
        }

        public void RecursiveConnectsystemtree(List<twopoint> pipelist, Document doc, XYZ point)
        {
            List<twopoint> pipelistsort = getfirstpipe(pipelist, point);
            twopoint tpl = pipelist.ElementAt(0);
            twopointlist tplist = new twopointlist(pipelist);
            //-----------------------------
            List<Connector> allcons = allconnectors(pipelistsort);

            //cycle through the main
            List<Branch> branches = new List<Branch>();
          //    tpl.MakeTreeNode(allcons.ElementAt(0), doc, allcons, branches, pipelist);
          //  conectorsall(doc, allcons);

            #region branchhandling
            //cycle through each branch
            //foreach (Branch branch in branches)
            //{

            //    List<Branch> subbranches = new List<Branch>();
            //    tpl.MakeTreeNode(branch.firstpipe, doc, allcons, subbranches);

            //    //create connection type

            //    // Connection 1 = regular T 
            //    if (branch.branchconnection == 1)
            //    {

            //        MakeBranchTee(branch, doc);
            //    }
            //    // Connection 2 = T with top thing 
            //    if (branch.branchconnection == 2)
            //    {

            //    }
            //    // Connection 3 = 45 Rolled T
            //    if (branch.branchconnection == 3)
            //    {

            //    }

            //    //MakeTreeNode(/branch.pipe, doc, allcons)
            //}
            #endregion
        }



        public void conectorsall(Autodesk.Revit.DB.Document doc, List<Connector> allcons)
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
                                    FlowDirectionType fd = FlowDirectionType.Bidirectional;
                                        fd = FlowDirectionType.Bidirectional;

                                    FamilyInstance fi = doc.Create.NewElbowFitting(knowncon, candidatecon);
                                    //if (fi == null)
                                    //{
                                    //    FlowDirectionType fd = FlowDirectionType.Bidirectional;
                                    //    fd = FlowDirectionType.Bidirectional;
                                    //}

                                    trans.Commit();
                                    allcons.Remove(candidatecon);
                                    allcons.Remove(knowncon);
                                    //MakeTreeNode(allcons.ElementAt(0), doc,
                                    //     allcons, branches, pipelist);
                                }
                                catch (Exception)
                                {
                                    trans.RollBack();
                                    trans.Dispose();
                                    //  TaskDialog.Show("as", "roll");
                                    continue;
                                }


                            }
                            else { }

                        }
                        allcons.Remove(knowncon);
                    }


                    catch (Exception) {  }
                }
            }
            catch (Exception) {  }

        }

        public List<twopoint> Onaxistree(List<twopoint> pipelist, Document doc, XYZ point, ViperFormData vpdata)
        {

            List<twopoint> pipelistsort = getfirstpipe(pipelist, point);
            twopoint tpl = pipelist.ElementAt(0);
            twopointlist tplist = new twopointlist(pipelistsort);

            tplist.todo = pipelistsort;
            ephialtes(pipelistsort, vpdata);

            return tplist.mainlist;
        }

        public List<twopoint> recursiveSome(twopoint first, List<twopoint> inputlist,  List<twopoint> brout)
         {

             foreach (twopoint test in inputlist)
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


        //recursive function which will run tests on a list
        //if a change is made, it will restart.
        //if no change is made to the list during hte iteration, it will stop
        public void ephialtes(List<twopoint> tplist, ViperFormData vpdata)
        {
            int tplength = tplist.Count;

            for (int i = 0; i < tplist.Count; i++)
            {
                try
                {
                    bool bl = false;
                    twopoint known = tplist.ElementAt(i);

                    for (int j = 0; j < tplist.Count; j++)
                    {
                        twopoint candidate = tplist.ElementAt(j);
                        if (candidate != known)
                        {
                            bool iswithindistance = TESTdistance2point2point(known, candidate, vpdata.Thrsh_straight); //vpdata.Thrsh_elbow);
                            bool isonaxis = TESTtwopointOnSameAxis(known, candidate);
                            if (isonaxis == true && iswithindistance == true)
                            {
                                twopoint tpnew = TwoPointRebuild(known, candidate);
                                tplist.Remove(candidate);
                                tplist.Remove(known);
                                tplist.Insert(0, tpnew);
                                bl = true;
                                break;
                            }

                            //else if (iswithindistance == true && known.pipefunction != candidate.pipefunction)
                            //{
                            //    twopoint tpnew = TwoPointRebuildfarthest(known, candidate);
                            //    tplist.Insert(0, tpnew);
                            //    bl = true;
                            //    break;
                            //}

                            else { }
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

        //Pipe connection Module//// NOT USES
        public twopoint getnearestpipe(List<twopoint> pipelist, twopoint pipe)
        {

            twopoint closest = pipelist.ElementAt(0);
            HelperMethods hl = new HelperMethods();

            foreach (twopoint tp in pipelist)
            {
                if (tp != pipe)
                {


                }
            }
            pipelist.Remove(closest);
            pipelist.Insert(0, closest);

            return closest;
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
            {
                cnout = con1;
            }
            else
            {
                cnout = con2;
            }

            return cnout;
        }

  
  
        //TEST TWO POINTS
        ////////////////////////////////////////////////////////////////////
        //test if the pipes are on the same axis and pepedicular
        public bool TESTtwopointOnSameAxis(twopoint known, twopoint test)
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
            else { }
        
         
            return bl;
        }

        public twopoint TwoPointRebuild (twopoint known, twopoint candidate)
        {
           // if(TESTdistance2point2point(known, candidate, thresDist) = true)
          //  {

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


            twopoint newtp = new twopoint(dl.ElementAt(3).pt1, dl.ElementAt(3).pt2, known.layer , 1);

            return newtp;
        }

        public twopoint TwoPointRebuildvert_horz(twopoint known, twopoint candidate)
        {
            // if(TESTdistance2point2point(known, candidate, thresDist) = true)
            //  {

            doublenum dist11 = new doublenum(known.pt1.DistanceTo(candidate.pt1), "Dist11",
               known.pt1, candidate.pt1);
            doublenum dist12 = new doublenum(known.pt1.DistanceTo(candidate.pt2), "Dist12",
                known.pt1, candidate.pt2);
            doublenum dist21 = new doublenum(known.pt2.DistanceTo(candidate.pt1), "Dist21",
                known.pt2, candidate.pt1);
            doublenum dist22 = new doublenum(known.pt2.DistanceTo(candidate.pt2), "Dist22",
                known.pt2, candidate.pt2);

            List<doublenum> dl = new List<doublenum>() { dist11, dist12, dist21, dist22 };
            dl.Sort((emp1, emp2) => emp1.num.CompareTo(emp2.num));


            twopoint newtp = new twopoint(dl.ElementAt(0).pt1, dl.ElementAt(0).pt2, known.layer, 1);

            return newtp;
        }




        public bool TESTdistance2point2point(twopoint known, twopoint candidate, double thresDist)
        {
            bool bl = false;
            if (known.pt1.DistanceTo(candidate.pt1) < thresDist
                || known.pt2.DistanceTo(candidate.pt1) < thresDist
                || known.pt1.DistanceTo(candidate.pt2) < thresDist
                || known.pt2.DistanceTo(candidate.pt2) < thresDist)
            {
                bl = true;

            }
            else { }

            return bl;
        }


        //test if the distance of the current 
        public List<Connector> TESTdistancepointtoline(List<Connector> points, Pipe pp, double threshold)
        {
            LocationCurve lc = pp.Location as LocationCurve;
            Autodesk.Revit.DB.Line pc = lc.Curve as Autodesk.Revit.DB.Line;
            //List<Connector> conlist = GetPipeconnectors(pp);

            List<Connector> candidateconnectors = new List<Connector>();
            foreach (Connector cc in points)
            {
                if (pc.Distance(cc.Origin) <= threshold
                    && cc.Owner != pp
                    //  && conlist.ElementAt(1).Origin.DistanceTo(cc.Origin) <= .001
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




        public List<twopoint> getfirstpipe(List<twopoint> pipelist, XYZ point)
        {
            twopoint closest = pipelist.ElementAt(0);
            HelperMethods hl = new HelperMethods();

            foreach (twopoint tp in pipelist)
            {
                closest = hl.testdist(closest, point, tp);
            }
            pipelist.Remove(closest);
            pipelist.Insert(0, closest);

            return pipelist;
        }

        //xxxxxxxxxxxxxxxxx
        public List<Connector> allends(List<twopoint> pipelist)
        {
            List<Connector> allconector = new List<Connector>();
            foreach (twopoint tp in pipelist)
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
        public List<Connector> allconnectors(List<twopoint> pipelist)
        {
            List<Connector> allconector = new List<Connector>();
            foreach (twopoint tp in pipelist)
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
        public List<twopoint> mainlist { get; set; }
        public List<twopoint> todo { get; set; }
        public List<twopoint> done { get; set; }

        public twopointlist(List<twopoint> Mainlist)
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
