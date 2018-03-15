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
    public class objectreconorig : GH_Component
    {
        #region
        public objectreconorig()
            : base("CAD recognizer_v1", "CAD recognizerv1", "CAD recognizerv1", "Extra", "CADreconv1")
        {
        }



        public override Guid ComponentGuid
        {
            get { return new Guid("086bf257-7501-4313-bb24-a2213f8f3099"); }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            //pManager.AddBooleanParameter("on/off", "T", "on/off", GH_ParamAccess.item);
            pManager.AddCurveParameter("All curves", "All", " All Curves", GH_ParamAccess.list);
            pManager.AddCurveParameter("Search items", "Search", " Known Curve", GH_ParamAccess.list);
            pManager.AddLineParameter("direction", "Vector", " Plane vector", GH_ParamAccess.item);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Reverse", "R", "Output", GH_ParamAccess.list);
            pManager.AddCurveParameter("Curve", "C", "Found objects", GH_ParamAccess.tree);
            pManager.AddCurveParameter("Final", "F", "Final Tree", GH_ParamAccess.tree);
            pManager.AddLineParameter("Vector", "V", "Average Vector", GH_ParamAccess.list);

            //   pManager.AddPlaneParameter("plane1", "Pl", "Plane start", GH_ParamAccess.list);
            pManager.AddPointParameter("xformed1", "P", "Point start", GH_ParamAccess.list);
            pManager.AddPointParameter("xformed", "P", "Point End", GH_ParamAccess.list);
            pManager.AddPointParameter("xformed", "P", "Average Point", GH_ParamAccess.list);
        }
        #endregion

        private List<cadgeo> Representall = new List<cadgeo>();
        private List<cadgeo> Representref = new List<cadgeo>();
  private Mmap mmapknown = new Mmap();

        public List<rep> AllCandidates = new List<rep>();
        public dataobject known = new dataobject();
        private List<string> sb = new List<string>();
        private List<string> sd = new List<string>();

        public List<rep> outputs = new List<rep>();
        public List<rep> rerun = new List<rep>();
        private int toofarindex = 150;



        protected override void SolveInstance(IGH_DataAccess DA)
        {

            #region setup and validation

            List<Curve> all = new List<Curve>();
            List<Curve> known = new List<Curve>();
            Rhino.Geometry.Line vtr = Rhino.Geometry.Line.Unset;

            //Retrieve the whole list using Da.GetDataList().
            if (!DA.GetDataList(0, all)) { return; }
            if (!DA.GetDataList(1, known)) { return; }
            if (!DA.GetData(2, ref vtr)) { return; }

            //Validate inputs.
            if (known.Count < 0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Known Count must be a positive integer");
                return;
            }
            if (all.Count < 2)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "All Count must be more");
                return;
            }
            if (!vtr.IsValid)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Count must be a positive integer");
                return;
            }
            #endregion

            //DATA ORGANIZATION PROCEDURES AND CLASS CREATION
            createUNknown(all);
            createknown(known);

            //SEARCH FUNCTION
            List<rep> found = searchlooponly(Representall, AllCandidates);
            List<rep> final = secondloop(found);

            //CLEANUP
            rep knownrep = new rep();
            Representref.ForEach(x => knownrep.Add(x));
            getcenterofmultreps(final);
            getcenterofrep(knownrep);

            //TRANSLATION AND UCS CREATION
            //create best fit lines for the pointmaps
            List<GH_Line> fitlineunkown = multiplebestfits(final);
            GH_Line fitlineknown = getbestfitline(knownrep);
            knownrep.translatedline = fitlineknown;

            //
            List<ptlist> xformed = planes(fitlineunkown, vtr, knownrep, 0);
            List<Point3d> placepts1 = pointmapcompare(knownrep, vtr.PointAt(0), xformed, final);
            List<ptlist> xformed1 = planes(fitlineunkown, vtr, knownrep, 1);
            List<Point3d> placepts2 = pointmapcompare(knownrep, vtr.PointAt(1), xformed, final);


            //OUTPUTS CREATION
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes = createtree(found);
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes2 = createtree(final);
            List<Point3d> avgpt = new List<Point3d>();
            final.ForEach(x => avgpt.Add(x.centerpt));

            //GRASSHOPPER SIDE OUTPUTS
            DA.SetDataList(0, sb);
            DA.SetDataTree(1, tes);
            DA.SetDataTree(2, tes2);
            DA.SetDataList(3, fitlineunkown);

            //   DA.SetDataList(4, xformed); // Plane
            DA.SetDataList(4, placepts1);
            DA.SetDataList(5, placepts2);
            DA.SetDataList(6, avgpt);
        }





        #region Orientation Fucntions

     
        private List<Point3d> pointmapcompare(rep knownrep, Point3d goodpt, List<ptlist> unkownpts, List<rep> unknownmap)
        {
            sb.Add("'pointmapcompare' called");
            List<Point3d> final = new List<Point3d>();
            try
            {
                List<cadgeo> gg = knownrep;
                double distanceknown = ptdistsum(gg, goodpt);
                //unknown map - the list of cadgeos in question
                for (int i = 0; i < unknownmap.Count; i++)
                {
                    sb.Add("  ");
                    sb.Add("'pointdistcompare' called " + i.ToString());

                    Point3d ptout = pointdistcompare(distanceknown,
                        unkownpts.ElementAt(i), unknownmap.ElementAt(i));

                    final.Add(ptout);

                }
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "error at 'pointmapcompare' fucntion ");
            }
            return final;
        }


        private Point3d pointdistcompare(double distanceknown, ptlist candidates, List<cadgeo> all)
        {

            //   sb.Add("known distance " + distanceknown.ToString());
            int pointindex = 0;
            double nearest = 10000;

            for (int i = 0; i < candidates.Count; i++) // Point3d pt in candidates)
            {
                // foreach candidate point, get the distance to the knowncadgeo points
                double distancecandidate = ptdistsum(all, candidates.ElementAt(i));
                // sb.Add("Ready " + i.ToString() + " " + candidates.ElementAt(i).ToString() + "  " + distancecandidate.ToString());
                // sb.Add("bool = " + Math.Abs(Math.Abs(distancecandidate) - Math.Abs(distanceknown)).ToString() + " ?? " + nearest.ToString());

                bool distance = Math.Abs(Math.Abs(distancecandidate) - Math.Abs(distanceknown)) <= nearest;

                if (distance == true)
                {
                    nearest = Math.Abs(Math.Abs(distancecandidate) - Math.Abs(distanceknown));
                    pointindex = i;
                    //  sb.Add("FOUND " + distance.ToString() +  " " +nearest.ToString());
                }

            }

            Point3d pointout = candidates.ElementAt(pointindex);

            // sb.Add("chosen point " + pointindex.ToString() + " " +  pointout.ToString());
            return pointout;
        }


        //distmap function for points only
        private List<double> ptdistmap(List<cadgeo> all, Point3d known)
        {
            List<double> map = new List<double>();
            foreach (cadgeo pt in all)
            {
                map.Add(Math.Round(pt.points.ElementAt(0).DistanceTo(known), 0));
                map.Add(Math.Round(pt.points.ElementAt(1).DistanceTo(known), 0));
            }
            return map;
        }

        //Distance of a candidate point to all the points in the cadgeo
        private double ptdistsum(List<cadgeo> all, Point3d known)
        {
            List<double> map = new List<double>();
            double total = 0;
            foreach (cadgeo pt in all)
            {

                total = total + Math.Round(pt.points.ElementAt(0).DistanceTo(known), 0);
                total = total + Math.Round(pt.points.ElementAt(1).DistanceTo(known), 0);
            }
            return total;
        }


        private List<ptlist> planes(List<GH_Line> Vect_unk, Line KVector, rep knownrep, double n)
        {
            sb.Add("Planes function called");

            GH_Line KFound = knownrep.translatedline;

            List<ptlist> pts = new List<ptlist>();
            try
            {
                Plane pl0;
                Line ln;
                KFound.CastTo(out ln);
                ln.ToNurbsCurve().FrameAt(0, out pl0);
                Point3d np;
                Point3d og = new Point3d(0, 0, 0);
                Vector3d xaxis = new Vector3d(1, 0, 0);
                Vector3d yaxis = new Vector3d(0, 1, 0);
                Vector3d zaxis = new Vector3d(0, 0, 1);
                Plane z = new Plane(og, xaxis, zaxis);

                Plane plk;
                Line lnk;
                KFound.CastTo(out lnk);
                lnk.ToNurbsCurve().FrameAt(1, out plk);


                pl0.RemapToPlaneSpace(KVector.ToNurbsCurve().PointAt(n), out np);
                Transform xform = Rhino.Geometry.Transform.Mirror(z);
                Point3d np2 = new Point3d(np);

                np2.Transform(xform);

                foreach (GH_Line gg in Vect_unk)
                {
                    ptlist pl = new ptlist();
                    Plane plu;
                    Line gln;
                    gg.CastTo(out gln);
                    gln.ToNurbsCurve().FrameAt(0, out plu);

                    Point3d dd = plu.PointAt(np.X, np.Y);
                    Point3d dd2 = plu.PointAt(np2.X, np2.Y);
                    pl.Add(dd);
                    pl.Add(dd2);
                    pts.Add(pl);
                }
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "error at 'planes' fucntion ");
            }
            sb.Add("Pointlists count is " + pts.Count.ToString());

            return pts;
        }
        #endregion

        #region BODYFUNCTIONS
        //
       
        //}

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


        // Compares two dmaps and returns distance between them
        private double comparemaps(List<double> knownm, List<double> distmap)
        {
            double total = 0;
            for (int i = 0; i < known.dmap.Count; i++)
            {
                total = total + distmap.ElementAt(i) - knownm.ElementAt(i);
            }
            return total;
        }

        private List<cadgeo> loopend(List<cadgeo> list)
        {
            try
            {
                for (int j = 0; j < list.Count; j++)
                {

                    for (int i = 0; i < list.Count; i++)
                    {
                        if (i != j)
                        {
                            cadgeo t0 = list.ElementAt(i);  // known - already in candidates list
                            cadgeo t1 = list.ElementAt(j);
                            bool bl = false;
                            List<double> distmapt = createdistmap(t1.points, t0.points);
                            foreach (dmap knownm in known.dmap)
                            {
                                // bool mapisEqual = new HashSet<double>(knownm).SetEquals(distmapt); //Origional 0 tolerance function
                                bool mapisEqual = isequal(knownm, distmapt);

                                if (mapisEqual == true)
                                {
                                    //thisrep.Add(t1);
                                    bl = true;
                                    list.Remove(t1);
                                    break;
                                }
                                else { }
                            }

                            //REPORTING
                            //sb.Add(i.ToString() + "  " + j.ToString() + "  "
                            //  + " ------ " + distmapt.ElementAt(0).ToString() + " " + distmapt.ElementAt(1).ToString() + " "
                            //  + distmapt.ElementAt(2).ToString() + " " + distmapt.ElementAt(3).ToString() + " --- " + bl.ToString());
                            // if (bl == true) { break; }

                        }
                    }
                }
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at Loop only Function");
            }
            return list;

        }

        // SAFE SEARCH LOOP
        private List<rep> searchlooponly(List<cadgeo> searchlist, List<rep> knownlist)
        {
            //  List<rep> found = new List<rep>();
            sb.Add("TOTAL knownlist before LOOP------ " + knownlist.Count.ToString());
            sb.Add("TOTAL searchlist before LOOP------ " + searchlist.Count.ToString());
            int counter = 0;

            try
            {

                for (int q = 0; q < searchlist.Count; q++) //check all cadobjects to the candidates (UNKWONNS)
                {
                    bool bl = false;
                    cadgeo t1 = searchlist.ElementAt(q);     // unknown object to check

                    for (int j = 0; j < knownlist.Count; j++)
                    {
                        rep thisrep = knownlist.ElementAt(j);

                        bool toofar = t1.points.Any(x => x.DistanceTo(thisrep.ElementAt(0).points.ElementAt(0)) > toofarindex);

                        if (toofar == false)
                        {

                            for (int i = 0; i < thisrep.Count; i++)
                            {
                                cadgeo t0 = thisrep.ElementAt(i);  // known - already in candidates list

                                List<double> distmapt = createdistmap(t1.points, t0.points);
                                int numtrue = 0;

                                foreach (dmap knownm in known.dmap)
                                {

                                    bool mapisEqual = isequal(knownm, distmapt);

                                    if (mapisEqual == true)
                                    {
                                        numtrue++;
                                         thisrep.Add(t1);          //old
                                         bl = true;                //old
                                         if (bl == true) { break; }//old
                                    }
                                    counter++;
                                }

                              //  if (numtrue > (Representref.Count - 2))
                               // {
                               //     thisrep.Add(t1);
                                //    bl = true;
                               //     if (bl == true) { break; }
                               // }


                                //REPORTING
                                //   sb.Add(q.ToString() + "  " + j.ToString() + "  " + i.ToString() + "  " 
                                // + " ------ " + distmapt.ElementAt(0).ToString() + " " + distmapt.ElementAt(1).ToString() + " "
                                // + distmapt.ElementAt(2).ToString() + " " + distmapt.ElementAt(3).ToString() + " --- " + bl.ToString());

                                if (bl == true) { break; }
                            }
                            if (bl == true) { break; }
                        }
                        if (bl == true) { break; }
                    }
                }


                sb.Add("TOTAL after LOOP 1------ " + knownlist.Count.ToString());
                sb.Add("Operations Run after   1------ " + counter.ToString());
                foreach (rep cg in knownlist)
                {
                    sb.Add("COUNT:" + cg.Count.ToString());
                }
            }
            catch (Exception) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'searchLooponly' Function"); }
            return knownlist;
        }

        

        private List<rep> secondloop(List<rep> full)
        {
            //when perfectly symmetrical objects, error occurs here.

            List<rep> finals = new List<rep>();
            try
            {
                sb.Add("  ");
                sb.Add("SECOND LOOP");
                sb.Add("TOTAL before LOOP------ " + full.Count.ToString());

                foreach (rep r in full)
                { sb.Add("Element " + full.IndexOf(r).ToString() + " COUNT :" + r.Count.ToString()); }

                for (int i = 0; i < full.Count; i++)
                {
                    //if there is the perfect count
                    if (full.ElementAt(i).Count == Representref.Count)
                    {
                        finals.Add(full.ElementAt(i));
                    }
                    // if the count is too high, either there are duplicates or something is not mapped right. 
                    #region 1
                    else if (full.ElementAt(i).Count >= Representref.Count)
                    {
                        List<cadgeo> lcg = full.ElementAt(i).ToList();

                        for (int j = 0; j < lcg.Count; j++)
                        {
                            for (int q = 0; q < lcg.Count; q++)
                            {
                                if (j != q)
                                {
                                    //checks for duplication of geometry
                                    if ((lcg.ElementAt(j).points.ElementAt(0) == lcg.ElementAt(q).points.ElementAt(0)
                                        && lcg.ElementAt(j).points.ElementAt(1) == lcg.ElementAt(q).points.ElementAt(1))
                                        || (lcg.ElementAt(j).points.ElementAt(1) == lcg.ElementAt(q).points.ElementAt(0)
                                        && lcg.ElementAt(j).points.ElementAt(0) == lcg.ElementAt(q).points.ElementAt(1))
                                        )
                                    {
                                        // sb.Add("True " + i.ToString() + " " + j.ToString() + " " + q.ToString());
                                        lcg.Remove(lcg.ElementAt(j));
                                    }
                                }
                            }
                        }
                        rep nr = new rep();
                        foreach (cadgeo gg in lcg)
                        {
                            nr.Add(gg);
                        }
                        finals.Add(nr);
                    }
                    else { }
                    #endregion

                    //if (i != j)
                    //{
                    //    //checks for appropriate mapping - create map for that obj and check the knownmap
                    //    dmap distmapt = createdmap(lcg.ElementAt(i).points, lcg.ElementAt(j).points);
                    //    Mmap mm = known.dmap;
                    //    bool bl = false;

                    //    foreach (dmap knownm in mm)
                    //    {

                    //        bool mapisEqual = isequal(knownm, distmapt);

                    //        if (mapisEqual == true)
                    //        {
                    //            bl = true;
                    //            if (bl == true) 
                    //            {  break;  }
                    //        }
                    //        if (bl == true)
                    //        { break; }
                    //    }

                    //    if (bl == true)
                    //    {
                    //     //   break; //??? not sure if this is right
                    //    }
                    //    else
                    //    {
                    //      //  lcg.Remove(lcg.ElementAt(j));
                    //    }

                    //}
                }

                sb.Add("TOTAL after LOOP------ " + finals.Count.ToString());
                foreach (rep r in finals)
                {
                    sb.Add("COUNT :" + r.Count.ToString());
                }
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at secondloop Function");
            }
            return finals;
        }


        private void createUNknown(List<Curve> allcurves)
        {
            try
            {
                foreach (Curve c in allcurves)
                {

                    cadgeo cg = new cadgeo();
                    List<Point3d> asd = new List<Point3d>();
                    asd.Add(c.PointAtStart);
                    asd.Add(c.PointAtEnd);

                    cg.linelength = Math.Round(c.PointAtEnd.DistanceTo(c.PointAtStart), 0);
                    cg.cadcurve = c;
                    cg.points = asd;
                    Representall.Add(cg);
                    //  sb.Add("YES" + c.PointAtStart.ToString() + "  " + c.PointAtEnd.ToString());

                }
                sb.Add("Total Curves : " + Representall.Count.ToString());
            }
            catch (Exception) { sb.Add("exception at 'createunkown'"); }
        }

        private void createknown(List<Curve> knowncurves)
        {
            try
            {
                for (int i = 0; i < knowncurves.Count; i++) //Curve c in knowncurves)
                {
                    cadgeo cg = new cadgeo();
                    List<Point3d> asd = new List<Point3d>();
                    asd.Add(knowncurves.ElementAt(i).PointAtStart);
                    asd.Add(knowncurves.ElementAt(i).PointAtEnd);

                    cg.linelength = Math.Round(knowncurves.ElementAt(i).PointAtEnd.DistanceTo(knowncurves.ElementAt(i).PointAtStart), 0);
                    cg.cadcurve = knowncurves.ElementAt(i);
                    cg.points = asd;

                    Representref.Add(cg);
                  
                }
                sb.Add("Used Curves : " + Representref.Count.ToString());



                // creating the known objectmap - object map 
                Mmap knownmap = new Mmap();
                List<int> exams = new List<int>();
                foreach (cadgeo cc in Representref)
                {
                    exams.Add(Representref.IndexOf(cc));
                }
                var com = exams.Select(x => exams.Where(y => exams.IndexOf(y) > exams.IndexOf(x))
                   .Select(z => new List<int> { x, z }))
                   .SelectMany(x => x);

                sb.Add("MAP IS : ");
                for (int i = 0; i < com.Count(); i++)
                {
                    dmap map = createdmap(Representref.ElementAt(com.ElementAt(i).ElementAt(0)).points,
                    Representref.ElementAt(com.ElementAt(i).ElementAt(1)).points);
                    ///////////////
                    //map.angle = Representref.ElementAt(com.ElementAt(i).ElementAt(1)).cadcurve.
                    //////////////
                    knownmap.Add(map);

                    sb.Add(map.ElementAt(0).ToString() + " " + map.ElementAt(1).ToString() + " "
                             + map.ElementAt(2).ToString() + " " + map.ElementAt(3).ToString());
                }
                known.dmap = knownmap;


                cadgeo cg0 = Representref.ElementAt(0);
                List<cadgeo> allt0 = Representall.Where(x => x.linelength.Equals(cg0.linelength)).ToList();

                sb.Add("number of candidates " + allt0.Count);
                List<cadgeo> nl = loopend(allt0);

                foreach (cadgeo tz in nl)
                {
                    rep rp = new rep();
                    rp.Add(tz);


                    AllCandidates.Add(rp);

                }
                sb.Add("number of candidates " + AllCandidates.Count);


            }
            catch (Exception) { sb.Add("exception at 'createknown'"); }
        }

        #endregion

        #region outdated
        #endregion

        #region HelperFunctions

        private List<double> createdistmap(List<Point3d> p1, List<Point3d> p2)
        {

            List<double> finalmap = new List<double>();
            try
            {
                finalmap.Add(Math.Round(p1.ElementAt(0).DistanceTo(p2.ElementAt(0)), 0));
                finalmap.Add(Math.Round(p1.ElementAt(0).DistanceTo(p2.ElementAt(1)), 0));
                finalmap.Add(Math.Round(p1.ElementAt(1).DistanceTo(p2.ElementAt(0)), 0));
                finalmap.Add(Math.Round(p1.ElementAt(1).DistanceTo(p2.ElementAt(1)), 0));
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'createDistmap'");
            }
            return finalmap;
        }

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
        private Grasshopper.Kernel.Data.GH_Structure<GH_Curve> createtree(List<rep> reps)
        {
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes = new GH_Structure<GH_Curve>();
            try
            {
                if (reps.Count > 0)
                {
                    for (int i = 0; i < reps.Count; i++)
                    {
                        GH_Path path = new GH_Path(i);

                        foreach (cadgeo c in reps.ElementAt(i))
                        {
                            GH_Curve nc = new GH_Curve(c.cadcurve);
                            tes.Append(nc, path);
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

        private List<Point3d> getcenter(List<rep> list)
        {
            List<Point3d> avgpts = new List<Point3d>();
            try
            {
                foreach (rep r in list)
                {
                    List<Point3d> pts = new List<Point3d>();
                    List<double> ptsx = new List<double>();
                    List<double> ptsy = new List<double>();
                    List<double> ptsz = new List<double>();

                    foreach (cadgeo c in r)
                    {
                        pts.Add(c.points.ElementAt(0));
                        pts.Add(c.points.ElementAt(1));
                        ptsx.Add(c.points.ElementAt(0).X);
                        ptsx.Add(c.points.ElementAt(1).X);
                        ptsy.Add(c.points.ElementAt(0).Y);
                        ptsy.Add(c.points.ElementAt(1).Y);
                        ptsz.Add(c.points.ElementAt(0).Z);
                        ptsz.Add(c.points.ElementAt(1).Z);
                    }

                    Point3d npt = new Point3d(ptsx.Sum() / ptsx.Count, ptsy.Sum() / ptsy.Count,
                        ptsz.Sum() / ptsz.Count);
                    avgpts.Add(npt);
                }
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getcenter' function");
            }
            return avgpts;
        }


        private void getcenterofmultreps(List<rep> list)
        {
            foreach (rep r in list)
            {
                getcenterofrep(r);
            }

        }

        private void getcenterofrep(rep r)
        {
            try
            {
                List<Point3d> pts = new List<Point3d>();
                List<double> ptsx = new List<double>();
                List<double> ptsy = new List<double>();
                List<double> ptsz = new List<double>();

                foreach (cadgeo c in r)
                {
                    pts.Add(c.points.ElementAt(0));
                    pts.Add(c.points.ElementAt(1));
                    ptsx.Add(c.points.ElementAt(0).X);
                    ptsx.Add(c.points.ElementAt(1).X);
                    ptsy.Add(c.points.ElementAt(0).Y);
                    ptsy.Add(c.points.ElementAt(1).Y);
                    ptsz.Add(c.points.ElementAt(0).Z);
                    ptsz.Add(c.points.ElementAt(1).Z);
                }

                Point3d npt = new Point3d(ptsx.Sum() / ptsx.Count, ptsy.Sum() / ptsy.Count,
                    ptsz.Sum() / ptsz.Count);
                // avgpts.Add(npt);
                r.centerpt = npt;

            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getcenterforrep' function");
            }

        }


        private Point3d getcenterptC(List<Curve> list)//List<rep> dz)
        {
            // List<Point3d> pts = new List<Point3d>();
            Point3d npt = new Point3d();
            try
            {
                List<double> ptsx = new List<double>();
                List<double> ptsy = new List<double>();
                List<double> ptsz = new List<double>();

                foreach (Curve r in list)
                {
                    // pts.Add(r.PointAtEnd.X);
                    // pts.Add(r.points.ElementAt(1));
                    ptsx.Add(r.PointAtEnd.X);
                    ptsx.Add(r.PointAtStart.X);
                    ptsy.Add(r.PointAtEnd.Y);
                    ptsy.Add(r.PointAtStart.Y);
                    ptsz.Add(r.PointAtEnd.Z);
                    ptsz.Add(r.PointAtStart.Z);

                }
                npt = new Point3d(ptsx.Sum() / ptsx.Count, ptsy.Sum() / ptsy.Count, ptsz.Sum() / ptsx.Count);
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getcenterptC' Function");
            }
            return npt;
        }

        private List<GH_Line> multiplebestfits(List<rep> reps)
        {
            List<GH_Line> dl = new List<GH_Line>();
            try
            {
                foreach (rep r in reps)
                {
                    List<cadgeo> bb = r.ToList();
                    dl.Add(getbestfitline(r));
                }
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'multiplebestfits' Function");
            }
            return dl;
        }

        //Creates a bestfit GH line to a list of points using the median point and median longestline point
        private GH_Line getbestfitline(rep r)
        {
            sb.Add("'getbestfitline' called");
            List<cadgeo> list = r.ToList();

            GH_Line ln = new GH_Line();
            try
            {

                //method 1 - instead of least curves, when the object is not symmetrical
                // the 
                List<Curve> longest = new List<Curve>();
                List<Curve> test = new List<Curve>();
                List<Point3d> pts = new List<Point3d>();

                sb.Add("FIRST PASS");
                foreach (cadgeo cg in list)
                {
                    test.Add(cg.cadcurve);
                    sb.Add(cg.cadcurve.GetLength().ToString());
                }

                //Sort the testlist
                test.Sort((imp1, imp2) => imp1.GetLength().CompareTo(imp2.GetLength()));

                sb.Add("SECOND PASS");
                foreach (Curve c in test)
                { sb.Add(Math.Round(c.GetLength(), 0).ToString()); }

                // if (test.Count < 2)
                //  {
                //Get the longest curve
                Curve longes = test.Last();

                //Get the longest curve in the testlist
                foreach (Curve cg in test)
                {
                    if (Math.Round(cg.GetLength(), 0) == Math.Round(longes.GetLength(), 0))
                    {
                        longest.Add(cg);
                        sb.Add(cg.GetLength().ToString());
                    }
                }
                Point3d avgall = r.centerpt;
                Point3d avglong = getcenterptC(longest);

                //If the points are too close or are the same,
                //Try another method of getting the averaline
                if (avgall.DistanceTo(avglong) < 2)
                {
                    double dd = leastsquare(test);
                    Line lnf = new Line(avgall, new Vector3d(1, dd, 0), 5);
                    ln = new GH_Line(lnf);
                }
                else
                {
                    Line lnf = new Line(avglong, avgall);
                    ln = new GH_Line(lnf);
                }
                // }
                // else
                // {
                //    Line lnf = new Line(test.ElementAt(0).PointAt(0.5), test.ElementAt(1).PointAt(0.5));
                //    ln = new GH_Line(lnf);
                //}
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getbestfitline' Function");
            }
            return ln;
        }

        //Try to get the 
        private double leastsquare(List<Curve> x)
        {
            List<double> avgpts = new List<double>();
            List<double> pts = new List<double>();

            List<double> ptsx = new List<double>();
            List<double> ptsy = new List<double>();
            List<double> ptsx2 = new List<double>();
            List<double> ptsxy = new List<double>();

            foreach (Curve c in x)
            {
                ptsx.Add(c.PointAtStart.X);
                ptsx.Add(c.PointAtEnd.X);
                ptsy.Add(c.PointAtStart.Y);
                ptsy.Add(c.PointAtEnd.Y);

                ptsx2.Add(c.PointAtStart.X * c.PointAtStart.X);
                ptsx2.Add(c.PointAtEnd.X * c.PointAtEnd.X);
                ptsxy.Add(c.PointAtEnd.X * c.PointAtEnd.Y);
                ptsxy.Add(c.PointAtStart.X * c.PointAtStart.Y);
            }

            double d = (ptsxy.Sum() - ptsx.Sum() * ptsy.Sum() / ptsx.Count) /
              (ptsx2.Sum() - ptsx.Sum() * ptsx.Sum() / ptsx.Count);

            double sdx = Math.Sqrt(Math.Abs(ptsx.Sum() / ptsx.Count));
            double sdy = Math.Sqrt(Math.Abs(ptsy.Sum() / ptsy.Count));

            double r = (ptsxy.Sum() - ptsx.Sum() * ptsy.Sum() / ptsx.Count) /
              (ptsx2.Sum() - ptsx.Sum() * ptsx.Sum() / ptsx.Count);

            double m = r * sdy / sdx;
            //  double m = d;
            return m;
            // double o = Math.Atan(1 / m);
            // avgpts.Add(m);
            // A = d;

        }



        private List<double> getvector(List<rep> list)
        {
            List<double> avgpts = new List<double>();
            try
            {
                foreach (rep r in list)
                {

                    List<double> ptsx = new List<double>();
                    List<double> ptsy = new List<double>();
                    List<double> ptsx2 = new List<double>();
                    List<double> ptsxy = new List<double>();

                    foreach (cadgeo c in r)
                    {
                        ptsx.Add(c.points.ElementAt(0).X);
                        ptsx.Add(c.points.ElementAt(1).X);
                        ptsy.Add(c.points.ElementAt(0).Y);
                        ptsy.Add(c.points.ElementAt(1).Y);

                        ptsx2.Add(c.points.ElementAt(0).X * c.points.ElementAt(0).X);
                        ptsx2.Add(c.points.ElementAt(1).X * c.points.ElementAt(1).X);
                        ptsxy.Add(c.cadcurve.PointAtEnd.X * c.cadcurve.PointAtEnd.Y);
                        ptsxy.Add(c.cadcurve.PointAtStart.X * c.cadcurve.PointAtStart.Y);
                    }
                    //Fit a line to the points using least squares method
                    // m = (Sum(xy) - (sum(x))(sum(y))/n)/(sum(x^2)-(sum(x))^2/n)

                    double rr = (ptsxy.Sum() - ptsx.Sum() * ptsy.Sum() / ptsx.Count) /
                        (ptsx2.Sum() - ptsx.Sum() * ptsx.Sum() / ptsx.Count);

                    double sdx = Math.Sqrt(Math.Abs(ptsx.Sum() / ptsx.Count));
                    double sdy = Math.Sqrt(Math.Abs(ptsy.Sum() / ptsy.Count));


                    double m = rr * sdy / sdx;

                    avgpts.Add(m);
                }
            }
            catch (Exception)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getvector' Function");
            }
            return avgpts;
        }

        #endregion

    }

   







}