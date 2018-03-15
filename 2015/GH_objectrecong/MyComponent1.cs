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
    public class MyComponent1 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MyComponent1()
            : base("CADOBJECT", "Cobj", "Cobj", "Extra", "CADrecon")
        {
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{fe75b036-8ac3-4b00-abab-fc90eebfe8fe}"); }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Object Direction Vector", "L", "Vector", GH_ParamAccess.item);
            pManager.AddCurveParameter("All Curves in Object", "C", " All Curves", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Element", "D", "Encoded Element", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            #region setup and validation
            Line line = new Line();
            List<Curve> known = new List<Curve>();

            //Retrieve the whole list using Da.GetDataList().
            if (!DA.GetData(0,  ref line)) { return; }
            if (!DA.GetDataList(1, known)) { return; }

            #endregion

            //DATA ORGANIZATION PROCEDURES AND CLASS CREATION
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes = new Grasshopper.Kernel.Data.GH_Structure<GH_Curve>();
            GH_Path path = new GH_Path (0);
            GH_Curve ln = new GH_Curve(line.ToNurbsCurve());
            tes.Append(ln, path);

            foreach (Curve c in known)
            {
                GH_Curve nc = new GH_Curve(c);
                tes.Append(nc, path);
            }
            DA.SetDataTree(0, tes);
        }

    
    }

 







}