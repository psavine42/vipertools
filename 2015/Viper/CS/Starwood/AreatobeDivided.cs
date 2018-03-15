using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk;
using System.IO;
using System.Windows;
namespace Revit.SDK.Samples.UIAPI.CS.Starwood
{

    public class Areatobedivided
    {
        //
        public XYZ origin;
        //  public List<Line> boundaries;
        //  public List<Line> masterlines;
        public List<bLine> blines;
        public double area;
        public double areatolerance = 10;
        public Project project;
        private Random rr = new Random();
        private VpGeoUtils vpgeo;
        private VpReporting vrp;
        private StarUtils su;// = new StarUtils();
        public StringBuilder sb;
        public int iteration { get; set; }

        public Areatobedivided(Project _project, int iter)
        {
            this.su = new StarUtils();
            this.project = _project;
            this.blines = new List<bLine>();
            // this.masterlines = new List<Autodesk.Revit.DB.Line>();
            this.origin = new XYZ();
            this.sb = new StringBuilder();
            this.iteration = iter;
            this.vpgeo = new VpGeoUtils();
            this.vrp = new VpReporting();
        }

        public Areatobedivided(Project _project, List<bLine> blines, int iter)
        {
            this.su = new StarUtils();
            this.project = _project;
            this.blines = blines;
            this.origin = new XYZ();
            this.sb = new StringBuilder();
            this.iteration = iter;
            this.area = computearea();
            this.vpgeo = new VpGeoUtils();
            this.vrp = new VpReporting();

        }


        ///////////////////////
        ////MAIN SUBDIV///////
        //////////////////////
        //Random subdivision single function
        //RECURSIVE single function
        public void subdivisioniteration1(double inputdist)
        {
            
            this.project.so.WriteLine(); 
            this.project.so.WriteLine();
            this.project.so.WriteLine("new iteration" + iteration.ToString());
            


            if (this.area == 0)
                 { this.area = this.computearea();  }

                 UnitType smallunit = this.project.computesmallestunittype();
                 this.reportshape();
                 this.project.so.WriteLine((smallunit.idealarea * 2).ToString());

                 if (smallunit.idealarea * 2 < this.area)
                 {
                     iteration++;
                     if (iteration > 100)
                     {  throw new Exception("Infinite loop detected:");     }
                     rr.Next();
                     double distance = 0;
                     if (inputdist == 0) 
                        { distance = 10; }
                     else  { distance = inputdist; }

                   //  MessageBox.Show(this.iteration.ToString() + "  " + distance.ToString() + " " + this.reportshape().ToString());

                     this.project.so.WriteLine("distance " + distance.ToString());
                     this.project.so.WriteLine("current area ");
                     this.reportshape(); 
                     this.project.so.WriteLine();

                     TempAreaLines tempareas = buildsubdivisionlineRVT2(distance);
                     Areatobedivided Tarea1 = new Areatobedivided(this.project, tempareas.NewLines1, this.iteration);
                     Areatobedivided Tarea2 = new Areatobedivided(this.project, tempareas.NewLines2, this.iteration);

                 //    MessageBox.Show(Tarea1.reportshape().ToString());
                 //    MessageBox.Show(Tarea2.reportshape().ToString());

                     this.project.so.WriteLine();
                     this.project.so.WriteLine("Area1");
                     Tarea1.reportshape();
                     this.project.so.WriteLine("Area2");
                     Tarea2.reportshape();

                     //Analyze results to determing next steps
                     this.analyzetypeResults(Tarea1, Tarea2, distance);
                 }
                 else 
                 {
                     this.project.so.WriteLine();
                     this.project.so.WriteLine("** DONE **");
                    // Areatobedivided Tarea1 = new Areatobedivided(this.project, this.blines , this.iteration );
                     UnitType ut = unitwithnearestarea(project.typelist, this);
                     this.reportshape();
                     this.addunitandkeep(this, ut);
                     this.project.so.WriteLine("** DONE **");
                 }
        }



        ////Random subdivision single function
        ////RECURSIVE single function
        //public void subdivisioniteration1(double inputdist)
        //{
        //    this.project.so.WriteLine(); this.project.so.WriteLine();
        //    this.project.so.WriteLine("new iteration" + iteration.ToString());

        //    //try
        //    //{
        //    //compute smallest unittype and if does not fit stop
        //    UnitType smallunit = this.project.computesmallestunittype();
        //    //         160        //     250
        //    if (smallunit.idealarea < computearea())
        //    {
        //        // project.so.WriteLine();
        //        iteration++;
        //        if (iteration > 100)
        //        { throw new Exception("Infinite loop detected:"); }
        //        rr.Next();
        //        double distance = 0;
        //        if (inputdist == 0) { distance = 10; }
        //        else { distance = inputdist; }


        //        this.project.so.WriteLine("distance " + distance.ToString());
        //        this.project.so.WriteLine("current area ");
        //        this.reportshape(); this.project.so.WriteLine();

        //        //look at masterlines - pick  
        //        List<Line> masterlines = this.getmasterlines();
        //        //create a line for subdividing this shape

        //        List<bLine> cutboudnaries;// = new List<bLine>();
        //        List<bLine> subdivline = buildsubdivisionlineRVT(distance, out cutboudnaries);
        //        Line ln;
        //        List<bLine> newlines = Intersectsubdivisionline(this.blines, subdivline.ElementAt(0), out ln);
        //        bLine newubdivline = new bLine(ln, false);




        //        //return the newlines
        //        List<bLine> dividinglines = new List<bLine>();
        //        //List<bLine> cutboudnaries = new List<bLine>();

        //        //sort everything on each side
        //        List<bLine> side1;
        //        List<bLine> side2;
        //        sortbyside(newlines, newubdivline, out side1, out side2);

        //        Areatobedivided Tarea1 = new Areatobedivided(this.project, this.iteration);
        //        Areatobedivided Tarea2 = new Areatobedivided(this.project, this.iteration);
        //        Tarea1.blines = side1;
        //        Tarea2.blines = side2;

        //        this.project.so.WriteLine();
        //        this.project.so.WriteLine("Area1");
        //        Tarea1.reportshape();
        //        this.project.so.WriteLine("Area2");
        //        Tarea2.reportshape();

        //        this.analyzetypeResults(Tarea1, Tarea2, distance);
        //    }
        //    else
        //    {
        //        this.project.so.WriteLine();
        //        this.project.so.WriteLine("** DONE **");
        //        Areatobedivided Tarea1 = new Areatobedivided(this.project, this.iteration);
        //        Tarea1.blines = this.blines;

        //        UnitType ut = unitwithnearestarea(project.typelist, Tarea1);
        //        Tarea1.reportshape();
        //        addunitandkeep(Tarea1, ut);
        //        this.project.so.WriteLine("** DONE **");
        //    }
        //    // }
        //    //catch (Exception)
        //    // {
        //    //  //   project.so.Write(project.sb.ToString());
        //    ////     System.Windows.MessageBox.Show(project.sb.ToString());
        //    // ////    TaskDialog.Show("fubar", project.so.ToString());
        //    // }
        //}





        private void analyzetypeResults(Areatobedivided s1, Areatobedivided s2, double distance)
        {
            // this.project.so.WriteLine("no unittype found");
            UnitType ut1 = unitwithnearestarea(project.typelist, s1);
            UnitType ut2 = unitwithnearestarea(project.typelist, s2);
            
            this.project.so.WriteLine();

            // no similar area found. 
            //pick the smaller area and build run again using analysis results
            if (ut1 == null && ut2 == null)
            {
                //If the area is larger than all units, add branch out both areas
                if (s1.computearea() > this.project.computelargestunittype().idealarea
                    && s2.computearea() > this.project.computelargestunittype().idealarea)
                {
                    this.project.so.WriteLine("BOTH AREAS TOO BIG");
                    this.project.so.WriteLine(" ");
                    s1.subdivisioniteration1(0);
                    s2.subdivisioniteration1(0);
                }
                else
                {
                    //Else if one unit is smaller, increase size and try to build the closest
                    //unit in size
                    this.project.so.WriteLine("Main 127 no unittype found");
                    Areatobedivided sout = PickSmallerArea(s1, s2);

                    this.project.so.WriteLine("unit area: " + sout.area.ToString());
                    this.project.so.WriteLine((sout.computearea() * 2).ToString() + " " + this.area.ToString());

                    if (sout.area * 2 > this.area)
                    {
                        this.project.so.WriteLine("Cannot make more than one Unit here");
                        this.addunitandkeep(sout, this.project.computelargestunittype());
                    }
                    else
                    {
                        double newdistance = analyseareaResults(project.typelist, sout, distance);
                        this.project.so.WriteLine("new distance is" + newdistance.ToString());
                        this.subdivisioniteration1(newdistance);
                    }
                }
            }

            // if both work, then it is DONE
            else if (ut2 != null && ut1 != null)
            {
                this.project.so.WriteLine("all units found -- projectbuild complete");
                this.project.so.WriteLine("**********************");
                this.project.so.WriteLine("**********************");
                this.addunitandkeep(s1, ut1);
                this.addunitandkeep(s2, ut2);
               // Linesdone();
                TaskDialog.Show("SA", "done. great success!");
            }

            // if one works add it to the project, and rebuild with the other area
            else
            {
                this.project.so.WriteLine("one unittype found");
                if (ut1 != null)
                {
                    this.addunitandkeep(s1, ut1);
                    this.project.so.WriteLine("unit area  " + ut1.idealarea.ToString());
                    this.project.so.WriteLine("A1 is to be built");
                    s2.subdivisioniteration1(0);
                }
                else if (ut2 != null)
                {
                    this.addunitandkeep(s2, ut2);
                    this.project.so.WriteLine("unit area  " + ut2.idealarea.ToString());
                    this.project.so.WriteLine("A2 is to be built");
                    s1.subdivisioniteration1(0);
                }
            }
        }

        #region Reporting
        /////////////////////
        ////REPORTING////////
        /////////////////////
        private Areatobedivided PickSmallerArea(Areatobedivided s1, Areatobedivided s2)
        {
            double d1 = s1.computearea();
            double d2 =s2.computearea();
            if (d1 > d2)
            { return s2; }
            else return s1;

        }

       
        #endregion

        private void addunitandkeep(Areatobedivided s1, UnitType ut)
        {
            Unit unit = new Unit(ut, s1.computeorigin(), s1.blines);
            this.project.addunit2(unit);
        }


        //get the unittype with the area nearest to what is needed
        public UnitType unitnearestarea(List<UnitType> uts, Areatobedivided testarea)
        {

            double dl = testarea.computearea();
            this.project.so.WriteLine("unittype testing : area to test - " + dl.ToString());
            double dub = 10000;
            int selectedunit = 0;

            for (int i = 0; i < uts.Count; i++)
            {
                //difference of unit area and calculated area
                double nz = uts.ElementAt(i).idealarea - dl;
                if (dub > nz)
                {
                    dub = nz;
                    selectedunit = i;
                    this.project.so.WriteLine("test " + i.ToString() + "  " + uts.ElementAt(i).idealarea.ToString() + dub.ToString());
                    this.project.so.WriteLine(selectedunit.ToString());
                }
            }
            UnitType ut = uts.ElementAt(selectedunit);
            return ut;
        }

        //get the unittype with the area nearest to what is needed
        public UnitType unitwithnearestarea(List<UnitType> uts, Areatobedivided testarea)
        {
            double dl = testarea.computearea();
            UnitType ut = unitnearestarea(uts, testarea);

            if (Math.Abs(ut.idealarea - dl) < this.areatolerance)
            {
                this.project.so.WriteLine("unit found " + ut.idealarea.ToString());
                return ut;
            }

            else
            {
                this.project.so.WriteLine("no unit found ");
                return null;
            }
        }

        ////
        private double analyseareaResults(List<UnitType> uts, Areatobedivided s1, double moveddistanceX)
        {
            UnitType thisut = unitnearestarea(uts, s1);
            double ideala = thisut.idealarea;
            double thisa = s1.computearea();
         //   this.project.so.WriteLine("ideal unit area " + ideala);
          //  this.project.so.WriteLine("this unit area  " + thisa);

            if (Math.Abs(thisa - ideala) < this.areatolerance)
            {
                return 0;
            }
            else
            {
                //lengthy is constant to be found     //ideal 
                double lenghty = thisa / moveddistanceX;
                return ideala / lenghty;
            }
        }

        // operations needed to compute the area from an unordered list of boundary curves.
        public double computearea()
        {
            List<XYZ> sorted1 = su.SortCurvesContiguous(this.blinestoRVT(this.blines), true, sb);
            List<UV> flat1 = Flatten(sorted1);
            StringBuilder sbt = new StringBuilder();
            //foreach (XYZ pt in sorted1)
            //{
            //    sbt.Append("    " + pt.ToString());
            //}
            //MessageBox.Show(sbt.ToString());

            double a1 = GetSignedPolygonArea(flat1);
            return a1;
        }

        private double getadist(Random rr)
        {
            return rr.Next() * 10;
        }

        private List<Line> getmasterlines()
        {
            List<Line> linesout = new List<Line>();

            foreach (bLine bl in this.blines)
            {
                if (bl.masterline == true)
                {
                    linesout.Add(bl.line);
                }
            }
            //  TaskDialog.Show("asd", linesout.Count.ToString());   
            if (linesout.Count > 0)
            {
                return linesout;
            }
            else return null;
        }

        private Line Getlongestline(List<Line> linesin)
        {
            Line l = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(1, 0, 0));
            double length = 0;
            foreach (Line bl in linesin)
            {
                if (bl.Length > length)
                {
                    length = bl.Length;
                    l = bl;
                }
            }
            return l;
        }

        private List<Line> blinestoRVT(List<bLine> bolines)
        {
            List<Line> ll = new List<Line>();
            foreach (bLine bl in bolines)
            {
                ll.Add(bl.line);
            }
            return ll;
        }

        private XYZ computeorigin()
        {
            var query = (from cust in blines
                         where cust.masterline = true
                         select cust).FirstOrDefault();

            return query.line.GetEndPoint(0);
        }

        //intersects new line with all boundaries in the area returns new boundaries

        private List<bLine> Intersectsubdivisionline(List<bLine> thisblines, bLine l, out Line nl)
        {
            //try
            //{
            this.project.sb.AppendLine("intersection entered");
                //    this.project.so.WriteLine(
                List<bLine> outs = new List<bLine>();
                List<XYZ> newpts = new List<XYZ>();

                Line extend = Line.CreateBound(l.line.GetEndPoint(0), // - l.line.Direction * 1000,
                    l.line.GetEndPoint(1) + l.line.Direction * 1000);
                this.project.sb.AppendLine(l.line.GetEndPoint(0).ToString() + " "   + l.line.GetEndPoint(1).ToString());
                this.project.sb.AppendLine(extend.GetEndPoint(0).ToString() + " " + extend.GetEndPoint(1).ToString());
                this.project.sb.AppendLine();
                foreach (bLine bl in thisblines)
                {
                    this.project.sb.AppendLine("interesction test " + bl.line.GetEndPoint(0).ToString() +
                        bl.line.GetEndPoint(1).ToString());
                    IntersectionResultArray ar = new IntersectionResultArray();
                    extend.Intersect(bl.line, out ar);
                    if (ar == null)
                    {
                        outs.Add(bl);
                    }
                    //if intersection exists, rebuild the lines as split
                    else if (ar.Size == 1)
                    {
                        XYZ intersect = ar.get_Item(0).XYZPoint;
                        this.project.sb.AppendLine(" intersects -----");
                        this.project.sb.AppendLine(intersect.ToString());

                        if (bl.line.GetEndPoint(0).DistanceTo(intersect) > .001)
                        {                     
                            Line l1 = Line.CreateBound(bl.line.GetEndPoint(0), intersect);
                            outs.Add(new bLine(l1, bl.masterline));
                        }
                        if (bl.line.GetEndPoint(1).DistanceTo(intersect) > .001)
                        {
                            Line l2 = Line.CreateBound(bl.line.GetEndPoint(1), intersect);
                            outs.Add(new bLine(l2, bl.masterline));
                        }
    
                        newpts.Add(intersect);
                    }

                    else { }

                }
                if (newpts.Count == 2)
                {
                    this.project.sb.AppendLine("new line created");
                    this.project.sb.AppendLine(newpts.ElementAt(0).ToString() + " " + newpts.ElementAt(1).ToString());
                    nl = Line.CreateBound(newpts.ElementAt(0), newpts.ElementAt(1));

                }
                else
                {  nl = null;  }

                return outs;
        }

        public Line Get_LinenearesttolastUnit()
        {
            if (this.getmasterlines() != null)
            {
                if (this.project.units.Count > 0)
                {
                    Line lineout = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(0, 0, 1));
                    Unit unit = this.project.units.Last();
                    XYZ location = unit.unitlocation1;
                    List<Line> masterlines = this.getmasterlines();
                     double distance = 10000;

                    foreach (Line line in masterlines)
                    {
                      
                      if (distance > line.Distance(location))
                      {
                          distance = line.Distance(location);
                          lineout = line;
                      }

                    }

                    return lineout;
                }
                else { return null; }
            }
            else return null;
        }


        public XYZ Get_PointnearesttolastUnit()
        {
            
                if (this.project.units.Count > 0)
                {
                    Line lineout = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(0, 0, 1));
                    Unit unit = this.project.units.Last();
                    return unit.unitlocation1;
                }
                else { return new XYZ(0, 0, 0); }
            
        }
      


        private TempAreaLines Intersectsubdivisionline2(TempAreaLines lines, bLine extend)
        {
            //Report_String_txt("intersection entered");
            //Report_String_txt(" ");
           
            List<XYZ> newpts = new List<XYZ>();
            List<bLine> temps = new List<bLine>();
            List<bLine> todelete = new List<bLine>();

            for (int i = 0; i < lines.outerlines.Count; i++)
            {
                bLine bl = lines.outerlines.ElementAt(i);
                this.project.sb.AppendLine("interesction test " + vrp.linereport(bl.line) + bl.masterline);

                IntersectionResultArray ints;
                bl.line.Intersect(extend.line, out ints);

               // XYZ intersect = vpgeo.IntersectLines(bl.line, extend.line);
               // if (intersect == null)
                if (ints == null)
                {
                  //  MessageBox.Show(" NO "  + vrp.linereport(bl.line) + vrp.linereport(extend.line)); 
                }
                else
                {
                    XYZ intersect = ints.get_Item(0).XYZPoint;
                 //   MessageBox.Show(" YES " + vrp.linereport(bl.line) + vrp.linereport(extend.line) + intersect.ToString());
                    //this.project.sb.AppendLine(" intersects -----");
                    //this.project.sb.AppendLine(intersect.ToString());
                    if (bl.masterline == true)
                    {
                        Line l1 = Line.CreateBound(bl.line.GetEndPoint(0), intersect);
                        Line l2 = Line.CreateBound(bl.line.GetEndPoint(1), intersect);
                        lines.NewLines1.Add(new bLine(l1, bl.masterline));
                        temps.Add(new bLine(l2, bl.masterline));
                    }
                    else
                    {
                        Line l1 = Line.CreateBound(bl.line.GetEndPoint(0), intersect);
                        Line l2 = Line.CreateBound(bl.line.GetEndPoint(1), intersect);
                        temps.Add(new bLine(l1, bl.masterline));
                        temps.Add(new bLine(l2, bl.masterline));
                    }

                    todelete.Add(bl);
                    newpts.Add(intersect);
                }
            }
            if (newpts.Count == 2)
            {
                
                //add the rebuilt new line (fromer dividing line)
                Report_String_txt("new line created");
                Report_String_txt(newpts.ElementAt(0).ToString() + " " + newpts.ElementAt(1).ToString());
                Line newline = Line.CreateBound(newpts.ElementAt(0), newpts.ElementAt(1));

             //   MessageBox.Show(vrp.linereport(newline));
                lines.intersectinglines.Add(new bLine(newline, false));
                lines.NewLines1.Insert(0, new bLine(newline, false));
                lines.NewLines2.Add(new bLine(newline, false));
                foreach (bLine n in todelete)
                {
                    lines.outerlines.Remove(n);
                }
            }
            else
            { //nl = null; //INTERSECTION DOES NOT HAPPEN  
            }
            foreach (bLine bl in temps)
            {lines.outerlines.Add(bl);  }

            return lines;
        }

           // create and move a line to divide a shape with
        public TempAreaLines buildsubdivisionlineRVT2(double dist)
        {
            TempAreaLines templines = new TempAreaLines(this.project);
            List<bLine> cutlinesout = new List<bLine>(this.blines);
            templines.outerlines = cutlinesout;
            

            this.project.so.WriteLine("buildsubdivision2 line entered");
            Line masterl = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(1, 0, 0));
            if (this.getmasterlines() != null)
            {
                //StringBuilder tempstr = new StringBuilder();
                //List<Line> masterlines = this.getmasterlines();
                //foreach (Line ll in masterlines)
                //{
                //    tempstr.AppendLine(vrp.linereport(ll) + "  " + ll.Length.ToString());
                //}
             //   Line sline = Getlongestline(masterlines);
                Line sline = this.Get_LinenearesttolastUnit();
                XYZ refpoint = this.Get_PointnearesttolastUnit();
                if(sline == null)
                {
                    List<Line> masterlines = this.getmasterlines();
                    sline = Getlongestline(masterlines);
                } 

            //    Line sline = su.Linenearestopoint(masterlines, new XYZ(0, 0, 0));
               //MessageBox.Show("Masterline " + vrp.linereport(sline) + "  " + sline.Length.ToString());
              // 
                

                if (sline.GetEndPoint(0).DistanceTo(refpoint) < sline.GetEndPoint(1).DistanceTo(refpoint))
                {  
                    masterl = Line.CreateBound(sline.GetEndPoint(0), sline.GetEndPoint(1));  
                }
                else 
                { 
                    masterl = Line.CreateBound(sline.GetEndPoint(1), sline.GetEndPoint(0));
                }

                MessageBox.Show("Masterline " + vrp.linereport(masterl) + "  " + masterl.Length.ToString() + "  " + dist.ToString());

                Line l = masterl.Clone() as Line;
                XYZ dir = l.Direction;
                Transform tr = Transform.CreateRotationAtPoint(new XYZ(0, 0, 1), Math.PI / 2, masterl.GetEndPoint(0));
                Line lz = l.CreateTransformed(tr) as Line;
                this.project.so.WriteLine(lz.GetEndPoint(0).ToString() + "  " + lz.GetEndPoint(1).ToString());

                //if the 
                if (dist > (masterl.Length ) || dist == (masterl.Length))
                {
                    // **************** Make funnyline

                    Transform tr2 = Transform.CreateTranslation(dir * (masterl.Length));
                    Line lb = lz.CreateTransformed(tr2) as Line;
                    this.project.so.WriteLine(lb.GetEndPoint(0).ToString() + "  " + lb.GetEndPoint(1).ToString());


                    MessageBox.Show("FNY Rport 1 " + templines.report_temp().ToString() + " - line - "
                        + vrp.linereport(masterl) + vrp.linereport(lb) );


                    templines = makefunnyArea(templines, masterl, lb);

                    MessageBox.Show("FNY Rport 2 " + templines.report_temp().ToString());
                    templines.sortcurves();
                    MessageBox.Show("FNY Rport 3 " + templines.report_temp().ToString());

                    //bLine lineout = new bLine(lb, false);
                    //templines = Intersectsubdivisionline2(templines, lineout);
                    //templines.report_temp();
                    //MessageBox.Show(templines.report_temp().ToString());
                    return templines;
                }
                else
                {
                    //returns line - not specialcase
                    Transform tr2 = Transform.CreateTranslation(dir * dist);
                    Line lb = lz.CreateTransformed(tr2) as Line;
                    this.project.so.WriteLine(lb.GetEndPoint(0).ToString() + "  " + lb.GetEndPoint(1).ToString());
                    bLine extend = new bLine(Line.CreateBound(lb.GetEndPoint(0) - lb.Direction * 1000, 
                        lb.GetEndPoint(1) + lb.Direction * 1000), false);

                  MessageBox.Show("Rport 1 " + templines.report_temp().ToString() + " - line - " + vrp.linereport(extend.line));

                     templines = Intersectsubdivisionline2(templines, extend);

                //    MessageBox.Show("Rport 2 " + templines.report_temp().ToString());
                    templines.sortcurves();
                  // MessageBox.Show("Rport 3 " + templines.report_temp().ToString());
                    return templines;
                }
            }
            else 
            {  return null; }
        }

        private TempAreaLines makefunnyArea(TempAreaLines tlist, Line masterl, Line bl)
        {
            Report_text("makefunnyArea");


            //
            Line l = masterl.Clone() as Line;
            bLine newubdivline = new bLine(bl, false);
            Line ln;
            List<bLine> newlines = Intersectsubdivisionline(tlist.outerlines, newubdivline, out ln);

            XYZ midpoint = ln.Evaluate(0.5, true);

        //    Line newmidline = Line.CreateBound(ln.GetEndPoint(0), midpoint);

            Line intline = Line.CreateBound(midpoint, ln.GetEndPoint(0));
            bLine newubdivline3 = new bLine(intline, false);
            Line ln3;
            List<bLine> newlines3 = Intersectsubdivisionline(this.blines, newubdivline3, out ln3);
            
            Line newcrossline = Line.CreateBound(midpoint, masterl.Direction * 100);
            MessageBox.Show(vrp.linereport(newcrossline));

            bLine newubdivline4 = new bLine(newcrossline, false);
            Line ln4;         
            List<bLine> newlines4 = Intersectsubdivisionline(newlines3 , newubdivline4, out ln4);

            MessageBox.Show(vrp.listreport_Bline(newlines4));

            bLine newintline1 = new bLine(ln3, false);
            bLine newintline2 = new bLine(ln4, false);

            Report_line(ln3);
            Report_line(ln4);

            MessageBox.Show("IN FNY Rport 2 " + vrp.linereport(ln3) + vrp.linereport(ln4));

            //build return list (split area)
            tlist.outerlines = newlines4;
            tlist.intersectinglines.Add(newintline1);
            tlist.intersectinglines.Add(newintline2);
            tlist.NewLines1.Add(newintline1);
            tlist.NewLines1.Add(newintline2);
            tlist.NewLines2.Add(newintline1);
            tlist.NewLines2.Add(newintline2);

            MessageBox.Show(tlist.report_temp());
          //  foreach (bLine bll in newlines2)
           // {
                

           // }
            //if the angle is 90 it will not be a 45 degree line



            //else get the angle between the two lines and that will be the vector
            return tlist;
        }

        // create and move a line to divide a shape with
        public bLine buildsubdivisionlineRVT(double dist)
        {
            List<bLine> cutlinesout = new List<bLine>();

            this.project.so.WriteLine("buildsubdivision line entered");
            Line masterl = Line.CreateBound(new XYZ(0, 0, 0), new XYZ(1, 0, 0));
            if (this.getmasterlines() != null)
            {
                List<Line> masterlines = this.getmasterlines();
                Line sline = Getlongestline(masterlines);
              //  Line sline = su.Linenearestopoint(masterlines, new XYZ(0, 0, 0));

                if (sline.GetEndPoint(0).DistanceTo(new XYZ(0, 0, 0)) <  sline.GetEndPoint(1).DistanceTo(new XYZ(0, 0, 0)))
                {  masterl = Line.CreateBound(sline.GetEndPoint(0), sline.GetEndPoint(1));  }
                else { masterl = Line.CreateBound(sline.GetEndPoint(1), sline.GetEndPoint(0)); }

                Line l = masterl.Clone() as Line;
                XYZ dir = l.Direction;
                Transform tr = Transform.CreateRotationAtPoint(new XYZ(0, 0, 1), Math.PI / 2, masterl.GetEndPoint(0));
                Line lz = l.CreateTransformed(tr) as Line;
                this.project.so.WriteLine(lz.GetEndPoint(0).ToString() + "  " + lz.GetEndPoint(1).ToString());

                //if the 
                if (dist > (masterl.Length - .1) || dist == (masterl.Length - .1))
                {
                    // **************** Make funnyline
                  //  Transform tr2 = Transform.CreateTranslation(dir * (masterl.Length - .1) );
                  //  Line lb = lz.CreateTransformed(tr2) as Line;
                  //  this.project.so.WriteLine(lb.GetEndPoint(0).ToString() + "  " + lb.GetEndPoint(1).ToString());

                  //  //
                  //  List<Line> lstn = makefunnyArea(masterl, lb);
                  //  bLine lineout= new bLine(lb, false);
                  //  Line ln;
                  //  List<bLine> newlines = Intersectsubdivisionline(lineout, out ln);
                  //  bLine newubdivline = new bLine(ln, false);


                  ////  bLine lineout = new bLine(lb, false);
                    Transform tr2 = Transform.CreateTranslation(dir * (masterl.Length - .1));
                    Line lb = lz.CreateTransformed(tr2) as Line;
                    this.project.so.WriteLine(lb.GetEndPoint(0).ToString() + "  " + lb.GetEndPoint(1).ToString());
                    bLine lineout = new bLine(lb, false);
                   return lineout;
                }
                else
                {
                    //returns line - not specialcase
                    Transform tr2 = Transform.CreateTranslation(dir * dist);
                    Line lb = lz.CreateTransformed(tr2) as Line;
                    this.project.so.WriteLine(lb.GetEndPoint(0).ToString() + "  " + lb.GetEndPoint(1).ToString());
                    bLine lineout = new bLine(lb, false);

                    return lineout;
                }
            }
            else 
            {  return null; }
        }



        /// <summary>
        /// Sorts lines by whether they are one or other side of the dividing line
        /// </summary>
        /// <param name="newlines"> set of lines to be tested</param>
        /// <param name="dividingline">dividing line</param>
        /// <param name="s1">side 1 of dividing line</param>
        /// <param name="s2">side 1 of dividing line</param>
        private void sortbyside(List<bLine> newlines, bLine dividingline, out List<bLine> s1, out List<bLine> s2)
        {
            this.project.so.WriteLine();
           this.project.so.WriteLine("sortbyside entered with :" + dividingline.line.GetEndPoint(0) + " "  + dividingline.line.GetEndPoint(1));
                List<bLine> side1 = new List<bLine>();
                List<bLine> side2 = new List<bLine>();
                side1.Add(new bLine(dividingline.line, false));
                side2.Add(new bLine(dividingline.line, false));

                foreach (bLine test in newlines)
                {
                    this.project.so.Write("test : " + test.line.GetEndPoint(0) + " "  + test.line.GetEndPoint(1));

                    double dl1 = this.su.computeside(dividingline.line, test.line.GetEndPoint(0));
                    this.project.so.Write(" " +dl1.ToString());
                    double dl2 = this.su.computeside(dividingline.line, test.line.GetEndPoint(1));
                    this.project.so.Write(" " + dl2.ToString());
                    if (dl1 > 0 || dl2 > 0)
                    {
                        side1.Add(test);
                        this.project.so.WriteLine("  -> side1 ");
                    }

                    else if (dl1 < 0 || dl2 < 0)
                    {
                        side2.Add(test);
                        this.project.so.WriteLine("  -> side2 ");
                    }
                }
                s1 = side1;
                s2 = side2;
        }

        private double GetSignedPolygonArea(List<UV> p)
        {
            int n = p.Count;
            double sum = p[0].U * (p[1].V - p[n - 1].V);
            for (int i = 1; i < n - 1; ++i)
            {
                sum += p[i].U * (p[i + 1].V - p[i - 1].V);
            }
            sum += p[n - 1].U * (p[0].V - p[n - 2].V);
            if (0.5 * sum > 0)
            {
                return 0.5 * sum;
            }
            else 
            {
                return -0.5 * sum;
            }
        }


        private UV Flatten(XYZ point)
        {
            return new UV(point.X, point.Y);
        }

        private List<UV> Flatten(List<XYZ> polygon)
        {
            //   double z = polygon[0].Z;

            List<UV> a = new List<UV>(polygon.Count);
            foreach (XYZ p in polygon)
            {
                //  Debug.Assert(Util.IsEqual(p.Z, z),
                //     "expected horizontal polygon");
                a.Add(Flatten(p));
            }
            return a;
        }


        //Reporting Utils

        private string reportshape()
        {
            VpReporting vpr = new VpReporting();
            StringBuilder sd = new StringBuilder();
            sd.AppendLine("Report shape");
            this.project.so.WriteLine("Report shape");
            foreach (bLine l in this.blines)
            {
                this.project.so.WriteLine("pt " + vpr.linereport(l.line) + " " + l.masterline.ToString());
                sd.AppendLine("pt " + vpr.linereport(l.line) + " " + l.masterline.ToString());
            }
            this.project.so.WriteLine("Report Area");
           this.project.so.WriteLine(this.area);
            sd.AppendLine("Report Area");
            sd.AppendLine(this.area.ToString());
            this.project.so.WriteLine();
            sd.AppendLine();
            return sd.ToString();
        }

        private void reportshapex(List<XYZ> side2)
        {
            foreach (XYZ l in side2)
            {
                project.so.WriteLine(l.ToString());
            }
        }

        private void Report_line(Line ln)
        {
            VpReporting vpr = new VpReporting();
            this.project.so.WriteLine(vpr.linereport(ln));
        }


       /// <summary>
       /// Reports a string to the project log
       /// </summary>
       /// <param name="ln"></param>
        public void Report_text(string ln)
        {
            VpReporting vpr = new VpReporting();
            this.project.so.WriteLine(ln);
            
        }

        private void Stringbuilder_text(string ln)
        {
            VpReporting vpr = new VpReporting();
            this.project.sb.AppendLine(ln);

        }

        private void Report_String_txt(string ln)
        {
            Stringbuilder_text(ln);
            Report_text(ln);
        }

      //  private 
    }

   

    # region Rhinocommon
    /////public void SplitareaAnd(Rhino.Geometry.Curve area, Rhino.Geometry.Curve crv)
    //{

    //    const double intol = 0.001;
    //    const double otol = 0.0;

    //    var events = Rhino.Geometry.Intersect.Intersection.CurveCurve(crv, area, intol, otol);
    //    List<double> dlist = new List<double>();

    //    for (int i = 0; i < events.Count; i++)
    //    {
    //        var ccx_event = events[i];
    //        dlist.Add(ccx_event.ParameterB);
    //    }

    //    Rhino.Geometry.Curve[] splits = area.Split(dlist);
    //    Rhino.Geometry.Curve smallest = Getcrvsmallestarea(splits);

    //   // Rhino.Geometry.Curve ccr = smallest.MakeClosed(otol);
    //}


    //
    //public Rhino.Geometry.Curve Getcrvsmallestarea (Rhino.Geometry.Curve[] areas)
    //{
    //    double dl = 100000;
    //    int n = 0;
    //    for (int i = 0; i < areas.Length; i++)
    //    {
    //        if (areas[i].GetLength() < dl)
    //        {
    //            n = i;
    //        }
    //    }
    //    return areas[n];
    //}

    //public Rhino.Geometry.Curve Getcrvlargestarea(Rhino.Geometry.Curve[] areas)
    //{
    //    double dl = 0;
    //    int n = 0;
    //    for (int i = 0; i < areas.Length; i++)
    //    {
    //        if (areas[i].GetLength() > dl)
    //        {
    //            n = i;
    //        }
    //    }
    //    return areas[n];
    //}



    //public Rhino.Geometry.Curve[] CreateAreabyOffset(double offsetdist, int dir)
    //{
    //    List<Rhino.Geometry.Curve> clist = new List<Rhino.Geometry.Curve>();
    //    foreach (Autodesk.Revit.DB.Line l in this.masterlines)
    //    {
    //        Rhino.Geometry.Line cr = su.ConverttoRC(l);
    //    }
    //    Rhino.Geometry.Curve[] plc = Rhino.Geometry.Curve.JoinCurves(clist) ;

    //    if (plc.Length == 1)
    //    {

    //        Rhino.Geometry.Plane pl;
    //        Rhino.Geometry.Curve orig = plc[0].DuplicateCurve();
    //        plc[0].FrameAt(0, out pl);
    //        plc[0].Offset(pl, offsetdist * dir, 0, CurveOffsetCornerStyle.None);

    //        Rhino.Geometry.Brep[] breps = Rhino.Geometry.Brep.CreateFromLoft(
    //        new List<Rhino.Geometry.Curve> { plc[0], orig }, plc[0].PointAtStart, orig.PointAtStart, LoftType.Straight, false);


    //        Rhino.Geometry.Curve[] curves = breps[0].DuplicateEdgeCurves(true);
    //        return curves;
    //    }
    //    else {return null;}


    //  }

    ///public void BuildAreaInRhino(List<Rhino.Geometry.Curve> rcurves, Document doc)
    //{
    //    List<Autodesk.Revit.DB.Curve> Revcrvs = new List<Autodesk.Revit.DB.Curve>();

    //    CurveArray floorCurveArray = new CurveArray();
    //    foreach(Rhino.Geometry.Curve crv in rcurves)
    //    {
    //       Autodesk.Revit.DB.Line l =  Autodesk.Revit.DB.Line.CreateBound(new XYZ (crv.PointAtStart.X, crv.PointAtStart.Y, crv.PointAtStart.Z),
    //            new XYZ (crv.PointAtStart.X, crv.PointAtStart.Y, crv.PointAtStart.Z));

    //        floorCurveArray.Append(l);
    //    }
    //    Transaction tr = new Transaction(doc, "thsi");
    //    tr.Start();
    //    doc.Create.NewFloor(floorCurveArray, false);
    //    tr.Commit();
    //}
    #endregion


}
