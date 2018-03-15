using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk;
using System.IO;

namespace Revit.SDK.Samples.UIAPI.CS.Starwood
{
    //Main class containing an iteration. 
    public class Project
    {
        #region Properties
        //metadata
        public double inches = 1;
        public double idealarea;

        //locaitondata
        public Document doc;
        public XYZ startpoint { get; set; }
        public int projectnumber;


        //containment data
        public List<UnitType> typelist { get; set; }
        public List<UnitType> usedtypelist = new List<UnitType>();
        public List<Unit> units = new List<Unit>();
        public List<projLevel> levels = new List<projLevel>();
        public StreamWriter so;
        public StringBuilder sb = new StringBuilder();// = new StringBuilder();
        //success metrics data
        //
        #endregion


        #region Main methods
        //
        public Project(Document _doc)
        {
            this.doc = _doc;
            this.so = new StreamWriter("C:\\Users\\psavine\\Desktop\\TBC_Revit Pens\\Viper\\CS\\bin\\Debug\\debug.txt", true);
            this.projectnumber = 0;
            this.so.WriteLine("PROJECT STARTED : " + projectnumber.ToString());
        }

        public Project(List<UnitType> tl, int i, Document _doc)
        {
            this.typelist = tl;
            this.projectnumber = i;
            this.doc = _doc;
            this.so = new StreamWriter("C:\\Users\\psavine\\Desktop\\TBC_Revit Pens\\Viper\\CS\\bin\\Debug\\debug.txt", true);
           // this.so.
            this.so.WriteLine("PROJECT STARTED : " + i.ToString());
        }

        // calculate the 'success' of this montecarlo project
        public double calculatetension()
        {
            double dl = 0;
            return dl;

        }


        public List<UnitType> controlleddistribution()
        {
            List<UnitType> uts = new List<UnitType>();
          //  uts.Add(new UnitType("type1 - 12'", 20 * inches, 12 * inches, 240, 30));
            uts.Add(new UnitType("type2 - 10'", 10 * inches, 16 * inches, 160, 20));
            uts.Add(new UnitType("type3 - 15'", 20 * inches, 15 * inches, 300, 50));
            return uts;
        }

        public bool projcomplete()
        {
            bool bl = true;
            foreach (UnitType type in this.typelist)
            {
                if (type.unitdone == false)
                {
                    bl = false;
                    break;
                }
            }
            return bl;
        }

        public double computetotalarea()
        {
            double totalarea = 0;
            foreach (Unit unit in this.units)
            {
                //  unit.unittype.realnumberofunits++;
                totalarea = totalarea + unit.computeArea();
            }
            return totalarea;
        }

        public string showprojectdata()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("-------------------");
            foreach (UnitType ut in this.typelist)
            {
                sb.AppendLine(ut.name + " - ideal : " + ut.numberofunits + " - real : " + ut.realnumberofunits);
            }
            sb.AppendLine("total Area : " + this.computetotalarea().ToString());
            return sb.ToString();
        }

        // creates the compact data for a single unit
        public void addunit(XYZ preunitlocation, UnitType ut, projLevel level, int side, Line line)
        {

            XYZ unitlocation = new XYZ(preunitlocation.X, preunitlocation.Y, level.levelnumber * 15);
            XYZ direction = line.Direction;
            XYZ secondpoint = line.Direction * ut.idealwidth + unitlocation;

            Unit unit = new Unit(ut, unitlocation, secondpoint);
            unit.unitwidth = ut.idealwidth;
            unit.unitlength = ut.ideallength;

            // solve for unit depth

            ut.realnumberofunits++;
            if (ut.realnumberofunits == ut.numberofunits)
            {
                this.usedtypelist.Add(ut);
                ut.unitdone = true;
                this.typelist.Remove(ut);

            }
            this.units.Add(unit);
        }

        // creates the compact data for a single unit
        public void addunit2(Unit unit)
        {

         ////   XYZ unitlocation = new XYZ(preunitlocation.X, preunitlocation.Y, level.levelnumber * 15);
         //  // XYZ direction = line.Direction;
         ////   XYZ secondpoint = line.Direction * ut.idealwidth + unitlocation;

         //   Unit unit = new Unit(ut, unitlocation, secondpoint);
         //   unit.unitwidth = ut.idealwidth;
         //   unit.unitlength = ut.ideallength;

         //   // solve for unit depth

         //   ut.realnumberofunits++;
         //   if (ut.realnumberofunits == ut.numberofunits)
         //   {
         //       this.usedtypelist.Add(ut);
         //       ut.unitdone = true;
         //       this.typelist.Remove(ut);

         //   }
            this.units.Add(unit);
        }

        public UnitType computesmallestunittype()
        {
            int unittnumber = 0;
            double unitarea = 100000;

            for (int i = 0; i < this.typelist.Count; i++)
            {
                if (unitarea > this.typelist.ElementAt(i).idealarea)
                {
                    unittnumber = i;
                }
            }
            return this.typelist.ElementAt(unittnumber);
        }

        public UnitType computelargestunittype()
        {
            int unittnumber = 0;
            double unitarea = 0;

            for (int i = 0; i < this.typelist.Count; i++)
            {
                if (unitarea < this.typelist.ElementAt(i).idealarea)
                {
                    unittnumber = i;
                }
            }
            return this.typelist.ElementAt(unittnumber);
        }

        #region Revitstuff

        // main function for all the revit stuff
        public void buildprojectinRevit()
        {
            FilteredElementCollector gFilter = new FilteredElementCollector(doc);
            ICollection<Element> z = gFilter.OfClass(typeof(FamilySymbol)).ToElements();
            FamilySymbol S_unit = z.FirstOrDefault(e => e.Name.Equals("Unit1")) as FamilySymbol;

            Transaction tr = new Transaction(doc, "tr");
            tr.Start();

            this.startpoint = new XYZ(0, this.projectnumber * 50 + 50, 0);
            foreach (Unit unit in this.units)
            {
                XYZ start = unit.unitlocation1 + startpoint;
                XYZ end = unit.unitlocation2 + startpoint;
                Plane pl = new Plane(new XYZ(0, 0, 1), start);

                XYZ startZ = new XYZ(start.X, start.Y, start.Z + 1);
                XYZ endZ = new XYZ(end.X, end.Y, end.Z + 1);

                Line line = Autodesk.Revit.DB.Line.CreateBound(start, end);
                Line lineZ1 = Autodesk.Revit.DB.Line.CreateBound(start, startZ);

                ModelCurve cr = this.doc.Create.NewModelCurve(line, SketchPlane.Create(doc, pl));

                FamilyInstance inst = doc.Create.NewFamilyInstance(start, S_unit, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                inst.get_Parameter("Length").Set(unit.unitlength);
                inst.get_Parameter("Width").Set(unit.unitwidth);
                inst.get_Parameter("VS MISC 1").Set(unit.unittype.name);
            }

            tr.Commit();
        }

        #endregion


        #endregion
    }

    public class bLine
    {
        public Line line {get ; set;}
        public bool masterline { get; set; } 

        public bLine(Line _line, bool master)
         {
             this.line = _line;
             this.masterline = master;
          }


    }



}
