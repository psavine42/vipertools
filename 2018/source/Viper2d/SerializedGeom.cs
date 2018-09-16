using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.UI.Selection;
using Viper.Viper2d;
using System.Diagnostics;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Viper
{
    public enum GeomType : int { None, Arc, Circle, Line, Polyline, Symbol, Solid, Face };

    public interface ISerialGeom
    {
        SerializedGeom Serialize();
    }

    public class SerializedGeom : ISerialGeom
    {
        public int layer;
        public string layerName;
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
            else if (obj is Curve)
                return new SerializedGeom(obj as Curve, transform, doc);
            else if (obj is Arc)
                return new SerializedGeom(obj as Arc, transform, doc);
            else if (obj is PolyLine)
                return new SerializedGeom(obj as PolyLine, transform, doc);
            else if (obj is Solid)
                return new SerializedGeom(obj as Solid, transform, doc);
            else if (obj is GeometryInstance)
                return new SerializedGeom(obj as GeometryInstance, transform, doc);    
            else
                return new SerializedGeom();
        }

        //(curDoc.GetElement(curObj.GraphicsStyleId) as GraphicsStyle).Name;

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
            this.AddPoint(transform.OfPoint(geom.Center));
            this.points.Add(geom.Radius);
        }

        public SerializedGeom(Line geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Line;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);
            //
            this.AddPoint(transform.OfPoint(geom.GetEndPoint(0)));
            this.AddPoint(transform.OfPoint(geom.GetEndPoint(1)));
        }

        public SerializedGeom(Face geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Face;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);
            //
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
            this.AddPoint(transform.OfPoint(geom.GetEndPoint(0)));
            this.AddPoint(transform.OfPoint(geom.GetEndPoint(1)));
        }
        public SerializedGeom(PolyLine geom, Transform transform, Document doc)
        {
            this.geomType = GeomType.Polyline;
            this.AddLayerInfo(doc, geom.GraphicsStyleId);
            //
            IList<XYZ> plist = geom.GetCoordinates();
            for (int i = 0; i < plist.Count; i++)
            {
                this.AddPoint(transform.OfPoint(plist[i]));
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="eid"></param>
        private void AddLayerInfo(Document doc, ElementId eid)
        {
            GraphicsStyle gStyle = doc.GetElement(eid) as GraphicsStyle;
            this.layer = eid.IntegerValue;
            this.layerName = gStyle.Name;
        }

        public SerializedGeom Serialize()
        {
            return this;
        }

        public void AddItem(ISerialGeom geom_item)
        {
            this.children.Add(geom_item);
        }

        private void AddPoint(XYZ xyz)
        {
            points.Add(xyz.X);
            points.Add(xyz.Y);
            points.Add(xyz.Z);
        }
    }
}
