using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI.Selection;
using Viper.Viper2d;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Viper
{
    public enum GeomType : int {
        None, Arc, Circle, Line, Polyline,
        Symbol, Solid, Face, Mesh, Point };

    public interface ISerialGeom
    {
        SerializedGeom Serialize();
    }


    // Basically A mesh exporter for Revit Geom
    // includes parameters in parametersets
    public class SerialRevit: ISerialGeom
    {
        public GeomType geomType;
        public List<double> points = new List<double>();
        public Dictionary<string, string> attrs = new Dictionary<string, string>();
        public Dictionary<string, double> attrd = new Dictionary<string, double>();
        public Dictionary<string, int> attri = new Dictionary<string, int>();

        public Dictionary<int, int[]> conref = new Dictionary<int, int[]>();
        public Dictionary<int, double[]> conpts = new Dictionary<int, double[]>();
        public List<ISerialGeom> children = new List<ISerialGeom>();

        // init -------------------------------------------------
        public SerialRevit(Element tp, Options geoOptions)
        {
            this.setLocation(tp);
            GeometryElement gm = null;
            gm = tp.get_Geometry(geoOptions);
            if (gm != null)
                this.AddSubGeoms(gm, tp.Document);
            this.AddMetaData(tp);
            this.ReadConnectivity(tp);
        }


        // Location Geometry -------------------------------------
        private void setLocation(Element el)
        {
            if ((el as MEPCurve) != null)
            {
                MEPCurve mepcurve = el as MEPCurve;
                this.geomType = GeomType.Line;
                LocationCurve lc = mepcurve.Location as LocationCurve;
                if (null != lc)
                {
                    Curve c = lc.Curve;
                    this.AddPoint(c.GetEndPoint(0));
                    this.AddPoint(c.GetEndPoint(1));
                }
            }
            else if ((el as FamilyInstance) != null)
            {
                this.geomType = GeomType.Point;
                FamilyInstance inst = el as FamilyInstance;
                LocationPoint lc = inst.Location as LocationPoint;
                if (null != lc)
                    this.AddPoint(lc.Point);
            }
            else
            {
                this.geomType = GeomType.None;
            }
        }

        private void GetTriangular(Document document, Solid solid, Transform transform)
        {
            // a solid has many faces
            FaceArray faces = solid.Faces;
            bool hasTransform = (null != transform);
            if (0 == faces.Size)
                return;

            foreach (Face face in faces)
            {
                if (face.Visibility != Autodesk.Revit.DB.Visibility.Visible)
                    continue;
                
                Mesh mesh = face.Triangulate();
                if (null == mesh)
                    continue;
                
                for (int ii = 0; ii < mesh.NumTriangles; ii++)
                {
                    MeshTriangle triangular = mesh.get_Triangle(ii);
                    try
                    {
                        XYZ[] triPnts = new XYZ[3];
                        for (int n = 0; n < 3; ++n)
                        {
                            XYZ point = triangular.get_Vertex(n);
                            this.AddPoint(point, transform);
                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
            }
        }

        public void AddSubGeoms(GeometryElement geoElement, Document doc)
        {
            foreach (GeometryObject go in geoElement)
            {
                Transform trsf = null;
                Solid solid = go as Solid;
                if (null != solid)
                {
                    GetTriangular(doc, solid, trsf);
                    continue;
                }

                // if the type of the geometric primitive is instance
                GeometryInstance instance = go as GeometryInstance;
                if (null != instance)
                {
                    ScanGeometryInstance(doc, instance, trsf);
                    continue;
                }

                GeometryElement geomElement = go as GeometryElement;
                if (null != geomElement)
                {
                    AddSubGeoms(geomElement, doc);
                }
            }
        }

        private void ScanGeometryInstance(Document document, GeometryInstance instance, Transform transform)
        {
            GeometryElement instanceGeometry = instance.SymbolGeometry;
            if (null == instanceGeometry)
            {
                return;
            }
            Transform newTransform;
            if (null == transform)
            {
                newTransform = instance.Transform;
            }
            else
            {
                newTransform = transform.Multiply(instance.Transform);	// get a transformation of the affine 3-space
            }
            AddSubGeoms(instanceGeometry, document);
        }

        // Adders ------------------------------------------------
        public void AddItem(ISerialGeom geom_item)
        {
            this.children.Add(geom_item);
        }

        private void AddPoint(XYZ xyz, Transform transform)
        {
            if (xyz == null)
                return;

            if (transform != null)
            {
                var xyz2 = transform.OfPoint(xyz);
                points.Add(xyz2.X);
                points.Add(xyz2.Y);
                points.Add(xyz2.Z);
            }
            else
            {
                points.Add(xyz.X);
                points.Add(xyz.Y);
                points.Add(xyz.Z);
            }
        }

        private void AddPoint(XYZ xyz)
        {
            if (xyz != null)
            {
                points.Add(xyz.X);
                points.Add(xyz.Y);
                points.Add(xyz.Z);
            }
        }

        // MetaData ----------------------------------------------
        private void ReadConnectivity(Element cnr)
        {
            MEPCurve cx = cnr as MEPCurve;
            if (cx == null)
            {
                return;
            }
            ConnectorManager cn = cx.ConnectorManager; 
            if (cn != null)
            {
                for (int i = 0; i < cn.Connectors.Size; i++)
                {
                    Connector con = cn.Lookup(i);
                    if (con == null)
                    {
                        continue;
                    }
                    try
                    {
                        if (con.IsConnected == true)
                        {
                            foreach (Connector other in con.AllRefs)
                            {
                                int other_owner = other.Owner.Id.IntegerValue;
                                if (other.ConnectorType != ConnectorType.End ||other.Owner.Id.IntegerValue.Equals(con.Owner.Id.IntegerValue))
                                {
                                    continue;
                                }
                                if (con != null && other_owner.Equals(other.Owner.Id.IntegerValue))
                                {
                                    continue;
                                }
                                this.conref[con.Id] = new int[] { other_owner, other.Id };
                            } 
                        }
                        this.conpts[con.Id] = new double[] { con.Origin.X, con.Origin.Y, con.Origin.Z };
                    }
                    catch { }
                }
            }
        }

        private void AddMetaData(Element e)
        {
            this.attri["ElementID"] = e.Id.IntegerValue;
            this.attrs["Category"] = e.Category.Name;
            this.attri["CategoryID"] = e.Category.Id.IntegerValue;
            foreach(Parameter p in e.ParametersMap)
            {
                if (p.HasValue == true)
                {
                    if (p.StorageType == StorageType.Double)
                    {
                        this.attrd[p.Definition.Name] = p.AsDouble();
                    }
                    else if (p.StorageType == StorageType.String)
                    {
                        this.attrs[p.Definition.Name] = p.AsString().ToString();
                    }
                    else if (p.StorageType == StorageType.ElementId)
                    {
                        this.attri[p.Definition.Name] = p.AsElementId().IntegerValue;
                    }
                    else if (p.StorageType == StorageType.Integer)
                    {
                        this.attri[p.Definition.Name] = p.AsInteger();
                    }
                }
            }
        }

        // serialization ----------------------------------------
        public SerializedGeom Serialize()
        {
            SerializedGeom outs = new SerializedGeom();
            outs.geomType = this.geomType;
            outs.attrs = this.attrs;
            outs.attri = this.attri;
            outs.attrd = this.attrd;
            outs.points = this.points;
            outs.conpts = this.conpts;
            outs.conref = this.conref;
            return outs;
        }


    }

    // Heirarchical export of CAD geometry 
    public class SerializedGeom : ISerialGeom
    {
        public int layer;
        public string layerName;
        public Dictionary<string, string> attrs;
        public Dictionary<string, double> attrd;
        public Dictionary<string, int> attri;
        public Dictionary<int, int[]> conref ;
        public Dictionary<int, double[]> conpts ;

        public GeomType geomType;
        public List<double> points = new List<double>();
        public List<ISerialGeom> children = new List<ISerialGeom>();

        public static SerializedGeom FromObject(GeometryObject obj, Transform transform, Document doc)
        {
            /// GeometryObject -> 
            if (obj is Line)
                return new SerializedGeom(obj as Line, transform, doc);

            else if (obj is Face)
                return new SerializedGeom(obj as Face, transform, doc);
           
            else if (obj is Arc)
                return new SerializedGeom(obj as Arc, transform, doc);

            else if (obj is PolyLine)
                return new SerializedGeom(obj as PolyLine, transform, doc);

            else if (obj is Curve)
                return new SerializedGeom(obj as Curve, transform, doc);

            else if (obj is Solid)
                return new SerializedGeom(obj as Solid, transform, doc);

            else if (obj is Mesh)
                return new SerializedGeom(obj as Mesh, transform, doc);

            else if (obj is GeometryInstance)
                return new SerializedGeom(obj as GeometryInstance, transform, doc);    
            else
                return new SerializedGeom();
        }

        public static List<ISerialGeom> FromCadImport(ImportInstance cad)
        {
            Options opt = new Options();
            List<ISerialGeom> geoms = new List<ISerialGeom>();
            Transform transf = null;

            foreach (GeometryObject geoObj in cad.get_Geometry(opt))
            {
                if (geoObj is GeometryInstance)
                {
                    geoms.Add(SerializedGeom.FromObject(geoObj, transf, cad.Document));
                }
            }
            return geoms;
        }

        // init -------------------------------------------------
        public SerializedGeom(TwoPoint tp)
        {
            this.geomType = GeomType.Line;
            this.layer = tp.layer;
            this.AddPoint(tp.pt1);
            this.AddPoint(tp.pt2);
        }

        public SerializedGeom() {
            this.geomType = GeomType.None;
        }

        public SerializedGeom(Arc geom, Transform transform, Document doc)
        {
            if (geom.IsBound == true)
                this.geomType = GeomType.Arc;
            else
                this.geomType = GeomType.Circle;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);
            //
            this.AddPoint(geom.Center, transform);
            this.points.Add(geom.Radius);
        }

        public SerializedGeom(Line geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Line;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);
            this.AddPoint(geom.GetEndPoint(0), transform);
            this.AddPoint(geom.GetEndPoint(1), transform);
        }

        public SerializedGeom(Face geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Face;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);
            var loops = geom.GetEdgesAsCurveLoops();
            for (int i = 0; i < loops.Count; i++)
            {
                List<Curve> loop = loops[i].ToList();
                for (int j = 0; j < loop.Count(); j++)
                {
                    Curve cr = loop[j];
                    this.AddItem(FromObject(cr, transform, doc));
                }
            }
        }

        public SerializedGeom(Curve geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Line;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);
            //
            this.AddPoint(geom.GetEndPoint(0), transform);
            this.AddPoint(geom.GetEndPoint(1), transform);
        }

        public SerializedGeom(Mesh geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Mesh;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);

            for (int i = 0; i < geom.NumTriangles; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var tri = geom.get_Triangle(i).get_Vertex(j);
                    this.AddPoint(tri, transform);
                }
            }
        }

        public SerializedGeom(PolyLine geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Polyline;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);
            //
            IList<XYZ> plist = geom.GetCoordinates();
            for (int i = 0; i < plist.Count; i++)
            {
                this.AddPoint(plist[i], transform);
            }
        }

        public SerializedGeom(Solid geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Solid;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);
            // 
            foreach (Face subgeom in geom.Faces)
            {
                this.AddItem(FromObject(subgeom, transform, doc));
            }
        }
        public SerializedGeom(GeometryInstance geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Symbol;
            Transform trans = geom.Transform;
            foreach (var subgeom in geom.SymbolGeometry)
            {
                this.AddItem(FromObject(subgeom, trans, doc));
            }
        }

        // ----------------------------------------------------
        private void AddLayerInfo(Document doc, ElementId eid)
        {
            GraphicsStyle gStyle = doc.GetElement(eid) as GraphicsStyle;
            this.layer = eid.IntegerValue;
            if (gStyle != null)
            {
                this.layerName = gStyle.Name;
            }    
        }

        public SerializedGeom Serialize()
        {
            return this;
        }

        public void AddItem(ISerialGeom geom_item)
        {
            this.children.Add(geom_item);
        }
        private void AddPoint(XYZ xyz, Transform transform)
        {
            if (transform != null)
            {
                var xyz2 = transform.OfPoint(xyz);
                points.Add(xyz2.X);
                points.Add(xyz2.Y);
                points.Add(xyz2.Z);
            }
            else
            {
                points.Add(xyz.X);
                points.Add(xyz.Y);
                points.Add(xyz.Z);
            }
        }

        private void AddPoint(XYZ xyz)
        {
            points.Add(xyz.X);
            points.Add(xyz.Y);
            points.Add(xyz.Z);
        }
    }
}
