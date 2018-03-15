using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using Rhino.Collections;

using System.Drawing;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using RMA.OpenNURBS;

namespace Recon
{
    public class objectreconvMULT : GH_Component
    {
        #region
        public objectreconvMULT()
            : base("CAD recognizerMULT", "CAD recognizerzzMULT", "CAD recognizerzzMULT", "Extra", "CADrecon")
        {
        }



        public override Guid ComponentGuid
        {
            get { return new Guid("db8f28af-4bb2-4b95-b887-b6783a88ccca"); }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddBooleanParameter("on/off", "T", "on/off", GH_ParamAccess.item);
            pManager.AddCurveParameter("All curves", "All", " All Curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("Search items", "Search", " Known Curve", GH_ParamAccess.tree);
        //    pManager.AddLineParameter("direction", "Vector", " Plane vector", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Reverse", "R", "Output", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve", "C", "Found objects", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Final", "F", "Final Tree", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Vector", "V", "Vector", GH_ParamAccess.tree);
            pManager.AddPointParameter("xformed1", "P", "Point start", GH_ParamAccess.tree);
            pManager.AddPointParameter("Dobj", "Cobj", "CadObject", GH_ParamAccess.tree);
           // pManager.AddPointParameter("xformed", "P", "Point End", GH_ParamAccess.list);
           // pManager.AddPointParameter("xformed", "P", "Average Point", GH_ParamAccess.list);
        }
        #endregion


        private List<dataobject> AllKnownObjects = new List<dataobject>();
        private List<string> sb = new List<string>();
        private List<string> sd = new List<string>();
        public List<rep> outputs = new List<rep>();
        public List<rep> rerun = new List<rep>();
        //private int toofarindex = 150;

        GH_Document gg = new GH_Document();

        protected override void SolveInstance(IGH_DataAccess DA)
        {
           // GH_Structure<IGH_Goo> volatileData = (GH_Structure<IGH_Goo>)this.Params.Input[0].VolatileData;


            #region setup and validation
           //GH_Document.NewSo
            gg.NewSolution(true);
            //ExpireSolution(false);
            ClearData();
            
            
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tre;// = new GH_Structure<GH_Curve>();
            List<Curve> all = new List<Curve>();
            List<Curve> known = new List<Curve>();

            all.Clear();
            known.Clear();

            //Retrieve the whole list using Da.GetDataList().
            if (!DA.GetDataList(0, all)) { return; }
            if (!DA.GetDataTree(1, out tre)) { return; }
           // Line l = new Line;
            //Line.TryFitLineToPoints(
           // tre.Clear();

            //Validate inputs.
            if (known.Count < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Known Count must be a positive integer");
                return;
            }

            #endregion

            //DATA ORGANIZATION PROCEDURES AND CLASS CREATION
            

            List<cadgeo> Representall = createUNknown(all);
            createknown(tre);

            //candidate tree is a List<List<rep>> datatype
            candidatetree cantree = makecandTrees(Representall, AllKnownObjects);
            candidatetree foundobjects = searchnew(cantree, AllKnownObjects);

            //OUTPUTS CREATION
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes = createtreecandidates(cantree);
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes2 = createtreecandidates(foundobjects);
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> vectors = createtreecrvs(foundobjects);
            Grasshopper.Kernel.Data.GH_Structure<GH_Point> points = createtreepts(foundobjects);

            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> cadobj = createtreecandidates(foundobjects);

            //GRASSHOPPER SIDE OUTPUTS
            
            DA.SetDataList(0, sb);
            DA.SetDataTree(1, tes);
            DA.SetDataTree(2, tes2);
            DA.SetDataTree(3, vectors);
            DA.SetDataTree(4, points);
            DA.SetDataTree(5, cadobj);
        }


        #region BODYFUNCTIONS
        //

        //translate the origional vectors to the found gemetries using the found gemeometry planes
        private void translateoriginvectors(List<rep> foundrep, dataobject known)
            {
                Plane pknown = known.plane;
                Line lineknown = known.rep.vector;

                foreach (rep r in foundrep)
                {
                    
                    Curve newline = lineknown.ToNurbsCurve();
                    Curve nl = newline.DuplicateCurve();
                    sb.Add("Object Origin = " + r.repplane.Origin.ToString() + " ---Line start is " + nl.PointAtStart.ToString() + "   " + nl.PointAtEnd.ToString());

                   nl.Transform(Transform.PlaneToPlane(pknown, r.repplane));
                  //  nl.Transform(Transform.PlaneToPlane(r.repplane, pknown));

                    sb.Add("Translated Line = " + nl.PointAtStart.ToString() + "  " + nl.PointAtEnd.ToString());
                    Line vl = new Line(nl.PointAtStart, nl.PointAtEnd);
                    r.vector = vl;

                }
            }

        //SAFE New function for comparing distancemaps with a given tolerence
        private bool isequal(List<double> knownm, List<double> distmap)
        {
            bool mapsame = false;
            int counter = 0;
            foreach (double dk in knownm)
            {
                bool hasone = false;

                foreach (double du in distmap)
                {
                    double diff1 = du + 1;
                    double diff2 = du - 1;
                    if (Math.Abs(du - dk) < 2)
                    {
                        hasone = true;
                        break;
                    }
                }

                if (hasone == true)
                {
                    counter++;
                }
            }

            if (counter == knownm.Count)
            {
                mapsame = true;
            }

            return mapsame;
        }

        private candidatetree searchnew (candidatetree cantree, List<dataobject> AllKnownObjects)
            {
                candidatetree finaltree = new candidatetree();
                try
                {
                    foreach (List<rep> cr in cantree)
                    {
                        dataobject Dataobject = AllKnownObjects.ElementAt(cantree.IndexOf(cr));
                        //Mmap known = Dataobject.dmap;
                        List<rep> foundrep = searchlooponly(cr, Dataobject);

                        translateoriginvectors(foundrep, Dataobject);
                        finaltree.Add(foundrep);
                    }
                }
                catch (Exception) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at main search Function"); }

                return finaltree;
             }

        // known is the candidate tree object - the known one
        // knownlist is the list of objects with the correct length
        private List<rep> searchlooponly(List<rep> knownlist, dataobject known)
        {
            sb.Add("TOTAL knownlist before LOOP------ " + knownlist.Count.ToString());
            List<rep> cl = new List<rep>();
            sb.Add(" knownlist items------ " + knownlist.ElementAt(0).Count.ToString());
            List<rep> candlist = new List<rep>();
            //ist<Curve> rotated = new List<Curve>();
            try
            {
                cadgeo frst = known.rep.ElementAt(0);
                Plane knownbase = new Plane(frst.cadcurve.PointAtStart, frst.cadcurve.PointAtEnd, Plane.WorldXY.Origin);
                known.plane = knownbase;
                // double knownlength = frst.linelength;
                List<cadgeo> allg = new List<cadgeo>();
                int tol = 2;

                for (int i = 0; i < knownlist.Count; i++)
                {
                    rep nr = knownlist.ElementAt(i);
                    foreach(cadgeo gg in nr)
                    {
                        allg.Add(gg);
                        sb.Add(gg.cadcurve.GetLength().ToString());
                    }
                }

                #region
                // create a new plane from each possible object
             //   foreach (cadgeo cg in allg)
                foreach (cadgeo cg in knownlist.ElementAt(0))
                {
                    //Create transformation planes
                    // cadgeo cg = r.ElementAt(0);
                    Line ncr = new Line(cg.cadcurve.PointAtStart, cg.cadcurve.PointAtEnd);
                    if (ncr.ToNurbsCurve().GetLength() < 1)  { }
                    
                    else
                    {
                        Plane p = new Plane(cg.cadcurve.PointAtStart, cg.cadcurve.PointAtEnd, Plane.WorldXY.Origin); //Possible problem
                        Plane pmirror = p;
                        Plane protate = p;
                        Plane pmirrorotate = p;
                        pmirror.Transform(Transform.Mirror(p.Origin, p.YAxis));
                        protate.Transform(Transform.Rotation(Math.PI, ncr.PointAt(0.5)));
                        pmirrorotate.Transform(Transform.Mirror(p.Origin, p.YAxis));
                        pmirrorotate.Transform(Transform.Rotation(Math.PI, ncr.PointAt(0.5)));

                        List<Plane> planes = new List<Plane>() { p, pmirror, protate, pmirrorotate };
                        //create a transformed version of the known object onto each possible plane

                        rep prep = new rep();
                        rep prep2 = new rep();
                        rep prep3 = new rep();
                        rep prep4 = new rep();
                        prep.repplane = p;
                        prep2.repplane = pmirror;
                        prep3.repplane = protate;
                        prep4.repplane = pmirrorotate;

                        foreach (cadgeo g in known.rep)
                        {
                            //rotated curve
                            cadgeo ccg = objlocated(g, knownbase, p);
                            cadgeo ccg2 = objlocated(g, knownbase, pmirror);
                            cadgeo ccg3 = objlocated(g, knownbase, protate);
                            cadgeo ccg4 = objlocated(g, knownbase, pmirrorotate);
                            Curve ccur = ccg.cadcurve;
                            if (ccg.cadcurve.GetLength() > 1)
                            {
                                prep.Add(ccg);
                            }
                           // else { }
                            if (ccg2.cadcurve.GetLength() > 1)
                            {
                                prep2.Add(ccg2);
                            }
                            if(ccg3.cadcurve.GetLength() > 1)
                            {
                                prep3.Add(ccg3);
                            }
                            if (ccg.cadcurve.GetLength() > 1)
                            {
                                prep4.Add(ccg4);
                            }

                        }
                        // rep nr = testPossibleRep(known, knownbase, planes, cg);

                        candlist.Add(prep);
                        candlist.Add(prep2);
                        candlist.Add(prep3);
                        candlist.Add(prep4);
                        }
                }
                #endregion
                sb.Add("TOTAL knownlist ------ " + allg.Count.ToString());
                sb.Add("TOTAL prep ------ " + allg.Count.ToString());
                //foreach
                sb.Add("     " + candlist.Count.ToString());

                //main loop which takes all the transformed objects and tests against existing 
                //geometry in that object's group

                foreach (rep prep in candlist)
                {
                    //  rep post = new rep();
                    rep pos = new rep();
                    List<cadgeo> takeout = new List<cadgeo>();

                    foreach (cadgeo pg in prep)
                    {

                        for (int i = 0; i < allg.Count; i++)
                        //foreach (cadgeo cg in allg)
                        {

                            Curve crg = allg.ElementAt(i).cadcurve;
                            Point3d start = crg.PointAtStart;
                            Point3d end = crg.PointAtEnd;
                            Curve cr = pg.cadcurve;

                            //  sb.Add(cr.PointAtStart.DistanceTo(start).ToString() + "--" + cr.PointAtEnd.DistanceTo(end)
                            //      + "      " + cr.PointAtStart.DistanceTo(end) + "--" + cr.PointAtEnd.DistanceTo(start));
                            bool bl =
                                    (cr.PointAtStart.DistanceTo(start) < tol && cr.PointAtEnd.DistanceTo(end) < tol) ||
                                    (cr.PointAtStart.DistanceTo(end) < tol && cr.PointAtEnd.DistanceTo(start) < tol);

                            if (bl == true)
                            {
                                //  pos.Add(pg);
                                pos.Add(allg.ElementAt(i));
                                takeout.Add(allg.ElementAt(i));
                                //  break;
                            }
                            //   sb.Add(pos.Count.ToString() + "  " + known.rep.Count.ToString());
                        }
                    }
                    if (pos.Count >= known.rep.Count)
                    {
                        pos.repplane = prep.repplane;
                        for (int i = 0; i < pos.Count; i++)
                        {
                            for (int j = 0; j < pos.Count; j++)
                            {
                                if (i != j)
                                {
                                    if (pos.ElementAt(i).cadcurve.PointAtStart == pos.ElementAt(j).cadcurve.PointAtStart
                                        && pos.ElementAt(i).cadcurve.PointAtEnd == pos.ElementAt(j).cadcurve.PointAtEnd)
                                        { 
                                        pos.Remove(pos.ElementAt(j));
                                        break;
                                        }
                                }

                            }
                        }
                        cl.Add(pos);
                        foreach (cadgeo cg in takeout)
                        {
                            allg.Remove(cg);
                        }
                    }


                }
                
            }
            catch (Exception) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'searchLooponlynew' Function"); }

            return cl;
        }

       //private List<rep> 
        private rep testPossibleRep(dataobject known, Plane knownbase, List<Plane> planes, cadgeo cg)
            {
                int tol = 1;
                rep nr = new rep();

                //Take a plane and 
                foreach (Plane p in planes)
                {
                    rep pr = new rep();
                    //for (int i = 0; i < allg.Count; i++)
                    //{

                         foreach (cadgeo g in known.rep)
                         {
                        //rotated curve
                        cadgeo ccg = objlocated(g, knownbase, p);
                        Curve ccur = ccg.cadcurve;

                        // Test the rotated rotated cadgeo against all branches
                        // for initial test dump all elements into a single list.
                       
                            Point3d start = cg.points.ElementAt(0);
                            Point3d end = cg.points.ElementAt(1);

                            bool bl =
                             (ccur.PointAtStart.DistanceTo(start) < tol && ccur.PointAtEnd.DistanceTo(end) < tol) ||
                            (ccur.PointAtStart.DistanceTo(end) < tol && ccur.PointAtEnd.DistanceTo(start) < tol);
                            if (bl == true)
                            {
                                pr.Add(ccg);
                              //  break;
                            }
                       // }
                    }
                    if (pr.Count == known.rep.Count)
                    {
                        nr = pr;
                        break;
                    }
                    else { }
                }

               return nr;
            }

        private cadgeo objlocated(cadgeo g, Plane knownbase, Plane unknownbase)
        {
            cadgeo ng = new cadgeo();
            try
            {
                
                Curve zz = g.cadcurve;
                Curve z = zz.DuplicateCurve();
                z.Transform(Transform.PlaneToPlane(knownbase, unknownbase));
                
                ng.cadcurve = z;
               // List<Point3d> 
              //  ng.points.Add(z.PointAtStart);
               // ng.points.Add(z.PointAtEnd);
              //  ng.linelength = Math.Round(z.PointAtStart.DistanceTo(z.PointAtEnd), 0);
               
            }
            catch (Exception) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'objlocated' Function"); }
            return ng;

        }

        private List<cadgeo> createUNknown(List<Curve> allcurves)
        {
            List<cadgeo> Representall = new List<cadgeo>();
            try
            {
                foreach (Curve c in allcurves)
                {
                    if (c.GetLength(0) >= 2)
                    {
                        cadgeo cg = new cadgeo();
                        List<Point3d> asd = new List<Point3d>();
                        asd.Add(c.PointAtStart);
                        asd.Add(c.PointAtEnd);
                        cg.linelength = Math.Round(c.GetLength(0), 0);// Math.Round(c.PointAtEnd.DistanceTo(c.PointAtStart), 0);
                        cg.cadcurve = c;
                        cg.points = asd;
                        Representall.Add(cg);
                        //  sb.Add("YES" + c.PointAtStart.ToString() + "  " + c.PointAtEnd.ToString());
                    }

                }
                sb.Add("Total Curves : " + Representall.Count.ToString());
            }
            catch (Exception) { sb.Add("exception at 'createunkown'"); }

            return Representall;
        }

        private void createknown(Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tre)
        {
            try
            {
                for (int j = 0; j < tre.Branches.Count; j++)
                {
                    List<GH_Curve> brnch = tre.Branches.ElementAt(j); 
                    
                    dataobject known = new dataobject();
                    Mmap knownmap = new Mmap();
                    rep knownitem = new rep();
                    List<cadgeo> Representref = new List<cadgeo>();

                    //First Item in the tree's list will be the vector. 
                    Curve vr;
                    GH_Curve ggr = brnch.ElementAt(0);
                    ggr.CastTo(out vr);
                    Line ln = new Line(vr.PointAtStart, vr.PointAtEnd);
                    knownitem.vector = ln;

                    for (int i = 1; i < brnch.Count; i++) //Curve c in knowncurves)
                    {
                       Curve cr;
                     //   tre.get_DataItem(k, i).CastTo(out cr);
                        GH_Curve crz = brnch.ElementAt(i);
                        crz.CastTo(out cr);
                        cadgeo cg = new cadgeo();
                        List<Point3d> asd = new List<Point3d>();
                        asd.Add(cr.PointAtStart);
                        asd.Add(cr.PointAtEnd);
                        cg.linelength = Math.Round(cr.GetLength(0), 0);
                        cg.cadcurve = cr;
                        cg.points = asd;
                        Representref.Add(cg);
                        knownitem.Add(cg);
                    }

                    #region 
                    //List<int> exams = new List<int>();
                    //foreach (cadgeo cc in Representref)
                    //{
                    //    exams.Add(Representref.IndexOf(cc));
                    //}
                    //var com = exams.Select(x => exams.Where(y => exams.IndexOf(y) > exams.IndexOf(x))
                    //   .Select(z => new List<int> { x, z }))
                    //   .SelectMany(x => x);
                    //sb.Add("reference length is : " + Representref.Count.ToString());

                    //sb.Add("MAP IS : ");
                    //for (int i = 0; i < com.Count(); i++)
                    //{
                    //    dmap map = createdmap(Representref.ElementAt(com.ElementAt(i).ElementAt(0)).points,
                    //    Representref.ElementAt(com.ElementAt(i).ElementAt(1)).points);
                    //    knownmap.Add(map);

                    //    sb.Add(map.ElementAt(0).ToString() + " " + map.ElementAt(1).ToString() + " "
                    //             + map.ElementAt(2).ToString() + " " + map.ElementAt(3).ToString());
                    //}
                    //known.dmap = knownmap;
                    #endregion

                    known.rep = knownitem;
                    AllKnownObjects.Add(known);


                }
                
            }
            catch (Exception ex) 
            { 
                sb.Add("exception at 'createKNOWN'" );
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
                sb.Add("Line: " + trace.GetFrame(0).GetFileLineNumber());
            }
           
        }

        private candidatetree makecandTrees(List<cadgeo> Representall, List<dataobject> AllKnownObjects)
        {
            candidatetree cdt = new candidatetree();

            //for every item in the entire list
             //create a bin for each type of object
             foreach (dataobject dobj in AllKnownObjects)
                {  
                    //Create a branch
                 List<rep> listrep = new List<rep>();
                 List<cadgeo> listgeo = new List<cadgeo>();
                 rep rp = new rep();
                 List<double> lengthlist = new List<double>();

                 //create the list of lengths to be checked against
                 foreach (cadgeo cnng in dobj.rep)
                      {
                       lengthlist.Add(cnng.linelength);
                       }

                 //check the lengths against the predetermined list
                    foreach (cadgeo cg in Representall)
                        {
                            foreach (double dl in lengthlist)
                            {
                                // cadgeo cng = dobj.rep.ElementAt(0);
                                if (dl == cg.linelength)
                                {
                                    rp.Add(cg);
                                    break;
                                }
                            }
                         }
                        
                        
                   // }
                    //add the bin to the candidateTree
                    listrep.Add(rp);
                    cdt.Add(listrep);
                    sb.Add("Branch : " + cdt.Count.ToString() + "Count : " + listrep.Count.ToString());

            }
            sb.Add("candidate tree items " + cdt.Count.ToString());

            return cdt;
        }

        #endregion

     
        #region HelperFunctions

     
        private dmap createdmap(List<Point3d> p1, List<Point3d> p2)
        {
            dmap finalmap = new dmap();
            try
            {
                finalmap.Add(Math.Round(p1.ElementAt(0).DistanceTo(p2.ElementAt(0)), 0));
                finalmap.Add(Math.Round(p1.ElementAt(0).DistanceTo(p2.ElementAt(1)), 0));
                finalmap.Add(Math.Round(p1.ElementAt(1).DistanceTo(p2.ElementAt(0)), 0));
                finalmap.Add(Math.Round(p1.ElementAt(1).DistanceTo(p2.ElementAt(1)), 0));

            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'createdmap' ");
            }
            return finalmap;
        }

        //Tree builder in grasshopper from a list of reps
        private Grasshopper.Kernel.Data.GH_Structure<GH_Curve> createtreecandidates(candidatetree reps)
        {

            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes = new GH_Structure<GH_Curve>();
            try
            {
                if (reps.Count > 0)
                {
                    //List<List<rep>>
                    for (int i = 0; i < reps.Count; i++)
                    {
                        //List<rep>
                        var bb = reps.ElementAt(i);
                       // bb.
                        for (int j = 0; j < reps.ElementAt(i).Count; j++)
                        {
                            
                            rep cr = reps.ElementAt(i).ElementAt(j);
                            //individual <rep>
                            for (int k = 0; k < cr.Count; k++)
                            //foreach (cadgeo c in cr)
                            {
                               
                               // int k = //reps.ElementAt(i).ElementAt(j)
                                GH_Path path = new GH_Path(i, j , k);

                                GH_Curve nc = new GH_Curve(reps.ElementAt(i)
                                    .ElementAt(j).ElementAt(k).cadcurve);
                                tes.Append(nc, path);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'createtree from candidate' Function");
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
                sb.Add("Line: " + trace.GetFrame(0).GetFileLineNumber());
            }
            return tes;
        }

        //Tree builder in grasshopper from a list of reps
        private Grasshopper.Kernel.Data.GH_Structure<GH_Curve> createtreecandidates2(candidatetree reps, int n)
        {
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes = new GH_Structure<GH_Curve>();
            int count = n;
            try
            {
                if (reps.Count > 0)
                {
                    //List<List<rep>>
                    for (int i = 0; i < reps.Count; i++)
                    {
                        //List<rep>
                        var bb = reps.ElementAt(i);
                        // bb.
                        for (int j = 0; j < reps.ElementAt(i).Count; j++)
                        {

                            rep cr = reps.ElementAt(i).ElementAt(j);

                            //individual <rep>
                            //
                            Curve bbb = cr.vector.ToNurbsCurve();
                            GH_Curve lcv = new GH_Curve(bbb);
                            
                            GH_Path path = new GH_Path(count);
                            tes.Append(lcv, path);
                            count++;

                            for (int k = 0; k < cr.Count; k++)
                            //foreach (cadgeo c in cr)
                            {
                                // int k = //reps.ElementAt(i).ElementAt(j)
                                //GH_Path path = new GH_Path(count);
                              //  count++;

                                GH_Curve nc = new GH_Curve(reps.ElementAt(i)
                                    .ElementAt(j).ElementAt(k).cadcurve);
                                tes.Append(nc, path);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'createtree from candidate' Function");
                System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);
                sb.Add("Line: " + trace.GetFrame(0).GetFileLineNumber());
            }
            return tes;
        }

        private Grasshopper.Kernel.Data.GH_Structure<GH_Point> createtreepts(candidatetree reps)
        {
            Grasshopper.Kernel.Data.GH_Structure<GH_Point> tes = new GH_Structure<GH_Point>();
            try
            {
                if (reps.Count > 0)
                {
                 for (int i = 0; i < reps.Count; i++)
                      {
                        List<rep> lr = reps.ElementAt(i);
                        GH_Path path = new GH_Path(i);

                        foreach (rep r in lr)
                        {
                            Point3d p= r.vector.ToNurbsCurve().PointAtStart;
                            GH_Point lcv = new GH_Point(p);
                            tes.Append(lcv, path);
                        }

                    }
                }
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'createtree' Function");
            }
            return tes;
        }

        private Grasshopper.Kernel.Data.GH_Structure<GH_Curve> createtreecrvs(candidatetree reps)
        {
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes = new GH_Structure<GH_Curve>();
            try
            {
                if (reps.Count > 0)
                {
                    for (int i = 0; i < reps.Count; i++)
                    {
                        List<rep> lr = reps.ElementAt(i);
                        GH_Path path = new GH_Path(i);

                        foreach (rep r in lr)
                        {
                            Curve p = r.vector.ToNurbsCurve();
                            GH_Curve lcv = new GH_Curve(p);
                            tes.Append(lcv, path);
                        }

                    }
                }
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'createtree' Function");
            }
            return tes;
        }

        #endregion

    }

    public class candidatetree : List<List<rep>>
    {
        public candidatetree() { }
    }





}