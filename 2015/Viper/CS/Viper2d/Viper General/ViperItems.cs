using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using GXYZ = Autodesk.Revit.DB.XYZ;
using System.Windows.Forms;
using Autodesk.Revit.UI.Selection;
using System.Linq;




namespace Revit.SDK.Samples.UIAPI.CS
{
   


    class BranchList<Branch> : List<Branch>
    {
        public BranchList()
        {

        }

    }

    //twopoint is the container class for a line that will become a pipe
    public class twopoint
    {
        public XYZ pt1 { get; set; }
        public XYZ pt2 { get; set; }
        public int layer { get; set; }
        public int pipefunction { get; set; } // 1 = horizantal 2 = riser
        public Pipe pipe { get; set; }
        public Duct duct { get; set; }
        public MEPCurve Mepcurve { get; set; }
        public string Revittypename { get; set; }
        public string Revitcategory { get; set; }

        public Conduit conduit { get; set; }
        public List<double> dimensionlist { get; set; }
        public bool IsStart { get; set; }

        //Initalization methods

        public twopoint(XYZ ptstart, XYZ ptend, int layers, int Pipefunction)
        {
            pt1 = ptstart;
            pt2 = ptend;
            layer = layers;
            pipefunction = Pipefunction;
            dimensionlist = new List<double>();
        }
        public twopoint()
        {
      
        }
        public twopoint(XYZ ptstart, XYZ ptend)
        {
           // List<double> dl =
            dimensionlist = new List<double>();
            pt1 = ptstart;
            pt2 = ptend;
            layer = 1;
            pipefunction = 1;

        }

        public twopoint(XYZ ptstart, XYZ ptend, MEPCurve mepcrv)
        {
            // List<double> dl =
            dimensionlist = new List<double>();
            this.Mepcurve = mepcrv;
            pt1 = ptstart;
            pt2 = ptend;
            layer = 1;
            pipefunction = 1;

        }


        public string reportall()
        {
            VpReporting vpr = new VpReporting();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("------------------------------------------------------");
            sb.AppendLine("Points : ");
            sb.AppendLine(vpr.pointreport(this.pt1) + "   "   + vpr.pointreport(this.pt2));
            if (this.Mepcurve != null)
            {
                sb.AppendLine("Category Name    : " + this.Mepcurve.Category.Name.ToString());
                sb.AppendLine("Type Name        : " + (this.Mepcurve as Pipe ).PipeType.Name.ToString());
                sb.AppendLine("Diameter         : " + this.Mepcurve.Diameter.ToString());
            }
            else
            {
                sb.AppendLine("No MEPCurve Object");
            }

            return sb.ToString();
        }

        public twopoint(Pipe Newpipe)
        {
            pipe = Newpipe;
        }

        //Cycle through the newly created set of pipes and turn to tree. 
        // TESTING -----------TESTING -----------TESTING -----------TESTING -----------
        //public void MakeTreeNode(Connector knowncon, Autodesk.Revit.DB.Document doc,
        //    List<Connector> allcons, List<Branch> branches, List<twopoint> pipelist)
        //{
        //    Makepipes mp = new Makepipes();

        //    for (int i = 0; i < allcons.Count; i++)

        //    //  Connector candidatecon in allcons)
        //    {
        //        Connector candidatecon = allcons.ElementAt(i);
        //        // sd.AppendLine(candidatecon.Origin.ToString());
        //        //  Connector knowncon = mp.thiscon(pp, candidatecon);

        //        // for each of the found connectors, test if it is
        //        //within the distance of the connector
        //        //and if it is 
        //        bool iswithindistance = mp.TESTdistancepointpoint(knowncon, candidatecon);
        //        bool isonaxis = mp.TESTpipesOnSameAxis(knowncon.Owner as Pipe, candidatecon.Owner as Pipe);

        //        //Case if it is an elbow condition
        //        if (iswithindistance == true  //  && isonaxis == false
        //                && knowncon.IsConnected == false && candidatecon.IsConnected == false
        //                && candidatecon.Owner != knowncon.Owner
        //                )
        //        {
        //            //TaskDialog.Show("dsa", knowncon.Origin.ToString() + 
        //            //    "  " + candidatecon.Origin.ToString());
        //            Transaction trans = new Transaction(doc, "makeconnector");
        //            trans.Start();
        //            try
        //            {

        //                doc.Create.NewElbowFitting(knowncon, candidatecon);
        //                trans.Commit();
        //                allcons.Remove(candidatecon);
        //                allcons.Remove(knowncon);
        //                MakeTreeNode(allcons.ElementAt(0), doc,
        //                    allcons, branches, pipelist);
        //            }
        //            catch (Exception)
        //            {
        //                trans.RollBack();
        //                trans.Dispose();
        //                //  TaskDialog.Show("as", "roll");
        //                continue;
        //            }
        //        }

        //        //case if its a extension condition
        //        //else if (iswithindistance == true   // && isonaxis == true
        //        //        && knowncon.IsConnected == false && candidatecon.IsConnected == false)
        //        //        {

        //        //        }
        //        //case if the connector is somewhere like in
        //        // middle of the line - aka branch condition
        //        //else if (iswithindistance == false   //  && isonaxis == false
        //        //       && knowncon.IsConnected == false && candidatecon.IsConnected == false
        //        //        && candidatecon.Owner != knowncon.Owner)
        //        //        {

        //        //          //  TaskDialog.Show("asdas", "branch detected");
        //        //            // MAKE BRANCH
        //        //            // if (conlist.First().Origin.DistanceTo(candidatecon.Origin) >=
        //        //            //       conlist.Last().Origin.DistanceTo(candidatecon.Origin))
        //        //            //   {
        //        //            Branch branch = new Branch(candidatecon.Owner as Pipe);
        //        //            //branch.mainpipe = pp;
        //        //            branch.firstconnector = candidatecon;
        //        //            branch.branchconnection = 1;
        //        //            allcons.Remove(knowncon);
        //        //            allcons.Remove(candidatecon);
        //        //            branches.Add(branch);
        //        //            // MakeTreeNode(candidatecon.Owner as Pipe, doc, allcons, );
        //        //            // }

        //        //        }
        //        else
        //        {

        //            allcons.Remove(knowncon);

        //            //  MakeTreeNode(candidatecon.Owner as Pipe, doc, allcons, branches);


        //        }

        //    }


        //    if (allcons.Count > 2)
        //    {
        //        Connector rand = allcons.ElementAt(0);
        //        // Pipe opipe = rand.Owner as Pipe;
        //        MakeTreeNode(rand, doc, allcons, branches, pipelist);
        //    }
        //    else { }


        //}

        ////Cycle through the newly created set of pipes and turn to tree. 


        //public void MakeTreeNode(Pipe pp, Autodesk.Revit.DB.Document doc,
        //    List<Connector> allcons, List<Branch> branches, List<twopoint> pipelist)
        //{
        //    Makepipes mp = new Makepipes();
        //    double threshold = 1;
        //    LocationCurve lc = pp.Location as LocationCurve;
        //    Autodesk.Revit.DB.Line pipeline = lc.Curve as Autodesk.Revit.DB.Line;
        //    List<Connector> conlist = mp.GetPipeconnectors(pp);

        //    List<Connector> candidatecons = mp.TESTdistancepointtoline(allcons, pp, threshold);
        //    StringBuilder sd = new StringBuilder();


        //    foreach (Connector candidatecon in candidatecons)
        //    {
        //        sd.AppendLine(candidatecon.Origin.ToString());
        //        Connector knowncon = mp.thiscon(pp, candidatecon);

        //        // for each of the found connectors, test if it is
        //        //within the distance of the connector
        //        //and if it is 
        //        bool iswithindistance = mp.TESTdistancepointpoint(knowncon, candidatecon);
        //        bool isonaxis = mp.TESTpipesOnSameAxis(pp, candidatecon.Owner as Pipe);

        //        //Case if it is an elbow condition
        //        if (iswithindistance == true  //  && isonaxis == false
        //                && knowncon.IsConnected == false && candidatecon.IsConnected == false
        //                && candidatecon.Owner != knowncon.Owner
        //                )
        //        {
        //            TaskDialog.Show("dsa", knowncon.Origin.ToString() +
        //                "  " + candidatecon.Origin.ToString());
        //            Transaction trans = new Transaction(doc, "makeconnector");
        //            try
        //            {
        //                trans.Start();
        //                doc.Create.NewElbowFitting(knowncon, candidatecon);
        //                trans.Commit();
        //                allcons.Remove(candidatecon);
        //                allcons.Remove(knowncon);
        //                MakeTreeNode(candidatecon.Owner as Pipe, doc,
        //                    allcons, branches, pipelist);
        //            }
        //            catch (Exception)
        //            {
        //                trans.Dispose();
        //                //  TaskDialog.Show("as", "roll");
        //                continue;
        //            }
        //        }

        //        //case if its a extension condition
        //        //else if (iswithindistance == true   // && isonaxis == true
        //        //        && knowncon.IsConnected == false && candidatecon.IsConnected == false)
        //        //        {

        //        //        }
        //        //case if the connector is somewhere like in
        //        // middle of the line - aka branch condition
        //        else if (iswithindistance == false   //  && isonaxis == false
        //               && knowncon.IsConnected == false && candidatecon.IsConnected == false
        //                && candidatecon.Owner != knowncon.Owner)
        //        {

        //            //  TaskDialog.Show("asdas", "branch detected");
        //            // MAKE BRANCH
        //            // if (conlist.First().Origin.DistanceTo(candidatecon.Origin) >=
        //            //       conlist.Last().Origin.DistanceTo(candidatecon.Origin))
        //            //   {
        //            Branch branch = new Branch(candidatecon.Owner as Pipe);
        //            //branch.mainpipe = pp;
        //            branch.firstconnector = candidatecon;
        //            branch.branchconnection = 1;
        //            allcons.Remove(knowncon);
        //            allcons.Remove(candidatecon);
        //            branches.Add(branch);
        //            // MakeTreeNode(candidatecon.Owner as Pipe, doc, allcons, );
        //            // }

        //        }
        //        else
        //        {

        //            allcons.Remove(knowncon);

        //            //  MakeTreeNode(candidatecon.Owner as Pipe, doc, allcons, branches);


        //        }

        //    }

        //    // allcons.
        //    //  Connector rand =  allcons.ElementAt(0);
        //    // Pipe opipe = rand.Owner as Pipe;
        //    // MakeTreeNode(opipe, doc, allcons, branches, pipelist);


        //}


        //NEW METHOD - Homewood Build - cycle through two points
        //tp is the current candidate
        // DELICATE LOOP _ DO NOT FUCK WITH THIS- COPY ONLY!!!!!

        public void Twopointsonaxis (twopoint known, Autodesk.Revit.DB.Document doc
            , twopointlist tplist, ViperFormData vpdata, int count)
        {
   
            Makepipes mp = new Makepipes();
           // double threshold = vpdata.Thrsh_elbow;
            try{
                //  TaskDialog.Show(" 169 ", tplist.mainlist.Count.ToString() + " " + tplist.todo.Count.ToString());

                for (int i = 0; i < tplist.todo.Count; i++)
                {
                   // try
                    //{
                        twopoint candidate = tplist.todo.ElementAt(i);
                        if (known != candidate)
                        {
                            // for each of the found connectors, test if it is
                            //within the distance of the connector
                            //and if it is 
                            bool iswithindistance = mp.TESTdistance2point2point(known, candidate, vpdata.Thrsh_elbow);//vpdata.Thrsh_elbow);
                            bool isonaxis = mp.TESTtwopointOnSameAxis(known, candidate);

                            //  TaskDialog.Show("viperitems 180", iswithindistance.ToString() +
                            //      " " + isonaxis.ToString() + " " + d1.ToString() + " " + d2.ToString());
                            //Case if it is an elbow condition

                            if (isonaxis == true && iswithindistance == true )
                            {

                                try
                                {

                                    twopoint tpnew = mp.TwoPointRebuild(known, candidate);
                                    tplist.todo.Remove(known);
                                    tplist.todo.Remove(candidate);
                                    tplist.mainlist.Remove(known);
                                    tplist.mainlist.Remove(candidate);

                                    int n = tplist.todo.IndexOf(known);

                                    tplist.mainlist.Add(tpnew);
                                    tplist.todo.Insert(n, tpnew);

                                  //  TaskDialog.Show(" PAR ", count.ToString() + " - " + tplist.todo.IndexOf(known).ToString() + 
                                 //       " z " + tplist.mainlist.Count.ToString() + " " + tplist.todo.Count.ToString());
                                    //twopointlistt.Insert(twopointlistt.Count, tpnew);
                                    Twopointsonaxis(tplist.todo.ElementAt(n), doc, tplist, vpdata, count);
                                }
                                catch (Exception)
                                {
                                    // TaskDialog.Show("as", "roll");
                                     continue;
                                }
                            }
                        
                            else
                            {
   
                            }
                        }  
                }
                try
                {
                    int nn = tplist.todo.IndexOf(known) + 1;
                    if (nn > tplist.todo.Count)
                    { //TaskDialog.Show("as", "brk);
                    }
                    // tplist.todo.Remove(known);
                    Twopointsonaxis(tplist.todo.ElementAt(nn), doc, tplist, vpdata, count);
                }
                catch (Exception) { }
                    //TaskDialog.Show("error 3", " loop "); 
               // }
            }
            catch (Exception) { }
            //TaskDialog.Show("ASD", sd.ToString());
        }


        



    }

    public class BlockObject
    {
        public List<cadvector> cad { get; set; }
        public List<Line> countLine { get; set; }
        public List<Arc> countArc { get; set; }
        public List<Face> countFace { get; set; }
        public List<Solid> countSol { get; set; }
        public Transform transform { get; set; }
        public int cadlayer { get; set; }

        //base location
        public XYZ location1 { get; set; }

        //secondary location - location on host
        public XYZ location2 { get; set; }
        public ElementId host { get; set; }
        public ElementId element { get; set; }

        public BlockObject()
        {
            List<cadvector> cv = new List<cadvector>();
            cad = cv;
            List<Line> ll = new List<Line>();
             countLine = ll;
            List<Arc> ar = new List<Arc>();
            countArc = ar;
            List<Face> fc = new List<Face>();
            countFace = fc;
            List<Solid> sl = new List<Solid>();
            countSol = sl;
            cadlayer = 0;
            XYZ loc = new GXYZ();
            location1 = loc;

        }

        public BlockObject(XYZ loc, ElementId hostobj)
        {
            List<cadvector> cv = new List<cadvector>();
            cad = cv;
            List<Line> ll = new List<Line>();
            countLine = ll;
            List<Arc> ar = new List<Arc>();
            countArc = ar;
            List<Face> fc = new List<Face>();
            countFace = fc;
            List<Solid> sl = new List<Solid>();
            countSol = sl;
            cadlayer = 0;

            location1 = loc;
            host = hostobj;
            

        }



        public bool compareBlockObject(BlockObject known, BlockObject test)
        {
            bool bl = false;
                if (comparevectorsizes(known, test) == true)
                {
                    if (loopcompare(known, test,"countLine") == true)
                    {
                        if (loopcompare(known, test,"countArc") == true)
                        {
                             bl = true;
                        }
                        else { return false; }
                    }
                    else { return false; }
                }
                else { return false;  }

            return bl;

        }

        private bool loopcompare(BlockObject known, BlockObject test, string listname)
        {
            bool bl = false;
            if (listname == "countLine")
            {
                int count = 0;
                for (int i = 0; i < known.countLine.Count; i++)
                {
                    if (test.countLine.ElementAt(i).Length
                        - known.countLine.ElementAt(i).Length < 0.0001)
                        {
                            count++;
                        }
                    else
                     { bl = false;}// break; }
                }
                if (count == known.countLine.Count)
                {
                    bl = true;
                }

            }
            else if (listname == "countArc")
            {
                int count = 0;
                for (int i = 0; i < known.countArc.Count; i++)
                {
                    if (known.countArc.ElementAt(i).Length
                        - test.countArc.ElementAt(i).Length < 0.000001)
                        {
                            if (known.countArc.ElementAt(i).IsCyclic == test.countArc.ElementAt(i).IsCyclic
                                && known.countArc.ElementAt(i).IsBound == test.countArc.ElementAt(i).IsBound)
                                {
                                count++;
                             }
                        }
                    else
                        { bl = false; } //break; }
                }
                if (count == known.countArc.Count)
                {
                    bl = true;
                }
            }

            else { }
            return bl;
        }

        //sort
        //test length
        //test intersection
        //test misc properties
        public void sortthis(BlockObject bo)
        {
            try
            {
                bo.countLine.OrderBy(a => a.Length);
                bo.countArc.OrderBy(a => a.Length);
                bo.countFace.OrderBy(a => a.Area);
                bo.countSol.OrderBy(a => a.SurfaceArea);
            }
            catch (Exception) { }
        }

        //compare size of two vectors
        public bool comparevectorsizes(BlockObject known, BlockObject test)
        {
            bool bl = false;
            try
            {
                if (known.countArc.Count == test.countArc.Count
                    && known.countFace.Count == test.countFace.Count
                    && known.countLine.Count == test.countLine.Count
                    && known.countSol.Count == test.countSol.Count
                    && known.cadlayer == test.cadlayer)
                { bl = true; }

                else {  }
            }
            catch (Exception) { }
            return bl;
        }


        public XYZ getblockcentroid()
        {
            List<XYZ> allpt = new List<GXYZ>();

            try
            {
                foreach (Line ll in countLine)
                {
                    allpt.Add(ll.GetEndPoint(0));
                    allpt.Add(ll.GetEndPoint(1));
                }
                foreach (Arc a in countArc)
                {
                    allpt.Add(a.Center);
                }
            }
            catch (Exception) { }

            double nx = 0;
            double ny = 0;
            double nz = 0; 

            foreach (XYZ pt in allpt)
            {
                nx = nx + pt.X;
                ny = ny + pt.Y;
                nz = nz + pt.Z;
            }

            if (nx != 0)
            { nx = nx / allpt.Count; }
            if (ny != 0)
            { ny = ny / allpt.Count; }
            if (nz != 0)
            { nz = nz / allpt.Count; }

            XYZ centroid = new GXYZ (nx , ny ,nz );

           return centroid;
        }

        public void getmidline()
        {
            List<XYZ> points = new List<GXYZ>();

            try
            {
                foreach (Line ll in countLine)
                {
                    points.Add(ll.GetEndPoint(0));
                    points.Add(ll.GetEndPoint(1));
                }
                foreach (Arc arc in countArc)
                {
                    points.Add(arc.Center);
                }
            }
            catch (Exception) { }

            int numPoints = points.Count;
            double meanX = points.Average(point => point.X);
            double meanY = points.Average(point => point.Y);

            double sumXSquared = points.Sum(point => point.X * point.X);
            double sumXY = points.Sum(point => point.X * point.Y);

            double a = (sumXY / numPoints - meanX * meanY) / (sumXSquared / numPoints - meanX * meanX);
            double b = (a * meanX - meanY);
        }

        public void getmidline2(out double M, out double B )
        {
            List<XYZ> points = new List<GXYZ>();

            try
            {
                foreach (Line ll in countLine)
                {
                    points.Add(ll.GetEndPoint(0));
                    points.Add(ll.GetEndPoint(1));
                }
                foreach (Arc arc in countArc)
                {
                    points.Add(arc.Center);
                }
            }
            catch (Exception) { }


           
            //Gives best fit of data to line Y = MC + B  
            double x1, y1, xy, x2, J;
            int i;

            x1 = 0.0;
            y1 = 0.0;
            xy = 0.0;
            x2 = 0.0;

            for (i = 0; i < points.Count; i++)
            {
                x1 = x1 + points[i].X;
                y1 = y1 + points[i].Y;
                xy = xy + points[i].X * points[i].Y;
                x2 = x2 + points[i].X * points[i].X;
            }

            J = ((double)points.Count * x2) - (x1 * x1);
            if (J != 0.0)
            {
                M = (((double)points.Count * xy) - (x1 * y1)) / J;
               // M = Math.Floor(1.0E3 * M + 0.5) / 1.0E3;
                B = ((y1 * x2) - (x1 * xy)) / J;
              //  B = Math.Floor(1.0E3 * B + 0.5) / 1.0E3;
            }
            else
            {
                M = 0;
                B = 0;
            }  
        }


    }



    public class cadvector
    {
        string type { get; set; }

        private cadvector()
        {

        }

        public void selfsort()
        {
           //List<cTag> week = new List<cTag>();
           // this.Sort(delegate(type c1, type c2) { return c1.date.CompareTo(c2.age); });

        }

    }

    public class Branch
    {

        public Pipe firstpipe { get; set; }
        public Connector firstconnector { get; set; }
        public int branchconnection { get; set; }
        public Pipe mainpipe { get; set; }

        public Branch(Pipe pipe)
        {
            firstpipe = pipe;

        }
        public Branch()
        {
           // firstpipe = pipe;

        }



    }

    class vpPipeSystem
    {
        public List<twopoint> pipelist { get; set; }
    }




}
