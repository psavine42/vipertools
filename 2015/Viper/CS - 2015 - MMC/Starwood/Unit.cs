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

    public class Unit
    {
        #region variables

        public double unitlength { get; set; }
        public double unitwidth { get; set; }
        public XYZ unitlocation1 { get; set; }
        public XYZ unitlocation2 { get; set; }
        public XYZ direction { get; set; }
        public List<bLine> boundaries;
        public UnitType unittype;
        public int priority { get; set; } // 0 is common items like units, 1 is common spaces

        // public Location lp ; //= new Location();
        #endregion

        #region Methods


        public Unit(UnitType ut, XYZ loc, List<bLine> _boundaries)
        {
            this.unittype = ut;
            this.unitlocation1 = loc;
            this.boundaries = _boundaries;
        }

        //Initialize method
        public Unit(UnitType ut, XYZ p1, XYZ p2)
        {
            this.unittype = ut;
            this.unitlocation1 = p1;
            this.unitlocation2 = p2;

            //   this.direction = dir;;
        }


        //get basic area
        public double computeArea()
        {
            double area = this.unitlength * this.unitwidth;
            return area;
        }

        // place a unit (master revit version)
        //public void placeaunit(UnitType tpp, Point pt, Point direction)
        //{



        //}

        #endregion


    }

}
