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


    public class UnitType
    {
        public string name { get; set; }
        public double ideallength { get; set; }
        public double idealwidth { get; set; }
        public double idealarea { get; set; }

        public bool unitdone { get; set; }
        //Bounds
        public double maxlength { get; set; }
        public double maxwidth { get; set; }
        public double minlength { get; set; }
        public double minwidth { get; set; }
        public double maxarea { get; set; }
        public double minarea { get; set; }

        public int numberofunits { get; set; }
        public int realnumberofunits { get; set; }
        public double unitdistribution { get; set; }

        //Constructors

        public UnitType(string _name, double _lenght, double _width, double _area, int _numberof)
        {
            this.unitdone = false;
            this.name = _name;
            this.ideallength = _lenght;
            this.idealwidth = _width;
            this.numberofunits = _numberof;
            this.idealarea = _area;
        }


        //public UnitType(string _name, double min_width, double _area, int _numberof)
        //{
        //    this.name = _name;
        //    this.minwidth = min_width;
        //    this.idealarea = _area;
        //    this.numberofunits = _numberof;
        //    this.name = _name;

        //}


        public UnitType(string _name, double _lenght, double _width, int _numberof,
            double max_lenght, double max_width, double min_lenght, double min_width,
            double max_area, double min_area, double ideal_area)
        {
            this.name = _name;
            this.ideallength = _lenght;
            this.idealwidth = _width;
            this.numberofunits = _numberof;

            this.name = _name;

        }
        //public double idealwidth { get; set; }

    }
}
