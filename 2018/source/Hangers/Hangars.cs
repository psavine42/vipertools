
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Linq;
using System.Diagnostics;
using Autodesk.Revit;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using GXYZ = Autodesk.Revit.DB.XYZ;
using System.Windows.Forms;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.Attributes;
//using Autodesk


namespace Viper
{
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
  //  [Autodesk.Revit.Attributes.Journaling(Autodesk.Revit.Attributes.JournalingMode.NoCommandData)]

    class MakeHangars : IExternalCommand
    {
        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message,
          ElementSet elements)
        {
            Result retRes = Result.Succeeded;
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;
            Reference ref1 =
            sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "select pipes");
            Element e = doc.GetElement(ref1) as Element;
            MEPSystem sys = ExtractMechanicalOrPipingSystem(e);
            List<BlockObject> blklist = new List<BlockObject>();

            TraversalTree tree = new TraversalTree(doc, sys);
            tree.Traverse(blklist);
           
          //  string fileName = "C:\\Users\\psavine\\Desktop\\Stuff\\traversal.xml";
            tree.addhangars( blklist);
            ViperUtils vpu = new ViperUtils();
            View3D view = Get3dView(doc);

            //Get the Hangars
             FilteredElementCollector gFilter = new FilteredElementCollector(doc);
            ICollection<Element> z = gFilter.OfClass(typeof(FamilySymbol)).ToElements();
           // FamilySymbol hangar = z.FirstOrDefault(b => b.Name.Equals("Support_ClevisHangar")) as FamilySymbol;
            FamilySymbol hangar = z.FirstOrDefault(b => b.Name.Equals("ClevisHangar")) as FamilySymbol;

            Transaction trans = new Transaction(doc, "asd");
            trans.Start();
            //place instances
            TaskDialog.Show("host", blklist.Count.ToString());
            
            foreach(BlockObject blk in blklist)
            {
             //   try
             //   {

                    //XYZ ptout;
                   // Floor fl = vpu.nearestfloor(view, doc, blk.location1, 1, out ptout);
                    //      double dl = blk.location1.DistanceTo(ptout);
                    //blk.host = fl.Id;
                   // TaskDialog.Show("host", fl.Id.ToString());

                    Pipe pp = doc.GetElement(blk.host) as Pipe;
                  //  var diam = pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM);

                  //  np.MAKE_PIPES_nsimple(Hangarlocations, m_document);

                    LocationCurve lc = pp.Location as LocationCurve;
                    Line ll = lc.Curve as Line;

                    //  FamilyInstance instance = doc.Create.NewFamilyInstance
                    //                   (blk.location1, hangar, StructuralType.NonStructural);
                 //   FamilyInstance instance = doc.Create.NewFamilyInstance(ll, hangar, );
              //  }
              //  catch (Exception) { TaskDialog.}
            }

            trans.Commit();

            return retRes;
        }



        View3D Get3dView(Document doc)
        {
            FilteredElementCollector collector
              = new FilteredElementCollector(doc)
                .OfClass(typeof(View3D));

            foreach (View3D v in collector)
            {
                Debug.Assert(null != v,
                  "never expected a null view to be returned"
                  + " from filtered element collector");

                // Skip view template here because view 
                // templates are invisible in project 
                // browser

                if (!v.IsTemplate)
                {
                    return v;
                }
            }
            return null;
        }

        // Extract the system from a selected pipe element
        private MEPSystem ExtractMechanicalOrPipingSystem(Element selectedElement)
        {
            MEPSystem system = null;

            if (selectedElement is MEPSystem)
            {
                if (selectedElement is MechanicalSystem || selectedElement is PipingSystem)
                {
                    system = selectedElement as MEPSystem;
                    return system;
                }
            }
            else // Selected element is not a system
            {
                FamilyInstance fi = selectedElement as FamilyInstance;
                //
                // If selected element is a family instance, iterate its connectors and get the expected system
                if (fi != null)
                {
                    MEPModel mepModel = fi.MEPModel;
                    ConnectorSet connectors = null;
                    try
                    {
                        connectors = mepModel.ConnectorManager.Connectors;
                    }
                    catch (System.Exception)
                    {
                        system = null;
                    }

                    system = ExtractSystemFromConnectors(connectors);
                }
                else
                {
                    //
                    // If selected element is a MEPCurve (e.g. pipe or duct), 
                    // iterate its connectors and get the expected system
                    Pipe mepCurve = selectedElement as Pipe;
                    if (mepCurve != null)
                    {
                        ConnectorSet connectors = null;
                        connectors = mepCurve.ConnectorManager.Connectors;
                        system = ExtractSystemFromConnectors(connectors);

                        // connectors.s

                        // system
                        // TaskDialog.Show("asd", "ispipse");
                        // TaskDialog.Show("asd", system.UniqueId.ToString());
                    }
                }
            }



            return system;
        }

        static private MEPSystem ExtractSystemFromConnectors(ConnectorSet connectors)
        {
            MEPSystem system = null;

            if (connectors == null || connectors.Size == 0)
            {
                return null;
            }

            // Get well-connected mechanical or piping systems from each connector
            List<MEPSystem> systems = new List<MEPSystem>();
            foreach (Connector connector in connectors)
            {
                MEPSystem tmpSystem = connector.MEPSystem;
                if (tmpSystem == null)
                {
                    TaskDialog.Show("asda", "FUCK");
                    continue;
                }

                MechanicalSystem ms = tmpSystem as MechanicalSystem;
                if (ms != null)
                {
                    if (ms.IsWellConnected)
                    {
                        systems.Add(tmpSystem);
                    }
                }
                else
                {
                    //  TaskDialog.Show("asda", "adsf");
                    PipingSystem ps = tmpSystem as PipingSystem;

                    if (ps != null)// && ps.IsWellConnected)
                    {
                        // TaskDialog.Show("asda", "ispipe");
                        systems.Add(tmpSystem);

                        // TaskDialog.Show("asda", tmpSystem.Name.ToString());
                    }
                }
            }

            // If more than one system is found, get the system contains the most elements
            int countOfSystem = systems.Count;
            if (countOfSystem != 0)
            {
                int countOfElements = 0;
                foreach (MEPSystem sys in systems)
                {
                    if (sys.Elements.Size > countOfElements)
                    {
                        system = sys;
                        countOfElements = sys.Elements.Size;
                    }
                }
            }

            return system;
        }

    }


    class Makepipess
    {

        public void MAKE_PIPES_nsimple(List<GXYZ> listends, Document doc)
        {

            FilteredElementCollector collector  =  new FilteredElementCollector(doc).OfClass(typeof(PipeType));
            PipeType pipeType = collector.FirstElement() as PipeType;

            FilteredElementCollector systems = new FilteredElementCollector(doc).OfClass(typeof(MEPSystem));
            MEPSystem system = systems.FirstElement() as MEPSystem;
            
            ElementId id = pipeType.Id;
            FilteredElementCollector levs = new FilteredElementCollector(doc);
            levs.OfClass(typeof(Level));

            Level lev = levs.FirstElement() as Level;
            ElementId levid = lev.Id;

           

            Transaction trans = new Transaction(doc, "tt");
            trans.Start();
            if (null != pipeType)
            {
                foreach (GXYZ tp in listends)
                {
                    GXYZ np = new GXYZ(tp.X, tp.Y, tp.Z + 2);

                    //Pipe pipe = doc.Create.NewPipe(tp, np, pipeType);
                    Pipe pipe = Pipe.Create(doc, system.Id, pipeType.Id, levid, tp, np);

                    pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).Set(0.16667);

                }
            }
            trans.Commit();

        }
    }

    public class TreeNode
    {
        #region Member variables
        /// <summary>
        /// Id of the element
        /// </summary>
        private Autodesk.Revit.DB.ElementId m_Id;
        /// <summary>
        /// Flow direction of the node
        /// For the starting element of the traversal, the direction will be the same as the connector
        /// connected to its following element; Otherwise it will be the direction of the connector connected to
        /// its previous element
        /// </summary>
        private FlowDirectionType m_direction;
        /// <summary>
        /// The parent node of the current node.
        /// </summary>
        private TreeNode m_parent;
        /// <summary>
        /// The connector of the previous element to which current element is connected
        /// </summary>
        private Connector m_inputConnector;
        /// <summary>
        /// The first-level child nodes of the current node
        /// </summary>
        private List<TreeNode> m_childNodes;
        /// <summary>
        /// Active document of Revit
        /// </summary>
        private Document m_document;
        #endregion

        #region Properties
        /// <summary>
        /// Id of the element
        /// </summary>
        public Autodesk.Revit.DB.ElementId Id
        {
            get
            {
                return m_Id;
            }
        }

        /// <summary>
        /// Flow direction of the node
        /// </summary>
        public FlowDirectionType Direction
        {
            get
            {
                return m_direction;
            }
            set
            {
                m_direction = value;
            }
        }

        /// <summary>
        /// Gets and sets the parent node of the current node.
        /// </summary>
        public TreeNode Parent
        {
            get
            {
                return m_parent;
            }
            set
            {
                m_parent = value;
            }
        }

        /// <summary>
        /// Gets and sets the first-level child nodes of the current node
        /// </summary>
        public List<TreeNode> ChildNodes
        {
            get
            {
                return m_childNodes;
            }
            set
            {
                m_childNodes = value;
            }
        }

        /// <summary>
        /// The connector of the previous element to which current element is connected
        /// </summary>
        public Connector InputConnector
        {
            get
            {
                return m_inputConnector;
            }
            set
            {
                m_inputConnector = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="doc">Revit document</param>
        /// <param name="id">Element's Id</param>
        public TreeNode(Document doc, Autodesk.Revit.DB.ElementId id)
        {
            m_document = doc;
            m_Id = id;
            m_childNodes = new List<TreeNode>();
        }

        /// <summary>
        /// Get Element by its Id
        /// </summary>
        /// <param name="eid">Element's Id</param>
        /// <returns>Element</returns>
        private Element GetElementById(Autodesk.Revit.DB.ElementId eid)
        {
            return m_document.GetElement(eid);
        }

        /// <summary>
        /// Dump the node into XML file
        /// </summary>
        /// <param name="writer">XmlWriter object</param>
        /// 
        public void DumpIntoXML(XmlWriter writer)
        {
            // Write node information
            Element element = GetElementById(m_Id);
            FamilyInstance fi = element as FamilyInstance;
            if (fi != null)
            {
                MEPModel mepModel = fi.MEPModel;
                String type = String.Empty;
                if (mepModel is MechanicalEquipment)
                {
                    type = "MechanicalEquipment";
                    writer.WriteStartElement(type);
                }
                else if (mepModel is MechanicalFitting)
                {
                    MechanicalFitting mf = mepModel as MechanicalFitting;
                    type = "MechanicalFitting";
                    writer.WriteStartElement(type);
                    writer.WriteAttributeString("Category", element.Category.Name);
                    writer.WriteAttributeString("PartType", mf.PartType.ToString());
                }
                else
                {
                    type = "FamilyInstance";
                    writer.WriteStartElement(type);
                    writer.WriteAttributeString("Category", element.Category.Name);
                }

                writer.WriteAttributeString("Name", element.Name);
                writer.WriteAttributeString("Id", element.Id.IntegerValue.ToString());
                writer.WriteAttributeString("Direction", m_direction.ToString());
                writer.WriteEndElement();
            }
            else
            {
                String type = element.GetType().Name;

                writer.WriteStartElement(type);
                writer.WriteAttributeString("Name", element.Name);
                writer.WriteAttributeString("Id", element.Id.IntegerValue.ToString());
                writer.WriteAttributeString("Direction", m_direction.ToString());
                writer.WriteEndElement();
            }

            foreach (TreeNode node in m_childNodes)
            {
                if (m_childNodes.Count > 1)
                {
                    writer.WriteStartElement("Path");
                }

                node.DumpIntoXML(writer);

                if (m_childNodes.Count > 1)
                {
                    writer.WriteEndElement();
                }
            }
        }


        /// <summary>
        /// Add hangars to a 
        /// </summary>
        /// <param name="writer">XmlWriter object</param>
        /// 
        public void addhangarsNode(List<BlockObject> blklist)
        {
          //  List<GXYZ> Hangarlocations = new List<GXYZ>();
            try
            {
                double thresh = 3;
                Element element = GetElementById(m_Id);
                FamilyInstance fi = element as FamilyInstance;
                Pipe pp = element as Pipe;

                TaskDialog.Show("pipe", pp.Id.IntegerValue.ToString());
                if (pp != null)
                {
                    LocationCurve lc = pp.Location as LocationCurve;

                    if (lc.Curve.Length < 2.5)
                    {
                        BlockObject blk = new BlockObject(lc.Curve.Evaluate(.5, true), pp.Id);
                        blklist.Add(blk);
                        //  Hangarlocations.Add(lc.Curve.Evaluate(.5, true));
                    }

                    else
                    {
                        //Normalized
                        //  Hangarlocations.Add(lc.Curve.Evaluate(1 / lc.Curve.Length, true));
                        //  Hangarlocations.Add(lc.Curve.Evaluate((lc.Curve.Length - 1) / lc.Curve.Length, true));

                        BlockObject blk = new BlockObject(lc.Curve.Evaluate(1 / lc.Curve.Length, true), pp.Id);
                        BlockObject blk2 = new BlockObject(lc.Curve.Evaluate((lc.Curve.Length - 1) / lc.Curve.Length, true), pp.Id);
                        blklist.Add(blk);
                        blklist.Add(blk2);

                        double midlength = lc.Curve.Length - 2;
                        int numberoffittings = (int)(midlength / thresh);
                        double fittingloc = midlength / numberoffittings;

                        for (int i = 0; i < numberoffittings; i++)
                        {
                            //Hangarlocations.Add(lc.Curve.Evaluate((1 / lc.Curve.Length) + (fittingloc * i) / lc.Curve.Length, true));
                            BlockObject bkl = new BlockObject(lc.Curve.Evaluate((1 / lc.Curve.Length) + (fittingloc * i) / lc.Curve.Length, true), pp.Id);
                            blklist.Add(bkl);
                        }
                    }
                    //  np.MAKE_PIPES_nsimple(Hangarlocations, m_document);

                }

                foreach (TreeNode node in m_childNodes)
                {
                    node.addhangarsNode(blklist);
                }
            }
            catch (Exception) { }

        }





        #endregion
    }


    public class TraversalTree
    {
        #region Member variables
        // Active document of Revit
        private Document m_document;
        // The MEP system of the traversal
        private MEPSystem m_system;
        // The flag whether the MEP system of the traversal is a mechanical system or piping system
        private Boolean m_isMechanicalSystem;
        // The starting element node
        private TreeNode m_startingElementNode;
        #endregion

        #region Methods
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="activeDocument">Revit document</param>
        /// <param name="system">The MEP system to traverse</param>
        /// 
        ////////////////////////////////////// CREATION /////////////////////////////////////
        public TraversalTree(Document activeDocument, MEPSystem system)
        {
            m_document = activeDocument;
            m_system = system;
            m_isMechanicalSystem = (system is MechanicalSystem);
        }

        /// <summary>
        /// Traverse the system
        /// </summary>
        public void Traverse(List<BlockObject> blklist)
        {
            // Get the starting element node
            m_startingElementNode = GetStartingElementNode();

            // Traverse the system recursively
            Traverse(m_startingElementNode, blklist);
        }

        /// <summary>
        /// Get the starting element node.
        /// If the system has base equipment then get it;
        /// Otherwise get the owner of the open connector in the system
        /// </summary>
        /// <returns>The starting element node</returns>
        private TreeNode GetStartingElementNode()
        {
            TreeNode startingElementNode = null;

            FamilyInstance equipment = m_system.BaseEquipment;
            //
            // If the system has base equipment then get it;
            // Otherwise get the owner of the open connector in the system
            if (equipment != null)
            {
                startingElementNode = new TreeNode(m_document, equipment.Id);
            }
            else
            {
                startingElementNode = new TreeNode(m_document, GetOwnerOfOpenConnector().Id);
            }

            startingElementNode.Parent = null;
            startingElementNode.InputConnector = null;

            return startingElementNode;
        }

        /// <summary>
        /// Get the owner of the open connector as the starting element
        /// </summary>
        /// <returns>The owner</returns>
        private Element GetOwnerOfOpenConnector()
        {
            Element element = null;

            //
            // Get an element from the system's terminals
            ElementSet elements = m_system.Elements;
            foreach (Element ele in elements)
            {
                element = ele;
                break;
            }

            // Get the open connector recursively
            Connector openConnector = GetOpenConnector(element, null);

            return openConnector.Owner;
        }

        /// <summary>
        /// Get the open connector of the system if the system has no base equipment
        /// </summary>
        /// <param name="element">An element in the system</param>
        /// <param name="inputConnector">The connector of the previous element 
        /// to which the element is connected </param>
        /// <returns>The found open connector</returns>
        private Connector GetOpenConnector(Element element, Connector inputConnector)
        {
            Connector openConnector = null;
            ConnectorManager cm = null;
            //
            // Get the connector manager of the element
            if (element is FamilyInstance)
            {
                FamilyInstance fi = element as FamilyInstance;
                cm = fi.MEPModel.ConnectorManager;
            }
            else
            {
                MEPCurve mepCurve = element as MEPCurve;
                cm = mepCurve.ConnectorManager;
            }

            foreach (Connector conn in cm.Connectors)
            {
                // Ignore the connector does not belong to any MEP System or belongs to another different MEP system
                if (conn.MEPSystem == null || !conn.MEPSystem.Id.IntegerValue.Equals(m_system.Id.IntegerValue))
                {
                    continue;
                }

                // If the connector is connected to the input connector, they will have opposite flow directions.
                if (inputConnector != null && conn.IsConnectedTo(inputConnector))
                {
                    continue;
                }

                // If the connector is not connected, it is the open connector
                if (!conn.IsConnected)
                {
                    openConnector = conn;
                    break;
                }

                //
                // If open connector not found, then look for it from elements connected to the element
                foreach (Connector refConnector in conn.AllRefs)
                {
                    // Ignore non-EndConn connectors and connectors of the current element
                    if (refConnector.ConnectorType != ConnectorType.End ||
                        refConnector.Owner.Id.IntegerValue.Equals(conn.Owner.Id.IntegerValue))
                    {
                        continue;
                    }

                    // Ignore connectors of the previous element
                    if (inputConnector != null && refConnector.Owner.Id.IntegerValue.Equals(inputConnector.Owner.Id.IntegerValue))
                    {
                        continue;
                    }

                    openConnector = GetOpenConnector(refConnector.Owner, conn);
                    if (openConnector != null)
                    {
                        return openConnector;
                    }
                }
            }

            return openConnector;
        }

        /// <summary>
        /// Traverse the system recursively by analyzing each element
        /// </summary>
        /// <param name="elementNode">The element to be analyzed</param>
        private void Traverse(TreeNode elementNode, List<BlockObject> blklist)
        {
            //
            // Find all child nodes and analyze them recursively
            AppendChildren(elementNode);
            foreach (TreeNode node in elementNode.ChildNodes)
            {
                Traverse(node, blklist);
            }
        }

        /// <summary>
        /// Find all child nodes of the specified element node
        /// </summary>
        /// <param name="elementNode">The specified element node to be analyzed</param>
        private void AppendChildren(TreeNode elementNode)
        {
            List<TreeNode> nodes = elementNode.ChildNodes;
            ConnectorSet connectors;
            //
            // Get connector manager
            Element element = GetElementById(elementNode.Id);
            FamilyInstance fi = element as FamilyInstance;
            if (fi != null)
            {
                connectors = fi.MEPModel.ConnectorManager.Connectors;
            }
            else
            {
                MEPCurve mepCurve = element as MEPCurve;
                connectors = mepCurve.ConnectorManager.Connectors;
            }

            // Find connected connector for each connector
            foreach (Connector connector in connectors)
            {
                MEPSystem mepSystem = connector.MEPSystem;
                // Ignore the connector does not belong to any MEP System or belongs to another different MEP system
                if (mepSystem == null || !mepSystem.Id.IntegerValue.Equals(m_system.Id.IntegerValue))
                {
                    continue;
                }

                //
                // Get the direction of the TreeNode object
                if (elementNode.Parent == null)
                {
                    if (connector.IsConnected)
                    {
                        elementNode.Direction = connector.Direction;
                    }
                }
                else
                {
                    // If the connector is connected to the input connector, they will have opposite flow directions.
                    // Then skip it.
                    if (connector.IsConnectedTo(elementNode.InputConnector))
                    {
                        elementNode.Direction = connector.Direction;
                        continue;
                    }
                }

                // Get the connector connected to current connector
                Connector connectedConnector = GetConnectedConnector(connector);
                if (connectedConnector != null)
                {
                    TreeNode node = new TreeNode(m_document, connectedConnector.Owner.Id);
                    node.InputConnector = connector;
                    node.Parent = elementNode;
                    nodes.Add(node);
                }
            }

            nodes.Sort(delegate(TreeNode t1, TreeNode t2)
            {
                return t1.Id.IntegerValue > t2.Id.IntegerValue ? 1 : (t1.Id.IntegerValue < t2.Id.IntegerValue ? -1 : 0);
            }
            );
        }

        /// <summary>
        /// Get the connected connector of one connector
        /// </summary>
        /// <param name="connector">The connector to be analyzed</param>
        /// <returns>The connected connector</returns>
        static private Connector GetConnectedConnector(Connector connector)
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

        /// <summary>
        /// Get element by its id
        /// </summary>
        private Element GetElementById(Autodesk.Revit.DB.ElementId eid)
        {
            return m_document.GetElement(eid);
        }


        /// <summary>
        /// Dump the traversal into an XML file
        /// </summary>
        /// <param name="fileName">Name of the XML file</param>
        public void DumpIntoXML(String fileName)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = "    ";
            XmlWriter writer = XmlWriter.Create(fileName, settings);

            // Write the root element
            String mepSystemType = String.Empty;
            mepSystemType = (m_system is MechanicalSystem ? "MechanicalSystem" : "PipingSystem");
            writer.WriteStartElement(mepSystemType);

            // Write basic information of the MEP system
            WriteBasicInfo(writer);
            // Write paths of the traversal
            WritePaths(writer);

            // Close the root element
            writer.WriteEndElement();

            writer.Flush();
            writer.Close();
        }

        /// <summary>
        /// Write basic information of the MEP system into the XML file
        /// </summary>
        /// <param name="writer">XMLWriter object</param>
        private void WriteBasicInfo(XmlWriter writer)
        {
            MechanicalSystem ms = null;
            PipingSystem ps = null;
            if (m_isMechanicalSystem)
            {
                ms = m_system as MechanicalSystem;
            }
            else
            {
                ps = m_system as PipingSystem;
            }

            // Write basic information of the system
            writer.WriteStartElement("BasicInformation");

            // Write Name property
            writer.WriteStartElement("Name");
            writer.WriteString(m_system.Name);
            writer.WriteEndElement();

            // Write Id property
            writer.WriteStartElement("Id");
            writer.WriteValue(m_system.Id.IntegerValue);
            writer.WriteEndElement();

            // Write UniqueId property
            writer.WriteStartElement("UniqueId");
            writer.WriteString(m_system.UniqueId);
            writer.WriteEndElement();

            // Write SystemType property
            writer.WriteStartElement("SystemType");
            if (m_isMechanicalSystem)
            {
                writer.WriteString(ms.SystemType.ToString());
            }
            else
            {
                writer.WriteString(ps.SystemType.ToString());
            }
            writer.WriteEndElement();

            // Write Category property
            writer.WriteStartElement("Category");
            writer.WriteAttributeString("Id", m_system.Category.Id.IntegerValue.ToString());
            writer.WriteAttributeString("Name", m_system.Category.Name);
            writer.WriteEndElement();

            // Write IsWellConnected property
            writer.WriteStartElement("IsWellConnected");
            if (m_isMechanicalSystem)
            {
                writer.WriteValue(ms.IsWellConnected);
            }
            else
            {
                writer.WriteValue(ps.IsWellConnected);
            }
            writer.WriteEndElement();

            // Write HasBaseEquipment property
            writer.WriteStartElement("HasBaseEquipment");
            bool hasBaseEquipment = ((m_system.BaseEquipment == null) ? false : true);
            writer.WriteValue(hasBaseEquipment);
            writer.WriteEndElement();

            // Write TerminalElementsCount property
            writer.WriteStartElement("TerminalElementsCount");
            writer.WriteValue(m_system.Elements.Size);
            writer.WriteEndElement();

            // Write Flow property
            writer.WriteStartElement("Flow");
            if (m_isMechanicalSystem)
            {
                writer.WriteValue(ms.GetFlow());
            }
            else
            {
                writer.WriteValue(ps.GetFlow());
            }
            writer.WriteEndElement();

            // Close basic information
            writer.WriteEndElement();
        }

        /// <summary>
        /// Write paths of the traversal into the XML file
        /// </summary>
        /// <param name="writer">XMLWriter object</param>
        private void WritePaths(XmlWriter writer)
        {
            writer.WriteStartElement("Path");
            m_startingElementNode.DumpIntoXML(writer);
            writer.WriteEndElement();
        }
        #endregion

        //PS ADDITIONS
        public void addhangars(List<BlockObject> blklist)
        {
            m_startingElementNode.addhangarsNode(blklist);
        }
    }
}
