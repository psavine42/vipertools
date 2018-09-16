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


namespace Viper
{

    public class TwoPointSR
    {
        public List<double> pts = new List<double>();
        public int layer;

        public TwoPointSR(TwoPoint tp)
        {
            pts.Add(tp.pt1.X);
            pts.Add(tp.pt1.Y);
            pts.Add(tp.pt2.X);
            pts.Add(tp.pt2.Y);
            layer = tp.layer;
        }
    }

    //twopoint is the container class for a line that will become a pipe
    public class TwoPoint : ISerialGeom
    {
        // points of the pipe element
        public XYZ pt1 { get; set; }
        public XYZ pt2 { get; set; }
        public int layer { get; set; }
        public int pipefunction { get; set; } // 1 = horizantal 2 = riser

        //For specific Trade Data
        public Pipe pipe { get; set; }
        public Duct duct { get; set; }
        public Conduit conduit { get; set; }

        public ProjectTree tree { get; set; }
        public Element FittingInstance { get; set; }
        public MEPCurve Mepcurve { get; set; }
        public PipingSystemType pipeSystemType { get; set; }
        public string Revittypename { get; set; }
        public string Revitcategory { get; set; }

        //For rack calculations
        public double Weight { get; set; }
        public double RackLocation { get; set; }

        //For ProjectTree Maintenance
        public TwoPoint tp_Parent { get; set; }
        public List<TwoPoint> tp_child = new List<TwoPoint>();
        
        public List<double> dimensionlist { get; set; }
        public bool IsStart { get; set; }
        //private Envelope _envelope;

        //public   Envelope Envelope =>  _envelope;
      
        //Initalization methods
        #region Initialize
        public TwoPoint(XYZ ptstart, XYZ ptend, int layers, int Pipefunction)
        {
            this._setLocation(ptstart, ptend);
            layer = layers;
            pipefunction = Pipefunction;
            dimensionlist = new List<double>();
        }
        public TwoPoint()
        {
      
        }
        public TwoPoint(XYZ ptstart, XYZ ptend)
        {
           // List<double> dl =
            dimensionlist = new List<double>();
            layer = 1;
            pipefunction = 1;
            this._setLocation(ptstart, ptend);
        }

        public TwoPoint(XYZ ptstart, XYZ ptend, MEPCurve mepcrv)
        {
            dimensionlist = new List<double>();
            this.Mepcurve = mepcrv;
            layer = 1;
            pipefunction = 1;
            this._setLocation(ptstart, ptend);
        }

        public TwoPoint(MEPCurve mepcrv)
        {
            dimensionlist = new List<double>();
            this.Mepcurve = mepcrv;
            layer = 1;
            pipefunction = 1;
            this._setLocation(mepcrv);
        }

        public TwoPoint(Pipe Newpipe)
        {
            this.pipe = Newpipe;
            this.Mepcurve = pipe as MEPCurve;
            this._setLocation(pipe as MEPCurve);
        }

        private void _setLocation(MEPCurve mepcurve)
        {
            Curve loc = (this.Mepcurve.Location as LocationCurve).Curve;
            this._setLocation(loc.GetEndPoint(0), loc.GetEndPoint(1));
        }

        private void _setLocation(XYZ ptstart, XYZ ptend)
        {
            pt1 = ptstart;
            pt2 = ptend;
        }
        #endregion

        public SerializedGeom Serialize()
        {
            return new SerializedGeom(this);
        }

        //////////////////////////////////////////////////////////
        //Reporting
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

        public string reportsome()
        {
            VpReporting vpr = new VpReporting();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("------------------------------------------------------");
            sb.AppendLine("Points : ");
            sb.AppendLine(vpr.pointreport(this.pt1) + "   " + vpr.pointreport(this.pt2));
            return sb.ToString();
        }

        public void tp_BuildTraverse(ProjectTree projtree)
        {
            this.tree = projtree;
            this.tree.sb.AppendLine(this.reportsome());
            List<TwoPoint> ends = this.GettwopointbyEnds();
            this.tree.sb.AppendLine("ends found :" + ends.Count());

            // if the twopoint is not a parent, add to children
            //remove it from candidates
            foreach (TwoPoint tpchild in ends)
            {
                // twopoint tpnew = tpchild.Copy();
                this.tp_child.Add(tpchild);
                tpchild.tp_Parent = this;

                if (projtree.unclassifiedtps.Contains(tpchild) == true)
                {
                    projtree.unclassifiedtps.Remove(tpchild);
                }
            }
            this.tree.sb.AppendLine("children found " + this.tp_child.Count.ToString());
            //Run the traverse for each of hte children
            foreach (TwoPoint child in this.tp_child)
            {
                child.tp_BuildTraverse(this.tree);

            }
        }


        #region getters
        public ElementId get_systemTypeId()
        {
            return Mepcurve.MEPSystem.GetTypeId();
        }

        public ElementId getLevelId()
        {
            return Mepcurve.ReferenceLevel.Id;
        }

        public ElementId getTypeId()
        {
            return Mepcurve.GetTypeId();
        }
        #endregion
        ////////////////////////////////////////////////////////

        #region Build
        private TwoPoint MakePipe(Document doc)
        {
            Pipe pp = Mepcurve as Pipe;
            ElementId typid = getTypeId();
            ElementId typlv = getLevelId();
            ElementId typsys = get_systemTypeId();
            // MEPSystem mep = pp.MEPSystem;
            Pipe pipe = Pipe.Create(doc, typsys, typid, typlv, pt1, pt2);
            pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble());
            Makepipes.Make_Insul(pp, pipe, doc);

            // pipe.SetSystemType(tp.get_systemTypeId());
            TwoPoint tpout = new TwoPoint(pt1, pt2, pipe as MEPCurve);
            return tpout;
        }

        private TwoPoint MakeConduit(Document doc)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(ConduitType));
            ConduitType type = collector.FirstElement() as ConduitType;

            Conduit ct = this.Mepcurve as Conduit;
            GXYZ np = new GXYZ(this.pt1.X, this.pt1.Y, this.pt1.Z);
            GXYZ nz = new GXYZ(this.pt2.X, this.pt2.Y, this.pt2.Z);

            Parameter diamz = ct.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM);
            double diam = diamz.AsDouble();

            Conduit pig2 = Conduit.Create(doc, type.Id, np, nz, ct.LevelId);
            pig2.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).Set(diam);

            TwoPoint tpout = new TwoPoint(np, nz, pig2);

            return tpout;
        }

        private TwoPoint MakeDuct(Document doc)
        {
            //GXYZ np = new GXYZ(tp.pt1.X, tp.pt1.Y, tp.pt1.Z);
            //GXYZ nz = new GXYZ(tp.pt2.X, tp.pt2.Y, tp.pt2.Z);

            //Pipe pipe = doc.Create.NewPipe(np, nz, tp.);
            //pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(tp.dimensionlist.ElementAt(0));
            //twopoint tpout = new twopoint(np, nz, pipe);

            return this;
        }


        public TwoPoint BuildElement(Document doc)
        {
            if (this.Mepcurve == null)
            {
                return null;
            }
            PipeInsulation insul = Mepcurve as PipeInsulation;
            if (insul != null)
            {
                ElementId hostid = insul.HostElementId;
                this.Mepcurve = doc.GetElement(hostid) as MEPCurve;
            }
            TwoPoint tpnew;
            if (this.Mepcurve as Pipe != null)
            {
                return MakePipe(doc);

            }
            else if (this.Mepcurve as Conduit != null)
            {
                return MakeConduit(doc);
            }
            else if (this.Mepcurve as Duct != null)
            {
                return MakeDuct(doc);
            }
            else
            {
                return null;
            }
        }

        #endregion

        public double MinEndpointDistance(TwoPoint other)
        {
            var numbers = new List<double>
            { this.pt1.DistanceTo(other.pt1), this.pt1.DistanceTo(other.pt2),
              this.pt2.DistanceTo(other.pt1), this.pt2.DistanceTo(other.pt2), };
            return numbers.Min();
        }
       
        public void Reverse()
        {
            XYZ p1 = this.pt1;
            XYZ p2 = this.pt2;
            this.pt1 = p2;
            this.pt2 = p1;
        }

        //public Tuple<XYZ, XYZ> Points()
        //{
        //    return Tuple<XYZ, XYZ>(){ pt1, pt2);
        //}

        ////////////////////////////////////////////////////////

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TwoPoint Copy()
        {
            TwoPoint tp = new TwoPoint();
            tp.pt1 = this.pt1;
            tp.pt2 = this.pt2;
            tp.tree = this.tree;
            tp.Mepcurve = this.Mepcurve;
            return tp;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<TwoPoint> GettwopointbyEnds()
        {
            ConnectorSet connectors;
            List<TwoPoint> listout = new List<TwoPoint>();
            //conditions
            // 1. if it is connected or not
            // 2. if it is connected to 
            
          //  Element element = doc.ElementById(elementNode.Id);
            FamilyInstance fi = this.FittingInstance as FamilyInstance;
            if (fi != null)
            {
                connectors = fi.MEPModel.ConnectorManager.Connectors;
            }
            else
            {
                MEPCurve mepCurve = this.Mepcurve as MEPCurve;
                connectors = mepCurve.ConnectorManager.Connectors;
            }

            foreach (Connector connector in connectors)
            {
                   Connector cand;

                    cand =  GetConnectedConnector(connector);
                    this.tree.sb.AppendLine("445 " + cand.Origin.ToString());

                    if (cand == null)
                    {
                        cand = GetCloseConnector(connector.Origin);
                    }

                    this.tree.sb.AppendLine("449 " + cand.Origin.ToString());
                   if (cand != null)
                   {
                       Element owner = this.tree.doc.GetElement(cand.Owner.Id);
                       MEPCurve ownermep = owner as MEPCurve;                      

                       if (this.tp_Parent != null)
                       {
                           // if the parent is an MEPcurve, then this object might
                           // be a fitting or another mepcurve
                           // 
                           if (this.tp_Parent.Mepcurve != null)
                           {
                               //if the owner is the parent, skip this
                               if (owner.Id == this.tp_Parent.Mepcurve.Id)
                               { }
                               else
                               {
                                   //if this is an MEP Curve
                                   if (ownermep != null)
                                   {
                                       var qry = from crv in this.tree.unclassifiedtps
                                                 where crv.Mepcurve.Id == ownermep.Id
                                                 select crv;
                                       TwoPoint tpl = qry.ElementAt(0);
                                       listout.Add(tpl);
                                   }

                                   // if this is a fitting element
                                   else
                                   {
                                       TwoPoint fitting = this.AddFitting(owner);
                                       listout.Add(fitting);
                                   }
                               }
                           }
                           else
                           {
                               //if this is an MEP Curve
                               if (ownermep != null)
                               {
                                   var qry = from crv in this.tree.unclassifiedtps
                                             where crv.Mepcurve.Id == ownermep.Id
                                             select crv;
                                   TwoPoint tpl = qry.ElementAt(0);
                                   listout.Add(tpl);

                               }
                               // if this is a fitting element
                               else
                               {
                                   TwoPoint fitting = this.AddFitting(owner);
                                   listout.Add(fitting);
                               }

                           }
                       }
                       else
                       {
                           //if this is an MEP Curve
                           if (ownermep != null)
                           {
                               var qry = from crv in this.tree.unclassifiedtps
                                         where crv.Mepcurve.Id == ownermep.Id
                                         select crv;
                               TwoPoint tpl = qry.ElementAt(0);
                               listout.Add(tpl);

                           }
                           // if this is a fitting element
                           else
                           {
                               TwoPoint fitting = this.AddFitting(owner);
                               listout.Add(fitting);
                           }
                       }
                   }
                   this.tree.sb.AppendLine("error - object has no connectors in it");
                }

                return listout;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <returns></returns>
        private TwoPoint AddFitting(Element owner)
        {
            TwoPoint fitting = new TwoPoint();
            fitting.FittingInstance = owner;
            FamilyInstance fin = owner as FamilyInstance;

            ConnectorSetIterator csi = fin.MEPModel.ConnectorManager.Connectors.ForwardIterator();
            // while (csi.MoveNext())
            csi.MoveNext();
            Connector connt1 = csi.Current as Connector;
            fitting.pt1 = connt1.Origin;
            csi.MoveNext();
            Connector connt2 = csi.Current as Connector;
            fitting.pt2 = connt2.Origin;
           // listout.Add(fitting);
            return fitting;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        private Connector GetCloseConnector(XYZ origin)
        {
           // Connector connout;
            foreach (TwoPoint tp in this.tree.unclassifiedtps)
            {
                if (tp.pt1.IsAlmostEqualTo(origin))
                {
                    ConnectorSet allRefs =
                        tp.Mepcurve.ConnectorManager.Connectors;
                    foreach (Connector conn in allRefs)
                    {
                        if (conn.Origin.IsAlmostEqualTo(origin))
                        {
                            return conn;
                       // break;
                        }
                    }
                }
                if (tp.pt2.IsAlmostEqualTo(origin))
                {
                    ConnectorSet allRefs =
                        tp.Mepcurve.ConnectorManager.Connectors;
                    foreach (Connector conn in allRefs)
                    {
                        if (conn.Origin.IsAlmostEqualTo(origin))
                        {
                            return conn;
                            
                        }
                    }

                }

            }
            
            return null;
        }

        private Connector GetConnectedConnector(Connector connector)
        {
            Connector connectedConnector = null;
            ConnectorSet allRefs = connector.AllRefs;
            foreach (Connector conn in allRefs)
            {
                // Ignore non-EndConn connectors and connectors of the current element
                if (conn.ConnectorType != ConnectorType.End ||
                    conn.Owner.Id.IntegerValue.Equals(connector.Owner.Id.IntegerValue))
                {
                    continue;
                }

                connectedConnector = conn;
                break;
            }

            return connectedConnector;
        }

        private void determineMEPCurve()
        {

        }

        private double GetOwnDiameter()
        {
            //if it is conduit
            if (this.Mepcurve.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble() != null)
            {
                return this.Mepcurve.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble();
            }

            // IF it is duct
            else if (this.Mepcurve.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).AsDouble() != null)
            {
                return this.Mepcurve.get_Parameter(BuiltInParameter.RBS_CONDUIT_DIAMETER_PARAM).AsDouble();
            }

            else
            {
                return 0;
            }

        }

        //determines if two objects or on same axis
        public void Twopointsonaxis(TwoPoint known, Document doc, twopointlist tplist, double threshold, int count)
        {

            Makepipes mp = new Makepipes();
            try
            {
                for (int i = 0; i < tplist.todo.Count; i++)
                {
                    TwoPoint candidate = tplist.todo.ElementAt(i);
                    if (known != candidate)
                    {
                        // for each of the found connectors, test if it is
                        //within the distance of the connector
                        bool iswithindistance = mp.TESTdistance2point2point(known, candidate, threshold);//vpdata.Thrsh_elbow);
                        bool isonaxis = mp.TESTtwopointOnSameAxis(known, candidate);

                        if (isonaxis == true && iswithindistance == true)
                        {

                            try
                            {
                                TwoPoint tpnew = mp.TwoPointRebuild(known, candidate);
                                tplist.todo.Remove(known);
                                tplist.todo.Remove(candidate);
                                tplist.mainlist.Remove(known);
                                tplist.mainlist.Remove(candidate);

                                int n = tplist.todo.IndexOf(known);

                                tplist.mainlist.Add(tpnew);
                                tplist.todo.Insert(n, tpnew);

                                Twopointsonaxis(tplist.todo.ElementAt(n), doc, tplist, threshold, count);
                            }
                            catch (Exception)
                            { continue; }
                        }
                    }
                }
                try
                {
                    int nn = tplist.todo.IndexOf(known) + 1;
                    if (nn > tplist.todo.Count)
                    { }
                    Twopointsonaxis(tplist.todo.ElementAt(nn), doc, tplist, threshold, count);
                }
                catch (Exception) { }
            }
            catch (Exception) { }
        }


    }


    // class for cad block imported into revit
    public class BlockObject
    {
        public List<cadvector> cad { get; set; }
        public List<Line> countLine { get; set; }
        public List<Arc> countArc { get; set; }
        public List<Face> countFace { get; set; }
        public List<Solid> countSol { get; set; }
        public Transform transform { get; set; }
        public List<XYZ> points { get; set; }
        public int cadlayer { get; set; }

        //base location
        public XYZ location1 { get; set; }

        //secondary location - location on host
        public XYZ location2 { get; set; }
        public ElementId host { get; set; }
        public ElementId element { get; set; }

        private void InitLists()
        {
            cad = new List<cadvector>();
            countLine = new List<Line>();
            countArc = new List<Arc>();
            countFace = new List<Face>();
            countSol = new List<Solid>();
            points = new List<XYZ>();
        }

        public BlockObject()
        {
            InitLists();
            cadlayer = 0;
            location1 = new GXYZ();
        }

        public BlockObject(XYZ loc, ElementId hostobj)
        {
            InitLists();
            cadlayer = 0;
            location1 = loc;
            host = hostobj;
        }

        public void Add(Line line)
        {
            this.points.Add(line.GetEndPoint(0));
            this.points.Add(line.GetEndPoint(1));
            this.countLine.Add(line);
        }

        public void Add(Arc line)
        {
            this.points.Add(line.GetEndPoint(0));
            this.points.Add(line.GetEndPoint(1));
            this.countArc.Add(line);
        }

        public void Add(Solid solid)
        {
            // solid.Edges.
            this.countSol.Add(solid);
        }


        public bool compareBlockObject(BlockObject known, BlockObject test)
        {
            bool bl = false;
            if (known.ListsOfSameSize(test) == true)
            {
                if (loopcompare(known, test, "countLine") == true)
                {
                    if (loopcompare(known, test, "countArc") == true)
                    {
                        bl = true;
                    }
                    else { return false; }
                }
                else { return false; }
            }
            else { return false; }
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

        public void Sort()
        {
            try
            {
                this.countLine.OrderBy(a => a.Length);
                this.countArc.OrderBy(a => a.Length);
                this.countFace.OrderBy(a => a.Area);
                this.countSol.OrderBy(a => a.SurfaceArea);
            }
            catch (Exception e) {
                Debug.WriteLine("sorting block fail: " + e.Message);
            }
        }

        public bool ListsOfSameSize(BlockObject other)
        {
            try
            {
                if (this.countArc.Count == other.countArc.Count
                    && this.countFace.Count == other.countFace.Count
                    && this.countLine.Count == other.countLine.Count
                    && this.countSol.Count == other.countSol.Count
                    && this.cadlayer == other.cadlayer)
                return true; 
            }
            catch (Exception) { }
            return false;
        }


        public XYZ Centroid()
        {

            double nx = 0;
            double ny = 0;
            double nz = 0; 

            foreach (XYZ pt in this.points)
            {
                nx = nx + pt.X;
                ny = ny + pt.Y;
                nz = nz + pt.Z;
            }

            if (nx != 0)
            { nx = nx / this.points.Count; }
            if (ny != 0)
            { ny = ny / this.points.Count; }
            if (nz != 0)
            { nz = nz / this.points.Count; }

            XYZ centroid = new GXYZ (nx , ny ,nz );

           return centroid;
        }

        public void getmidline()
        {
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
        public List<TwoPoint> pipelist { get; set; }
    }




}
