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
    public class Toppath : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Toppath()
            : base("Item Path", "TPath", "Path", "Extra", "CADrecon")
        {
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{5e86efd6-8d17-4ff5-9bf0-49b6c87b74c1}"); }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Tree", "T", "Vector", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Item", "I", "Vector", GH_ParamAccess.item);
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
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> intree = new Grasshopper.Kernel.Data.GH_Structure<GH_Curve>();
            List<Curve> known = new List<Curve>();
            int n = new int(); 
            //Retrieve the whole list using Da.GetDataList().
            if (!DA.GetDataTree(0,  out intree)) { return; }
            if (!DA.GetData(1, ref n)) { return; }

            #endregion

            //DATA ORGANIZATION PROCEDURES AND CLASS CREATION
            Grasshopper.Kernel.Data.GH_Structure<GH_Curve> tes = new Grasshopper.Kernel.Data.GH_Structure<GH_Curve>();
            //GH_Path path = new GH_Path (0);
            //GH_Curve ln = new GH_Curve(line.ToNurbsCurve());
           // tes.Append(ln, path);

            foreach (Curve c in known)
            {
                GH_Curve nc = new GH_Curve(c);
               // tes.Append(nc, path);
            }
            DA.SetDataTree(0, tes);
        }

    
    }

 







}