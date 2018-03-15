using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using GXYZ = Autodesk.Revit.DB.XYZ;
using System.Windows.Forms;
using Autodesk.Revit.DB.Structure;

namespace Revit.SDK.Samples.UIAPI.CS.V_BlockMapping_OCR
{
   
    public class OCR
    {

    }

    public class objectreconorig
    {
     
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


        public void SolveInstance(List<Line> all, List<Line> known, Line vtr)
        {

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
            List<Line> fitlineunkown = multiplebestfits(final);

            //find the bestfitline of the known map
            Line fitlineknown = getbestfitline(knownrep);
            knownrep.translatedline = fitlineknown;

            //create fitlines from the found maps, and move them to their own plane
      //      List<ptlist> xformed = planes(fitlineunkown, vtr, knownrep, 0);
     //       List<XYZ> placepts1 = pointmapcompare(knownrep, vtr.GetEndPoint(0), xformed, final);
       //     List<ptlist> xformed1 = planes(fitlineunkown, vtr, knownrep, 1);
        //    List<XYZ> placepts2 = pointmapcompare(knownrep, vtr.GetEndPoint(1), xformed, final);
        }



     
        private List<XYZ> pointmapcompare(rep knownrep, XYZ goodpt, List<ptlist> unkownpts, List<rep> unknownmap)
        {
            sb.Add("'pointmapcompare' called");
            List<XYZ> final = new List<XYZ>();
            try
            {
                List<cadgeo> gg = knownrep;
                double distanceknown = ptdistsum(gg, goodpt);
                //unknown map - the list of cadgeos in question
                for (int i = 0; i < unknownmap.Count; i++)
                {
                    sb.Add("  ");
                    sb.Add("'pointdistcompare' called " + i.ToString());

                    XYZ ptout = pointdistcompare(distanceknown,
                        unkownpts.ElementAt(i), unknownmap.ElementAt(i));

                    final.Add(ptout);

                }
            }
            catch (Exception)
            {
                //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "error at 'pointmapcompare' fucntion ");
            }
            return final;
        }


        private XYZ pointdistcompare(double distanceknown, ptlist candidates, List<cadgeo> all)
        {

            int pointindex = 0;
            double nearest = 10000;

            for (int i = 0; i < candidates.Count; i++) // Point3d pt in candidates)
            {
                // foreach candidate point, get the distance to the knowncadgeo points
                double distancecandidate = ptdistsum(all, candidates.ElementAt(i));
                bool distance = Math.Abs(Math.Abs(distancecandidate) - Math.Abs(distanceknown)) <= nearest;

                if (distance == true)
                {
                    nearest = Math.Abs(Math.Abs(distancecandidate) - Math.Abs(distanceknown));
                    pointindex = i;
                    //  sb.Add("FOUND " + distance.ToString() +  " " +nearest.ToString());
                }

            }

            XYZ pointout = candidates.ElementAt(pointindex);

            // sb.Add("chosen point " + pointindex.ToString() + " " +  pointout.ToString());
            return pointout;
        }


        //distmap function for points only
        private List<double> ptdistmap(List<cadgeo> all, XYZ known)
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
        private double ptdistsum(List<cadgeo> all, XYZ known)
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

        //move the known map to a plane based on the line 
        //private List<ptlist> planes(List<Line> Vect_unk, Line KVector, rep knownrep, double n)
        //{
        //    sb.Add("Planes function called");

        //    Line KFound = knownrep.translatedline;

          //  List<ptlist> pts = new List<ptlist>();
            //try
            //{
                //Plane pl0;
                //Line ln;
                //KFound.CastTo(out ln);
                //ln.ToNurbsCurve().FrameAt(0, out pl0);
                //XYZ np;
                //XYZ og = new XYZ(0, 0, 0);
                //XYZ xaxis = new XYZ(1, 0, 0);
                //XYZ yaxis = new XYZ(0, 1, 0);
                //XYZ zaxis = new XYZ(0, 0, 1);
                //Plane z = new Plane(og, xaxis, zaxis);

                //Plane plk;
                //Line lnk;
                //KFound.CastTo(out lnk);
                //lnk.ToNurbsCurve().FrameAt(1, out plk);


                //pl0.RemapToPlaneSpace(KVector.ToNurbsCurve().PointAt(n), out np);
                //Transform xform = Rhino.Geometry.Transform.Mirror(z);
                //XYZ np2 = new XYZ(np);

                //np2.Transform(xform);

                //foreach (GH_Line gg in Vect_unk)
                //{
                //    ptlist pl = new ptlist();
                //    Plane plu;
                //    Line gln;
                //    gg.CastTo(out gln);
                //    gln.ToNurbsCurve().FrameAt(0, out plu);

                //    XYZ dd = plu.PointAt(np.X, np.Y);
                //    XYZ dd2 = plu.PointAt(np2.X, np2.Y);
                //    pl.Add(dd);
                //    pl.Add(dd2);
                //    pts.Add(pl);
            //    }
            //}
            //catch (Exception)
            //{
              //  AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "error at 'planes' fucntion ");
       //     }
          //  sb.Add("Pointlists count is " + pts.Count.ToString());

         //   return pts;
      //  }
        

    
      

        //SAFE New function for comparing distancemaps with a given tolerence
        private bool isequal(dmap knownm, dmap distmap)
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
                            dmap distmapt = createdmap(t1.points, t0.points);
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

                        }
                    }
                }
            }
            catch (Exception)
            {
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at Loop only Function");
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
                        rep thisrep = knownlist.ElementAt(j); // pick an element from known map

                        //if the the points in the item to be tested are too far, then stop
                        // this keeps looping at a minimum. THIS IS CRITICAL
                        bool toofar = t1.points.Any(x => x.DistanceTo(thisrep.ElementAt(0).points.ElementAt(0)) > toofarindex);

                        //if it is within the distance, check geometry
                        if (toofar == false)
                        {

                            for (int i = 0; i < thisrep.Count; i++)
                            {
                                cadgeo t0 = thisrep.ElementAt(i);  // known - already in candidates list

                                // distance map is matrix of distances between endpoints
                                dmap distmapt = createdmap(t1.points, t0.points);
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
            catch (Exception) { 
               // AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'searchLooponly' Function"); 
            }
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

                }

                sb.Add("TOTAL after LOOP------ " + finals.Count.ToString());
                foreach (rep r in finals)
                {
                    sb.Add("COUNT :" + r.Count.ToString());
                }
            }
            catch (Exception)
            {
                //AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at secondloop Function");
            }
            return finals;
        }


        private void createUNknown(List<Line> allcurves)
        {
            try
            {
                foreach (Line c in allcurves)
                {

                    cadgeo cg = new cadgeo();
                    List<XYZ> asd = new List<XYZ>();
                    asd.Add(c.GetEndPoint(0));
                    asd.Add(c.GetEndPoint(1));

                    cg.linelength = Math.Round(c.GetEndPoint(0).DistanceTo(c.GetEndPoint(1)), 0);
                    cg.cadcurve = c;
                    cg.points = asd;
                    Representall.Add(cg);
                    //  sb.Add("YES" + c.PointAtStart.ToString() + "  " + c.PointAtEnd.ToString());

                }
                sb.Add("Total Curves : " + Representall.Count.ToString());
            }
            catch (Exception) { sb.Add("exception at 'createunkown'"); }
        }

        private void createknown(List<Line> knowncurves)
        {
            try
            {
                for (int i = 0; i < knowncurves.Count; i++) //Curve c in knowncurves)
                {
                    cadgeo cg = new cadgeo();
                    List<XYZ> asd = new List<XYZ>();
                    asd.Add(knowncurves.ElementAt(i).GetEndPoint(0));
                    asd.Add(knowncurves.ElementAt(i).GetEndPoint(1));

                    cg.linelength = Math.Round(knowncurves.ElementAt(i).GetEndPoint(1).DistanceTo(knowncurves.ElementAt(i).GetEndPoint(0)), 0);
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

     
       

        private dmap createdmap(List<XYZ> p1, List<XYZ> p2)
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
             //   AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'createdmap' ");
            }
            return finalmap;
        }

        //Tree builder in grasshopper from a list of reps
        //private Grasshopper.Kernel.Data.GH_Structure<GH_Curve> createtree(List<rep> reps)
        //{
        //    Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes = new GH_Structure<GH_Curve>();
        //    try
        //    {
        //        if (reps.Count > 0)
        //        {
        //            for (int i = 0; i < reps.Count; i++)
        //            {
        //                GH_Path path = new GH_Path(i);

        //                foreach (cadgeo c in reps.ElementAt(i))
        //                {
        //                    GH_Curve nc = new GH_Curve(c.cadcurve);
        //                    tes.Append(nc, path);
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'createtree' Function");
        //    }
        //    return tes;
        //}

        private List<XYZ> getcenter(List<rep> list)
        {
            List<XYZ> avgpts = new List<XYZ>();
            try
            {
                foreach (rep r in list)
                {
                    List<XYZ> pts = new List<XYZ>();
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

                    XYZ npt = new XYZ(ptsx.Sum() / ptsx.Count, ptsy.Sum() / ptsy.Count,
                        ptsz.Sum() / ptsz.Count);
                    avgpts.Add(npt);
                }
            }
            catch (Exception)
            {
               // AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getcenter' function");
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
                List<XYZ> pts = new List<XYZ>();
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

                XYZ npt = new XYZ(ptsx.Sum() / ptsx.Count, ptsy.Sum() / ptsy.Count,
                    ptsz.Sum() / ptsz.Count);
                // avgpts.Add(npt);
                r.centerpt = npt;

            }
            catch (Exception)
            {
            //    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getcenterforrep' function");
            }

        }


        private XYZ getcenterptC(List<Line> list)//List<rep> dz)
        {
            // List<Point3d> pts = new List<Point3d>();
            XYZ npt = new XYZ();
            try
            {
                List<double> ptsx = new List<double>();
                List<double> ptsy = new List<double>();
                List<double> ptsz = new List<double>();

                foreach (Curve r in list)
                {
                    // pts.Add(r.PointAtEnd.X);
                    // pts.Add(r.points.ElementAt(1));
                    ptsx.Add(r.GetEndPoint(1).X);
                    ptsx.Add(r.GetEndPoint(0).X);
                    ptsy.Add(r.GetEndPoint(1).Y);
                    ptsy.Add(r.GetEndPoint(0).Y);
                    ptsz.Add(r.GetEndPoint(1).Z);
                    ptsz.Add(r.GetEndPoint(0).Z);

                }
                npt = new XYZ(ptsx.Sum() / ptsx.Count, ptsy.Sum() / ptsy.Count, ptsz.Sum() / ptsx.Count);
            }
            catch (Exception)
            {
             //   AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getcenterptC' Function");
            }
            return npt;
        }

        private List<Line> multiplebestfits(List<rep> reps)
        {
            List<Line> dl = new List<Line>();
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
              //  AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'multiplebestfits' Function");
            }
            return dl;
        }

        //Creates a bestfit GH line to a list of points using the median point and median longestline point
        private Line getbestfitline(rep r)
        {
            sb.Add("'getbestfitline' called");
            List<cadgeo> list = r.ToList();

            Line ln = Line.CreateBound(new XYZ(0,0,0),new XYZ(1,0,0));
            try
            {

                //method 1 - instead of least curves, when the object is not symmetrical
                // the
                List<Line> longest = new List<Line>();
                List<Line> test = new List<Line>();
                List<XYZ> pts = new List<XYZ>();

                sb.Add("FIRST PASS");
                foreach (cadgeo cg in list)
                {
                    test.Add(cg.cadcurve);
                    sb.Add(cg.cadcurve.Length.ToString());
                }

                //Sort the testlist
                test.Sort((imp1, imp2) => imp1.Length.CompareTo(imp2.Length));

                sb.Add("SECOND PASS");
                foreach (Line c in test)
                { sb.Add(Math.Round(c.Length, 0).ToString()); }

                // if (test.Count < 2)
                //  {
                //Get the longest curve
                Line longes = test.Last();

                //Get the longest curve in the testlist
                foreach (Line cg in test)
                {
                    if (Math.Round(cg.Length, 0) == Math.Round(longes.Length, 0))
                    {
                        longest.Add(cg);
                        sb.Add(cg.Length.ToString());
                    }
                }
                XYZ avgall = r.centerpt;
                XYZ avglong = getcenterptC(longest);

                //If the points are too close or are the same,
                //Try another method of getting the averaline
                if (avgall.DistanceTo(avglong) < 2)
                {
                    double dd = leastsquare(test);
                    Line lnf = Line.CreateBound(avgall, new XYZ(1, dd, 0));
                    ln = lnf;
                }
                else
                {
                    Line lnf = Line.CreateBound(avglong, avgall);
                    ln = lnf;
                }

            }
            catch (Exception)
            {
             //   AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getbestfitline' Function");
            }
            return ln;
        }

        //Try to get the 
        private double leastsquare(List<Line> x)
        {
            List<double> avgpts = new List<double>();
            List<double> pts = new List<double>();

            List<double> ptsx = new List<double>();
            List<double> ptsy = new List<double>();
            List<double> ptsx2 = new List<double>();
            List<double> ptsxy = new List<double>();

            foreach (Line c in x)
            {
                ptsx.Add(c.GetEndPoint(0).X);
                ptsx.Add(c.GetEndPoint(1).X);
                ptsy.Add(c.GetEndPoint(0).Y);
                ptsy.Add(c.GetEndPoint(1).Y);

                ptsx2.Add(c.GetEndPoint(0).X * c.GetEndPoint(0).X);
                ptsx2.Add(c.GetEndPoint(1).X * c.GetEndPoint(1).X);
                ptsxy.Add(c.GetEndPoint(1).X * c.GetEndPoint(1).Y);
                ptsxy.Add(c.GetEndPoint(0).X * c.GetEndPoint(0).Y);
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
                        ptsxy.Add(c.cadcurve.GetEndPoint(1).X * c.cadcurve.GetEndPoint(1).Y);
                        ptsxy.Add(c.cadcurve.GetEndPoint(0).X * c.cadcurve.GetEndPoint(0).Y);
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
              //  AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Error at 'getvector' Function");
            }
            return avgpts;
        }

    }
        
    
   public class dataobject
    {
        public Mmap dmap { get; set; }
        public rep rep { get; set; }
        public Line line { get; set; }
        public Plane plane { get; set; }
    }

    public class cadgeo
    {
        public List<XYZ> points { get; set; }
        public string linetype { get; set; }
        public double linelength { get; set; }
        public Line cadcurve { get; set; }
    }

    public class Mmap : List<dmap>
    {
        public string Name;
        public Mmap() { }
    }

    public class dmap : List<double>
    {
        public dmap() { }
        public double angle { get; set; }


    }

    public class rep : List<cadgeo>
    {
        public string Name;
        public XYZ centerpt { get; set; }
        public Line bestfitline { get; set; }
        public Line translatedline { get; set; }
        public Plane repplane { get; set; }
        public Line vector { get; set; }
        public rep() { }
    }

    public class ptlist : List<XYZ>
    {
         public ptlist() { }
    }

    
}