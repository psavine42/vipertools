using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk;
using System.IO;
//using Rhino.Geometry;

namespace Revit.SDK.Samples.UIAPI.CS.Starwood
{

    public class SWstart
    {
        double inches = 1;
        public Autodesk.Revit.DB.Line line { get; set; }
        public List<Autodesk.Revit.DB.Line> centerlines = new List<Autodesk.Revit.DB.Line>();

        public Document doc; 
        public int totalunits = 100;
        public StringBuilder sb = new StringBuilder();
        public List<UnitType> unittypes;// = new List<UnitType>();
        public int randwidth = 15;
        private double depth = 20;

        private StarUtils su = new StarUtils();
        private List<Project> projects = new List<Project>();

        // MAIN - class entry point 
        public void Main(Line inline, Document _doc)
        {
            //inititalize
            this.line = inline;
            this.doc = _doc;
            StringBuilder sd = new StringBuilder();
            Random rr = new Random();

            //create the base data
            // createrandomdistribution(unittypes);
            List<UnitType> unittypes = 
                controlleddistribution();
            double idealtotalarea = su.calculateidealarea(unittypes);
            double totallength = su.calculatetotalhall_length(unittypes);
            double numfloors =  totallength/ line.Length;

            //Reporting
            sd.AppendLine(idealtotalarea.ToString());
            sd.AppendLine(totallength.ToString());
            sd.AppendLine(numfloors.ToString());

            //run one iteration of montecarlo
            for (int i = 0; i < 1; i++)
            {
                rr.Next();
                Project proj = new Project(new List<UnitType>(unittypes), i, doc);
                this.projects.Add(proj);
                runMonteCarlo_v1(proj, line,  rr );
            }

           // Project p = comparesimulations(projects);
           // p.buildprojectinRevit();
            //results reporting
            foreach (Project project in projects)
            {
                project.buildprojectinRevit();
                sb.Append(project.showprojectdata());
            }
            TaskDialog.Show("Results", sb.ToString());
        }


        // returns most successful simulation out of all.
        private Project comparesimulations(List<Project> projects)
        {
            Project p = projects.ElementAt(0);
            foreach (Project project in projects)
            {
                double dl = project.computetotalarea();
                if (dl > p.computetotalarea())
                {
                    p = project;
                }
            }
            return p;
        }

        // one iteration of a montecarlo - reports 
        public void runMonteCarlo_v1(Project proj, Line line, Random rr)
        {           
            //initialize new project
           // Project proj = new Project(unittypes, j, doc);
            
            XYZ templocaiton = line.GetEndPoint(0) as XYZ;
            this.centerlines.Add(line);

            //
            //determinecorners();

            double totallength = su.calculatetotalhall_length(proj.typelist);
            double numfloors =  totallength/ line.Length + 1;

            for (int q = 0; q < numfloors; q++)
                 {
                  projLevel pl = new projLevel(q);
                  proj.levels.Add(pl);
                 }

            double spaceleft = line.Length;

            #region safe
             //create units and locations 
            while (spaceleft > 0 && proj.projcomplete() == false)
            {
                // next random number
                rr.Next();
                //check list of units which can be added
                List<int> distlist = new List<int>();
                for (int i = 0; i < proj.typelist.Count; i++)
                {
                    if (proj.typelist.ElementAt(i).unitdone == false)
                    {
                        distlist.Add(proj.typelist.ElementAt(i).numberofunits);
                    }
                    else { sb.Append("done"); }
                }
                var loadedDie = new LoadedDie(distlist.ToArray(), rr);

                //pick a type from that list
                int number = loadedDie.NextValue();
                UnitType ut = proj.typelist.ElementAt(number);
                
                spaceleft = spaceleft - ut.idealwidth;
                if (spaceleft > 0  )
                {
                    foreach(projLevel plev in proj.levels)
                    {
                    // pick possible alternate unit?
                     XYZ preunitlocation = line.Evaluate(spaceleft, false);
                     proj.addunit(preunitlocation, ut, plev, 1, line);
                    }
                    
                }
                else break;
            }
            #endregion
        }
       
        // one iteration of a montecarlo - reports 
        public void runMonteCarlo_v2(Project proj, Line line, Random rr)
        {
            //initialize new project
            // Project proj = new Project(unittypes, j, doc);

            XYZ templocaiton = line.GetEndPoint(0) as XYZ;
            this.centerlines.Add(line);

            //
            //determinecorners();

            double totallength = su.calculatetotalhall_length(proj.typelist);
            double numfloors = totallength / line.Length + 1;

            for (int q = 0; q < numfloors; q++)
            {
                projLevel pl = new projLevel(q);
                proj.levels.Add(pl);
            }

            double spaceleft = line.Length;

            #region safe
            //create units and locations 
            while (spaceleft > 0 && proj.projcomplete() == false)
            {
                // next random number
                rr.Next();
                //check list of units which can be added
                //pick a type from that list
                var loadedDie = new LoadedDie2(rr, proj.typelist );
                int number = loadedDie.NextValue();
                UnitType ut = proj.typelist.ElementAt(number);

                spaceleft = spaceleft - ut.idealwidth;
                if (spaceleft > 0)
                {
                    foreach (projLevel plev in proj.levels)
                    {
                        // pick possible alternate unit?
                        XYZ preunitlocation = line.Evaluate(spaceleft, false);
                        proj.addunit(preunitlocation, ut, plev, 1, line);
                    }

                }
                else break;
            }
            #endregion
        }

        //
        public void determinecorners()
        {
            if (centerlines.Count > 1)
            {
                for (int q = 0; q < centerlines.Count; q++)
                {
                    XYZ p1 = centerlines.ElementAt(q).GetEndPoint(0);
                    XYZ p2 = centerlines.ElementAt(q).GetEndPoint(1);
                    for (int i = 0; i < centerlines.Count; i++)
                    {
                        if (i != q)
                        {
                           // centerlines.ElementAt(i).Distance(
                        }
                    }
                }

            }
        }

        public void createcornerzone() // needs corner location and range of outside
        // can be some function of width and random +- with range
        {

        }

        public void roughcornerzonearea()
        {


        }

        //
        public void buildhalldata()
        {

        }

        // creating unit types data options 
        #region dataimport options

        //place holder to load from excel/csv
        public void loaddata()
        {

        }

        public List<UnitType> controlleddistribution()
        {
            
            this.unittypes.Add(new UnitType("type1 - 12'", 20 * inches, 12 * inches, 240, 30));
            this.unittypes.Add(new UnitType("type2 - 10'", 20 * inches, 10 * inches, 200, 20));
            this.unittypes.Add(new UnitType("type3 - 15'", 20 * inches, 15 * inches, 300, 50));
            return unittypes;
        }

        // random distribution created for testing purposes
        //
        public void createrandomdistribution(List<UnitType> unittypes)
        {
            double dl = 0;
            while (dl < totalunits)
            {
                Random r = new Random();
                int num = Convert.ToInt32(r.Next(totalunits / 2) );
                Random r2 = new Random();

                // 5 feet is the plus minus for a range max/min of hotel room size
                int dd = Convert.ToInt32(r2.Next(5));
                double sw = dd + randwidth;

                UnitType nt = new UnitType(sw.ToString(), 15*inches, sw, sw*15 , num);
                dl = dl + num;
                unittypes.Add(nt);
            }

            computeUnitDistributionsfromUnitQuantities(unittypes);
        }

        #endregion 

        #region helpermethods
        // this should stay as its onw module - move into a type module
        public void computeUnitDistributionsfromUnitQuantities(List<UnitType> unittypes)
        {
            foreach(UnitType utype in unittypes)
            {
                utype.unitdistribution = utype.numberofunits / totalunits;
            }
        }

                ///////////////
        ////Helpermethods
        ///////////////
        public void debug1()
        {
            foreach(UnitType ut in unittypes)
            {
            sb.AppendLine(ut.name + " " + ut.idealwidth.ToString()
                + " " + ut.numberofunits.ToString());
            }
            sb.AppendLine(doc.ToString());
            sb.AppendLine(line.Origin.ToString());

            TaskDialog.Show("test", sb.ToString());
        }

        public void debug2()
        {
            foreach(UnitType ut in unittypes)
            {
            sb.AppendLine(ut.name + " " + ut.idealwidth.ToString()
                + " " + ut.numberofunits.ToString());
            }
            sb.AppendLine(doc.ToString());
            sb.AppendLine(line.Origin.ToString());

            TaskDialog.Show("test", sb.ToString());
        }

         #endregion
    }

    public class projLevel
    {
        //Properties
        public int levelnumber {get ; set;}


        //Constructors
        public projLevel (int number)
        {
            this.levelnumber = number;
        }


        //Methods

    }


}
