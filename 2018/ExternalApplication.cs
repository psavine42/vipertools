

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.Attributes;
using GXYZ = Autodesk.Revit.DB.XYZ;
using Autodesk.Revit.UI;
using Viper.Viper2d;
using Viper.Forms;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI.Events;

using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using RApplication = Autodesk.Revit.ApplicationServices.Application;
using Autodesk.Revit.DB.Structure;

using Autodesk.Revit.UI.Selection;

using IWin32Window = System.Windows.Forms.IWin32Window;
using Keys = System.Windows.Forms.Keys;
using Viper.Properties;



namespace Viper
{
   public class ExternalApp : IExternalApplication
    {
        string root = "Viper.";
        private UIControlledApplication s_uiApplication;
        static String addinPath = typeof(ExternalApp).Assembly.Location;


        BitmapSource convertFromBitmap(System.Drawing.Bitmap bitmap)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public UIControlledApplication UIControlledApplication
        {
            get { return s_uiApplication; }
        }

        void createCommandBinding(UIControlledApplication application)
        {
           RevitCommandId wallCreate = RevitCommandId.LookupCommandId("ID_NEW_REVIT_DESIGN_MODEL");
           AddInCommandBinding binding = application.CreateAddInCommandBinding(wallCreate);
           binding.Executed += new EventHandler<ExecutedEventArgs>(binding_Executed);
           binding.CanExecute += new EventHandler<CanExecuteEventArgs>(binding_CanExecute);
        }

        void addButton(RibbonPanel panel, string name, string cls)
        {
            PushButtonData pbdsl = new PushButtonData(name, name, addinPath, root + cls);
            pbdsl.LargeImage = convertFromBitmap(Resources.viper_2D_Pipe_Granade);
            PushButton pbsl = panel.AddItem(pbdsl) as PushButton;
        }


        void createRibbonButton(UIControlledApplication application)
        {
            
            application.CreateRibbonTab("Viper");
            RibbonPanel rp = application.CreateRibbonPanel("Viper", "Sleeves");
            RibbonPanel pipes = application.CreateRibbonPanel("Viper", "Pipe Tools");
            RibbonPanel apm = application.CreateRibbonPanel("Viper", "APM Tools");
            RibbonPanel dev = application.CreateRibbonPanel("Viper", "In development");

            #region Pipes
            PushButtonData pbd = new PushButtonData("Wall", "SelectBySize", addinPath, root + "SelectPipesBySize");
            ContextualHelp ch = new ContextualHelp(ContextualHelpType.ContextId, "HID_OBJECTS_WALL");
            pbd.SetContextualHelp(ch);
            pbd.LongDescription = "We redirect the wiki help for this button to Wall creation.";
            pbd.LargeImage = convertFromBitmap(Resources.viper_Select_by_Size);
            PushButton pb = pipes.AddItem(pbd) as PushButton;

            PushButtonData pbdsl = new PushButtonData("PushParams", "PushParams", addinPath, root + "ParametertrizeBOP");
            pbdsl.LargeImage = convertFromBitmap(Resources.viper_2D_Pipe_Granade);
            PushButton pbsl = pipes.AddItem(pbdsl) as PushButton;

            PushButtonData Runrack = new PushButtonData("Run Rack", "Run Rack", addinPath, root + "PipeRunRack");
            Runrack.LargeImage = convertFromBitmap(Resources.viper_2D_Pipe_Granade);
            PushButton pRunrack = pipes.AddItem(Runrack) as PushButton;

            PushButtonData Split = new PushButtonData("Split Pipe", "Split Pipe", addinPath, root + "PipeSplit");
            Split.LargeImage = convertFromBitmap(Resources.viper_2D_Pipe_Granade);
            PushButton splitpipe = pipes.AddItem(Split) as PushButton;

            PushButtonData pbd3 = new PushButtonData("Pipe Edit", "Edit Pipes", addinPath, root + "PipeRunEdit");
            pbd3.LargeImage = convertFromBitmap(Resources.viper_Extend_Pipe);
            PushButton pb3 = pipes.AddItem(pbd3) as PushButton;

            PushButtonData pbd1 = new PushButtonData("viperAll", "viperAll", addinPath, root + "ViperGrenade");
            pbd1.LargeImage = convertFromBitmap(Resources.viper_2D_Pipe_Granade);
            PushButton pb1 = pipes.AddItem(pbd1) as PushButton;
            #endregion

            #region Sleeves
            PushButtonData pbd4 = new PushButtonData("@Floor", "Sleeve-Floor",  addinPath, root + "MakePenetrations");
            pbd4.LargeImage = convertFromBitmap(Resources.viper_Create_Floor_Sleeves);
            PushButton pb4 = rp.AddItem(pbd4) as PushButton;

            PushButtonData pbd5 = new PushButtonData("@Wall", "Sleeve-Wall", addinPath, root + "MakePenetrations");
            pbd5.LargeImage = convertFromBitmap(Resources.viper_Create_Wall_Sleeves);
            PushButton pb5 = rp.AddItem(pbd5) as PushButton;

            PushButtonData pbd2 = new PushButtonData("Layout", "Layout", addinPath, root + "LocatePenetrations"); // "LocateTagGeneral");
            pbd2.LargeImage = convertFromBitmap(Resources.viper_Update_Sleeve_Tag);
            PushButton pb2 = rp.AddItem(pbd2) as PushButton;

            PushButtonData slv2 = new PushButtonData("LayoutPolar", "LayoutPolar", addinPath, root + "LocatePenetrationsPolar");
            slv2.LargeImage = convertFromBitmap(Resources.viper_Update_Sleeve_Tag);
            PushButton bslv2 = rp.AddItem(slv2) as PushButton;
            #endregion

            #region INDEV
            //second is visible
            PushButtonData v1 = new PushButtonData("Viper Dump", "Viper Dump", addinPath, root + "ViperGrenadeDump");
            v1.LargeImage = convertFromBitmap(Resources.viper_Viper_Classic);
            PushButton bv1 = dev.AddItem(v1) as PushButton;

            PushButtonData v2 = new PushButtonData("TestPipes", "TestPipes", addinPath, root + "TestPipes");
            v2.LargeImage = convertFromBitmap(Resources.viper_Block_Map);
            PushButton bv2 = dev.AddItem(v2) as PushButton;
            #endregion
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            s_uiApplication = application;
            ApplicationOptions.Initialize(this);

            createCommandBinding(application);
            createRibbonButton(application);
            
            // add custom tabs to options dialog.
            AddTabCommand addTabCommand = new AddTabCommand(application);
            addTabCommand.AddTabToOptionsDialog();
            return Result.Succeeded;
        }

        void binding_CanExecute(object sender, CanExecuteEventArgs e)
        {
            e.CanExecute = true;
        }

        void binding_Executed(object sender, ExecutedEventArgs e)
        {
           //UIApplication uiApp = sender as UIApplication;
  
        }
    }


   #region DONE

   [Transaction(TransactionMode.Manual)]
   public class SelectPipesBySize : IExternalCommand
   {

       private static RApplication m_application;
       private static UIDocument uidoc;
       private static Document doc;

       // Selects all instances of a pipe with a given size (Alan request - DONE)
       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {
           m_application = commandData.Application.Application;
           uidoc = commandData.Application.ActiveUIDocument;
           doc = commandData.Application.ActiveUIDocument.Document;
           Selection sel = uidoc.Selection;

           //prompt a selection
           Reference ref1 = sel.PickObject(ObjectType.Element, "please pick a pipe");
           Pipe pp = doc.GetElement(ref1) as Pipe;

           // if its not a pipe, command fails
           if (pp == null)
           {
               TaskDialog.Show("DUDE!", "That's not a Pipe bro");
               return Result.Failed;
           }

           //retrieve all pipes 
           FilteredElementCollector fl = new FilteredElementCollector(doc).OfClass(typeof(Pipe));

           //linq to get the pipetype and diameter
           var pipesinter = from elem in fl
                            where elem.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble() == pp.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).AsDouble()
                            where (elem as Pipe).PipeType.Name == (pp as Pipe).PipeType.Name
                            select elem.Id;

            sel.SetElementIds(pipesinter.ToList());
            return Result.Succeeded;
       }
   }

    [Transaction(TransactionMode.Manual)]
    public class SelectPipesBySlope : IExternalCommand
    {
        private static RApplication m_application;
        private static UIDocument uidoc;
        private static Document doc;

        // Selects all instances of a pipe with a given size (Val request E)
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            Selection sel = uidoc.Selection;

            //prompt a selection
            Reference ref1 = sel.PickObject(ObjectType.Element, "please pick a pipe");
            Pipe pp = doc.GetElement(ref1) as Pipe;

            // if its not a pipe, command fails
            if (pp == null)
            {
                TaskDialog.Show("DUDE!", "That's not a Pipe bro");
                return Result.Failed;
            }

            //retrieve all pipes 
            FilteredElementCollector fl = new FilteredElementCollector(doc, doc.ActiveView.Id).OfClass(typeof(Pipe));
            LocationCurve lc = pp.Location as LocationCurve;
            Line pc = lc.Curve as Line;

            //slope is 0 : pipe is horizantal
            if (lc.Curve.GetEndPoint(0).Z == lc.Curve.GetEndPoint(1).Z)
            {
                var pipesinter = from elem in fl
                                 where (elem.Location as LocationCurve).Curve.GetEndPoint(0).Z
                                    == (elem.Location as LocationCurve).Curve.GetEndPoint(1).Z
                                 where (elem as Pipe).PipeType.Name == (pp as Pipe).PipeType.Name
                                 select elem.Id;
                sel.SetElementIds(pipesinter.ToList());
            }

            // If pipe is vertical
            else if (lc.Curve.GetEndPoint(0).X == lc.Curve.GetEndPoint(1).X &&
                lc.Curve.GetEndPoint(0).Y == lc.Curve.GetEndPoint(1).Y)
            {
                var pipesinter = from elem in fl
                                 where (elem.Location as LocationCurve).Curve.GetEndPoint(0).X
                                   == (elem.Location as LocationCurve).Curve.GetEndPoint(1).X
                                   && (elem.Location as LocationCurve).Curve.GetEndPoint(0).Y
                                   == (elem.Location as LocationCurve).Curve.GetEndPoint(1).Y
                                 where (elem as Pipe).PipeType.Name == (pp as Pipe).PipeType.Name
                                 select elem.Id;
                sel.SetElementIds(pipesinter.ToList());
            }
            else
            {
                // calculate slope
                var pipesinter = from elem in fl
                                 where elem.get_Parameter(BuiltInParameter.RBS_PIPE_SLOPE).AsValueString()
                                   == pp.get_Parameter(BuiltInParameter.RBS_PIPE_SLOPE).AsValueString()
                                 where (elem as Pipe).PipeType.Name == (pp as Pipe).PipeType.Name
                                 select elem.Id;

                sel.SetElementIds(pipesinter.ToList());
            }

            //add all to selection set (---- better way?)
            return Result.Succeeded;
        }

    }

    [Transaction(TransactionMode.Manual)]
    public class RemoveShared : IExternalCommand
    {

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Result.Succeeded;
        }
    }

    // Select a Pipe by its size
    [Transaction(TransactionMode.Manual)]
    public class MakePenetrations : IExternalCommand
    {
        private static RApplication m_application;
        private static UIDocument uidoc;
        private static Document doc;

        // open the floor penetrations dialogue
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            UIApplication app = commandData.Application;

            Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Penetrate");
            tran.Start();

            Penetrations pens = new Penetrations();
            Pendata pendata = new Pendata();

            Penetraitons_Control pn = new Penetraitons_Control(pendata, doc);

            if (pn.ShowDialog() == DialogResult.OK)
            {
                pens.Sleeves_by_LinkedClash(app, pendata.filename, doc);
                tran.Commit();
            }

            return Result.Succeeded;
        }
    }

    // Locate penetrations by XY coordinate to a line
    [Transaction(TransactionMode.Manual)]
    public class LocatePenetrations : IExternalCommand
    {
        private static RApplication m_application;
        private static UIDocument uidoc;
        private static Document doc;

        // open the floor penetrations dialogue
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            UIApplication app = commandData.Application;

            Transaction tran = new Transaction(doc, "Penetrate");
            tran.Start();

            Pendata pendata = new Pendata();
            Penetraitons_Control pn = new Penetraitons_Control(pendata, doc);

            // collect boundary linesand sort into X and Y lists
            List<Element> nlx = VpObjectFinders.GetGenericfams(doc, "TBC_3D ControlLine_X");
            List<Element> nly = VpObjectFinders.GetGenericfams(doc, "TBC_3D ControlLine_Y");

            if ((nlx.Count == 0) | (nly.Count == 0))
            { 
                TaskDialog.Show("error", " no gridlines are found - looking for 'TBC_3D ControlLine'");
            }

            //get the sleeves
            List<Element> sleeves = VpObjectFinders.GetGenericfams(doc, "TBC_Round Sleeve - Floor or Wall");
            List<Element> sleevesd = VpObjectFinders.GetGenericfams(doc, "TBC_Square Sleeve - Floor or Wall");

            //locate realationship of sleeves to line by level
            Locatepointtolines(sleeves, nlx, "Distance X", "Control Line X");
            Locatepointtolines(sleeves, nly, "Distance Y", "Control Line Y");
            Locatepointtolines(sleevesd, nlx, "Distance X", "Control Line X");
            Locatepointtolines(sleevesd, nly, "Distance Y", "Control Line Y");

            tran.Commit();
            return Result.Succeeded;
        }

        public void Locatepointtolines(List<Element> sleeves, List<Element> lines, string paramtofill1, string paramtofill2)
        {
            StringBuilder sb = new StringBuilder();
            int count = 0;

            foreach (Element sleeve in sleeves)
            {
                LocationPoint sleeveloc = sleeve.Location as LocationPoint;
                XYZ pt = sleeveloc.Point;
                sb.AppendLine();
                double mindistance = 1000;
                string controlline = " ";
                ElementId ptlevel = ptlevelname(pt, doc);

                //test for sleevedir and level - as seperate funcitons
                foreach (Element line in lines)
                {
                    LocationCurve lineloc = line.Location as LocationCurve;
                    XYZ ptzright = new GXYZ(pt.X, pt.Y, lineloc.Curve.GetEndPoint(0).Z);
                    ElementId linelev = ptlevelname(lineloc.Curve.GetEndPoint(0), doc);
                    double dl = lineloc.Curve.Distance(ptzright);

                    if (mindistance > dl && ptlevel == linelev)
                    {
                        mindistance = dl;
                        controlline = line.ParametersMap.get_Item("Grid").AsString();
                        count++;
                    }
                }
                if (mindistance != 1000)
                {
                    double rounded = Math.Round(mindistance, 2);
                    ViParam.GetSet(sleeve, paramtofill1, rounded);
                    ViParam.GetSet(sleeve, paramtofill2, controlline);
                }
            }
        }

        private ElementId ptlevelname(XYZ pt, Document doc)
        {
            List<Level> levels = new FilteredElementCollector(doc)
             .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();

            ElementId levout = null;
            double dl = 1000000;
            foreach (Level lev in levels)
            {
                if (Math.Abs(lev.Elevation - pt.Z) <= dl)
                {
                    dl = Math.Abs(lev.Elevation - pt.Z);
                    levout = lev.Id;
                }
            }
            return levout;
        }
    }

    // Locate penetrations by XY coordinate to a line
    [Transaction(TransactionMode.Manual)]
   public class LocateTagGeneral : IExternalCommand
   {
       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {
           var m_application = commandData.Application.Application;
           UIDocument uidoc = commandData.Application.ActiveUIDocument;
           Document doc = commandData.Application.ActiveUIDocument.Document;
           UIApplication app = commandData.Application;

           Transaction tran = new Transaction(doc, "Penetrate");
           tran.Start();
           VpTesting vpt = new VpTesting();
           
           //// collect boundary lines and sort into X and Y lists
           List<Element> nlx = VpObjectFinders.GetGenericfams(doc, "TBC_3D ControlLine_X");
           List<Element> nly = VpObjectFinders.GetGenericfams(doc, "TBC_3D ControlLine_Y");

           if (nlx.Count == 0)
           { TaskDialog.Show("error", " no gridlines are found - looking for 'TBC_3D ControlLine_X'"); }
           if (nly.Count == 0)
           { TaskDialog.Show("error", " no gridlines are found - looking for 'TBC_3D ControlLine_Y'"); }

           ////get the sleeves
           FilteredElementCollector fg = new FilteredElementCollector(doc);

           LogicalOrFilter ortype = ViperUtils.CategoriesFilter(
               new List<BuiltInCategory>() { BuiltInCategory.OST_PipeFitting });
           List<Element> nl  = fg.OfCategory(BuiltInCategory.OST_PipeFitting)
               .OfClass(typeof(FamilyInstance)).ToElements().ToList();
            
           foreach (Element pf in nl)
           {
               XYZ pt = (pf.Location as LocationPoint).Point;
               vpt.LineDistanceToPoint(pf, pt, doc, nlx, "Distance X", "Control Line X");
               vpt.LineDistanceToPoint(pf, pt, doc, nly, "Distance Y", "Control Line Y");
               vpt.LineDistanceToPoint(pf, pt, doc, nlx, "Distance X", "Control Line X");
               vpt.LineDistanceToPoint(pf, pt, doc, nly, "Distance Y", "Control Line Y");
           }

            List<Element> sleeves = fg.OfCategory(BuiltInCategory.OST_GenericModel)
               .OfClass(typeof(FamilyInstance)).ToElements().ToList();
            tran.Commit();
           return Result.Succeeded;
       }
   }


   // Locate penetrations by distance and angle
   [Transaction(TransactionMode.Manual)]
   public class LocatePenetrationsPolar : IExternalCommand
   {

       private static RApplication m_application;
       private static UIDocument uidoc;
       private static Document doc;

        // open the floor penetrations dialogue
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication app = commandData.Application;
            m_application = app.Application;
            uidoc = app.ActiveUIDocument;
            doc = uidoc.Document;

            Transaction tran = new Transaction(doc, "Penetrate");
            tran.Start();

            //Penetrations pens = new Penetrations();
            Pendata pendata = new Pendata();
            Penetraitons_Control pn = new Penetraitons_Control(pendata, doc);

            // find boundary point
            List<Element> nlx = VpObjectFinders.GetGenericfams(doc, "TBC_3D ControlPoint");
            List<Element> nly = VpObjectFinders.GetGenericfams(doc, "TBC_3D ControlLine_Y");

            if (nlx.Count == 0)
            { TaskDialog.Show("error", " no gridlines are found - looking for 'TBC_3D ControlLine_X'"); }

            //get the sleeves
            List<Element> sleeves = VpObjectFinders.GetGenericfams(doc, "TBC_Round Sleeve - Floor or Wall");

            //locate realationship of sleeves to line by level
            Locatepointtopoint(sleeves, nlx, "Distance X", "Control Line Y");
            tran.Commit();
            return Result.Succeeded;
        }

       public void Locatepointtopoint(List<Element> sleeves, List<Element> lines, string paramtofill1, string paramtofill2)
       {
           foreach (Element sleeve in sleeves)
           {
               LocationPoint sleeveloc = sleeve.Location as LocationPoint;
               LocationPoint Controlpoint = lines.ElementAt(0).Location as LocationPoint;

               XYZ pt = sleeveloc.Point;
               double Distance = Controlpoint.Point.DistanceTo(sleeveloc.Point);

               //normalize the two points
               XYZ Controlpoint2 = new XYZ (Controlpoint.Point.X + 1, Controlpoint.Point.Y, Controlpoint.Point.Z);
               Line baseline = Line.CreateBound(Controlpoint.Point, Controlpoint2);

               Line dline = Line.CreateBound(Controlpoint.Point, sleeveloc.Point);

               double angle = XYZ.BasisY.AngleTo(dline.Direction);
               double angleDegrees = angle * 180 / Math.PI;

               if (sleeveloc.Point.X < Controlpoint.Point.X)
                   angle = 2 * Math.PI - angle;

               double angleDegreesCorrected = angle * 180 / Math.PI;
               ViParam.GetSet(sleeve, paramtofill1, Distance);
               ViParam.GetSet(sleeve, paramtofill2, angleDegreesCorrected.ToString());

            }
       }

   }

   // Push Guids to shared parameters
   [Transaction(TransactionMode.Manual)]
   public class GuidPush : IExternalCommand
   {

       private static RApplication m_application;
       private static UIDocument uidoc;
       private static Document doc;

       //http://thebuildingcoder.typepad.com/blog/2010/05/get-type-id-and-preview-image.html

       // open the floor penetrations dialogue
       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {
           m_application = commandData.Application.Application;
           uidoc = commandData.Application.ActiveUIDocument;
           doc = commandData.Application.ActiveUIDocument.Document;

           Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Penetrate");
           tran.Start();

           Selection sel = uidoc.Selection;
           IList<Reference> pickedRef = sel.PickObjects(ObjectType.Element, "Select Elements");

           int counter = 0;
           int counterg = 0;
           foreach (Reference r in pickedRef)
           {
               try
               {

                   Element elem = doc.GetElement(r);
                    ViParam.GetSet(elem, "Element ID Inst", elem.Id.ToString());
                    ViParam.GetSet(elem, "GUID Inst", elem.UniqueId.ToString());
                    
                    Type tp = elem.GetType();
                    ViParam.GetSet(elem, "Element ID Type", elem.GetTypeId().ToString());
                    ViParam.GetSet(elem, "GUID Type", elem.GetType().GUID.ToString());
                    
                   //if (elem is FamilyInstance)
                   //{
                   //     FamilyInstance fi = elem as FamilyInstance;
                   //     FamilySymbol native_el = fi.Symbol;
                   //     ViParam.GetSet(fs, "Element ID Type", fs.Id.ToString());
                   //     ViParam.GetSet(fs, "GUID Type", fs.UniqueId.ToString());
                   //}
                   //else if (elem is Wall)
                   //{
                   //     Wall wall = elem as Wall;
                   //     WallType native_el = wall.WallType; 
                   //     ViParam.GetSet(wk, "Element ID Type", wk.Id.ToString());
                   //     ViParam.GetSet(wk, "GUID Type", wk.UniqueId.ToString());
                   //}

                   //else if (elem is Floor)
                   //{
                   //     Floor fi = elem as Floor;
                   //     FloorType fs = fi.FloorType;
                   //     ViParam.GetSet(fs, "Element ID Type", fs.Id.ToString());
                   //     ViParam.GetSet(fs, "GUID Type", fs.UniqueId.ToString());

                   // }

                    counterg++;
               }
               catch (Exception) { counter = counter + 1; }
           }

           TaskDialog.Show("Errors", counterg.ToString() + "   " + counter.ToString());
           tran.Commit();

           return Result.Succeeded;
       }
   }
 
   //Create Pipes External Command
   [Transaction(TransactionMode.Manual)]
   public class PipeRunEdit : IExternalCommand
   {
        private static UIDocument uidoc;
        private static Document doc;
        static bool _place_one_single_instance_then_abort = true;
        IWin32Window _revit_window;
        List<GXYZ> _created = new List<GXYZ>();

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // DOcuments data
            UIApplication uiapp = commandData.Application;
            uidoc = uiapp.ActiveUIDocument;
            doc = uidoc.Document;
            var app = uiapp.Application;
            ViperUtils vputil = new ViperUtils();
            VpObjectFinders vpo = new VpObjectFinders();

            // USER INPUTS
            Selection selall = uidoc.Selection;
            IList<Reference> sheeps = selall.PickObjects(ObjectType.Element, "Select target");

            // Conversions/ class references.
            Element elem = doc.GetElement(sheeps.ElementAt(0));

            MEPCurve leadpipe = elem as MEPCurve;
            LocationCurve lc = leadpipe.Location as LocationCurve;
            XYZ base1 = lc.Curve.GetEndPoint(1);
            FamilySymbol fs = VpObjectFinders.GetGenericfam(doc, "Fakeline");

            // set sketchplane
            using (Transaction tran = new Transaction(doc, "Viper"))
            {
                tran.Start();
                Plane plane = Plane.CreateByNormalAndOrigin(new GXYZ(0, 0, 1), base1);
                SketchPlane sp = SketchPlane.Create(doc, plane);
                uidoc.ActiveView.SketchPlane = sp;
                uidoc.ActiveView.HideActiveWorkPlane();
                fs.Activate();
                tran.Commit();
            }

            // place fake family which is the 'drag' command              
            app.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
            try
            {
                uidoc.PromptForFamilyInstancePlacement(fs);
            }
            catch (Exception e)
            {
                 // TaskDialog.Show()
            }
            FamilyInstance inst = vputil.GetGenericinst(doc, "Fakeline");

            //retrieve the endpoint which is the final point of the chosen pipe
            Curve cc = (inst.Location as LocationCurve).Curve;
            XYZ userpoint = cc.GetEndPoint(1);

            //Get the nearest pipe to the start point of the fakeline
            MEPCurve selpipe = ViperUtils.nearestref(doc, sheeps, cc as Line);
            LocationCurve mlc = selpipe.Location as LocationCurve;
            XYZ vector = vputil.pipevector(selpipe, userpoint);

            // rebuild pipe
            using (Transaction tran2 = new Transaction(doc, "Viper"))
            {
                tran2.Start();
                foreach (Reference sheep in sheeps)
                {
                    MEPCurve pipe = doc.GetElement(sheep) as MEPCurve;
                    XYZ oldpoint = ViperUtils.nearestpipepoints(pipe, userpoint);
                    XYZ basepoint = ViperUtils.farthestpipepoints(pipe, userpoint);
                    vputil.rebuildpipevector(doc, pipe, basepoint, oldpoint, vector);
                }
                tran2.Commit();
            }

            using (Transaction tran3 = new Transaction(doc, "Viper"))
            {
                tran3.Start();
                ElementId eid = inst.Id;
                doc.Delete(eid);
                tran3.Commit();
            }
            
            return Result.Succeeded;
        }

        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            ICollection<ElementId> idsAdded
              = e.GetAddedElementIds();
            
            int n = idsAdded.Count;
            foreach (ElementId eid in idsAdded)
            {
                Curve cc = (doc.GetElement(eid).Location as LocationCurve).Curve;
                XYZ userpoint = cc.GetEndPoint(1);
                _created.Add(userpoint);
            }
            TaskDialog.Show("Added", n.ToString());
        }
    }


    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class PipeRunRack : IExternalCommand
    {
        private static UIDocument uidoc;
        private static Document doc;

        private List<ElementId> _created = new List<ElementId>();

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result retRes = Result.Succeeded;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            UIApplication uiapp = commandData.Application;
            var app = uiapp.Application;

            // user selection
            Selection selall = uidoc.Selection;
            List<MEPCurve> meps;

            if (selall.GetElementIds().Count > 0)
            {
                meps = ViperUtils.RefsToCurve(doc, selall.GetElementIds());
            }
            else
            {
                IList<Reference> sheeps = selall.PickObjects(ObjectType.Element, "Select target");
                meps = ViperUtils.RefsToCurve(doc, sheeps);
            }
            SketchPlane sp = HelperMethods.SetSketchplance(doc, meps[0], uidoc);

            // place fake family which is the 'drag' command
            FamilySymbol fs = VpObjectFinders.GetGenericfam(doc, "Fakeline");
            PromptForFamilyInstancePlacementOptions options = new PromptForFamilyInstancePlacementOptions();
            var handler = new EventHandler<DocumentChangedEventArgs>(OnDocumentChanged);
            app.DocumentChanged += handler;
            try
            {
                uidoc.PromptForFamilyInstancePlacement(fs, options);
            }
            catch (Exception e) { }
            app.DocumentChanged -= handler;


            List<Line> linelist = new List<Line>();
            foreach (ElementId id in _created)
            {
                Element inst = doc.GetElement(id);
                LocationCurve cc = inst.Location as LocationCurve;
                if (cc != null)
                {
                    linelist.Add(cc.Curve as Line);
                }
            }
            // compute each line on rack
            MEPCurve originpipe = ViperUtils.nearestref(meps, linelist.ElementAt(0));
            List<PipeRun> tpout = RackUtils.BisectingAngles(linelist, meps, originpipe);

            // transact to build pipe
            using (Transaction tran2 = new Transaction(doc, "Viper"))
            {
                tran2.Start();
                tpout.ForEach(x => x.Transact(doc));
                tran2.Commit();
            }

            CleanUp(meps, _created);
            tpout.Clear();
            selall.Dispose();
            return Result.Succeeded;
        }

        void OnDocumentChanged(object sender, DocumentChangedEventArgs e)
        {
            _created.AddRange(e.GetAddedElementIds());
        }

        private void try_del(ElementId x)
        {
            try { doc.Delete(x); } catch (Exception) { }
        }

        private void CleanUp(List<MEPCurve> pipe_refs, List<ElementId> fake_lines)
        {
            // Delete fakelines and user references 
            using (Transaction tran3 = new Transaction(doc, "Viper"))
            {
                tran3.Start();
                _created.ForEach(x => try_del(x));
                pipe_refs.ForEach(x => try_del(x.Id));
                fake_lines.Clear();
                pipe_refs.Clear();
                tran3.Commit();
            }
        }
    }


    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class PipeSplit : IExternalCommand
    {
        private static UIDocument uidoc;
        private static Document doc;

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result retRes = Result.Succeeded;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            ViperUtils vputil = new ViperUtils();
            VpObjectFinders vpo = new VpObjectFinders();
            Makepipes mp = new Makepipes();
            Selection selall = uidoc.Selection;

            XYZ p1 = selall.PickPoint();
            XYZ p2 = selall.PickPoint();

            PipeSplitData psdata = new PipeSplitData();
            PipeSplitControl pscontrol = new PipeSplitControl(psdata);

            Transaction tran2 = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran2.Start();

            if (pscontrol.ShowDialog() == DialogResult.OK)
            {
                double height = psdata.distance / 12;
                List<Element> z = VpObjectFinders.AllMEPCurves(doc);
                double tempdist = 1000;
                MEPCurve crvout = z.FirstOrDefault() as MEPCurve;
                foreach (Element e in z)
                {
                    MEPCurve crv = e as MEPCurve;
                    if (crv != null)
                    {
                        Line lt = (crv.Location as LocationCurve).Curve as Line;
                        double db = lt.Distance(p1);
                        if (db < tempdist)
                        {
                            crvout = crv;
                            tempdist = db;
                        }
                    }
                }

                Line lineout = (crvout.Location as LocationCurve).Curve as Line;
                IntersectionResult retres1 = lineout.Project(p1);
                IntersectionResult retres2 = lineout.Project(p2);

                XYZ ip1 = retres1.XYZPoint;
                XYZ ip2 = retres2.XYZPoint;

                XYZ crvpt1 = ViperUtils.nearestpipepoints(crvout, ip1);
                XYZ crvpt2 = ViperUtils.farthestpipepoints(crvout, ip1);

                Line line = Line.CreateBound(ip1, ip2);
                Line l2 = line.Clone().CreateOffset(height, new XYZ(1, 1, 0)) as Line;

                TwoPoint tp1 = new TwoPoint(crvpt1, ip1, crvout);
                TwoPoint tp2 = new TwoPoint(ip1, l2.GetEndPoint(0), crvout);
                TwoPoint tp3 = new TwoPoint(l2.GetEndPoint(0), l2.GetEndPoint(1), crvout);
                TwoPoint tp4 = new TwoPoint(l2.GetEndPoint(1), ip2, crvout);
                TwoPoint tp5 = new TwoPoint(ip2, crvpt2, crvout);

                List<TwoPoint> listout = new List<TwoPoint>() { tp1, tp2, tp3, tp4, tp5 };
                List<TwoPoint> geolist = Makepipes.MAKE_PIPES_general(listout, doc);

                // need makepipes command that will use connector manager
                // before makepipes shouls be orderpipes (to tree).

                vputil.connectrun(geolist, doc);
                doc.Delete(crvout.Id);

                tran2.Commit();
            }
            return retRes;
        }
    }


    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class PipeConnect : IExternalCommand
   {

       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;

       //EXECUTE FUNCTION
       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {
           // DOcuments data
           Autodesk.Revit.UI.Result retRes = Autodesk.Revit.UI.Result.Succeeded;
           uidoc = commandData.Application.ActiveUIDocument;
           doc = commandData.Application.ActiveUIDocument.Document;
           ViperUtils vputil = new ViperUtils();
           Makepipes mp = new Makepipes();

           // USER INPUTS
           Selection selall = uidoc.Selection;
           Reference p1 = selall.PickObject(ObjectType.Element, "Select target");

           Selection selall2 = uidoc.Selection;
           Reference p2 = selall2.PickObject(ObjectType.Element, "Select target");

           //Data transforms
           MEPCurve pp1 = doc.GetElement(p1) as Element as MEPCurve;
           LocationCurve pc1 = pp1.Location as LocationCurve;
          
           MEPCurve pp2 = doc.GetElement(p2) as Element as MEPCurve;
           LocationCurve pc2 = pp2.Location as LocationCurve;


           // Get the midpoint // good
           XYZ vector = new XYZ();
           XYZ midpoint = vputil.twopipepoint(pp1, pp2, out vector);

           //test how mant directions are in vector
           if (vector.X > 0 && vector.Y > 0 && vector.Z > 0)
           {
               // if all 3 dimensions affected
           }

           //
           else
           {
               // only two dimensions 
           }

           // get the perpendicular curve to a pipe
           XYZ vecp1 = vputil.MEPCurvetoVector(pp1);
           XYZ perpvec = vputil.GetPerpVector(vecp1);
           
          //TaskDialog.Show

           //create the line
           Line ll = Line.CreateBound(midpoint - perpvec, midpoint+ perpvec);

           //create the line from a location curve
           Line pl1 = Line.CreateBound(pc1.Curve.GetEndPoint(0), pc1.Curve.GetEndPoint(1));
           Line plz1 =Line.CreateBound(pl1.GetEndPoint(0).Subtract(pl1.Direction.Multiply(1000))
               , pl1.GetEndPoint(1).Add(pl1.Direction.Multiply(1000)));

           Line pl2 = Line.CreateBound(pc2.Curve.GetEndPoint(0), pc2.Curve.GetEndPoint(1));
           Line plz2 = Line.CreateBound(pl2.GetEndPoint(0).Subtract(pl2.Direction.Multiply(1000))
               , pl2.GetEndPoint(1).Add(pl2.Direction.Multiply(1000)));

           //intersect the lines
           IntersectionResultArray intarray1 = new IntersectionResultArray();
           ll.Intersect(plz1, out intarray1);

           IntersectionResultArray intarray2 = new IntersectionResultArray();
           ll.Intersect(plz2, out intarray2);

          
           XYZ nbp1 = ViperUtils.farthestpipepoints(pp1, intarray1.get_Item(0).XYZPoint);
           XYZ nbp2 = ViperUtils.farthestpipepoints(pp2, intarray2.get_Item(0).XYZPoint);


           FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));
            PipeType pipeType = collector.FirstElement() as PipeType;

            // FilteredElementCollector system = new FilteredElementCollector(doc).OfClass(typeof(PipeSystemType));
            //collector.OfClass(typeof(PipeType));
            //PipeType pipeType = collector.FirstElement() as PipeType;
            //PipeSystemType.

            Pipe pipe2 = doc.GetElement(p2) as Pipe;
            
            Transaction tran3 = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran3.Start();

            Pipe pipe = Pipe.Create(doc, pipe2.MEPSystem.Id, pipeType.Id, pp2.LevelId, 
                intarray1.get_Item(0).XYZPoint, intarray2.get_Item(0).XYZPoint);
            
            //Pipe pipe = doc.Create.NewPipe(intarray1.get_Item(0).XYZPoint,
            //    intarray2.get_Item(0).XYZPoint, pipeType);

            pipe.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).
                Set(pp1.get_Parameter(BuiltInParameter.RBS_PIPE_DIAMETER_PARAM).ToString());

            vputil.buildpipevector(doc, pp1, nbp1,  intarray1.get_Item(0).XYZPoint  );
            vputil.buildpipevector(doc, pp2, nbp2, intarray2.get_Item(0).XYZPoint);

            tran3.Commit();
  
            return retRes;
       }

   }

   #endregion

   #region onholddev


   [Transaction(TransactionMode.Manual)]
    public class TagAllPipeEnds : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;

        // todo test
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            //retrieve all pipes 
            FilteredElementCollector fl = new FilteredElementCollector(doc).OfClass(typeof(Pipe));
            Autodesk.Revit.DB.View view = doc.ActiveView;

           // doc.Create.NewTag(
            //add all to selection set (---- better way?)
            foreach (Pipe p in fl)
            {
                if (p == null)
                { }
                else
                {
                    LocationCurve lc = p.Location as LocationCurve;
                    GXYZ pt1 = lc.Curve.GetEndPoint(0);
                    Reference ref1 = p.ConnectorManager.Connectors.ForwardIterator().Current as Reference;
                    doc.Create.NewSpotElevation(view, ref1, pt1, new GXYZ(pt1.X + 1, pt1.Y + 1, pt1.Z),
                        new GXYZ(pt1.X + 2, pt1.Y + 1, pt1.Z), new GXYZ(pt1.X + 3, pt1.Y + 1, pt1.Z), true);
                }

            }


            return Result.Succeeded;
        }
    }


   //Create Pipes External Command
   [Transaction(TransactionMode.Manual)]
   public class MMC_SizeRacks : IExternalCommand
   {

       private static Autodesk.Revit.ApplicationServices.Application m_application;
       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;
       private ViperFormData vpdata;

       //EXECUTE FUNCTION
       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {
           Result retRes = Result.Succeeded;

           m_application = commandData.Application.Application;
           uidoc = commandData.Application.ActiveUIDocument;
           doc = commandData.Application.ActiveUIDocument.Document;
           ViperUtils vputil = new ViperUtils();

           //if hte project has a project tree, then look it up
           //if the project does not have a project tree
           ProjectTree projtree = new ProjectTree(doc);
           int count = 0;

           while (projtree.unclassifiedtps.Count > 1 && count < 1000 )
           {
              // System.Windows.Forms.MessageBox.Show(count.ToString());
               TwoPointTree tptree = new TwoPointTree(projtree.unclassifiedtps.FirstOrDefault(), projtree);
               tptree.BuildTraverse();
               
               projtree.ProjectMEPTree.Add(tptree);
               count++;
           }
          System.Windows.Forms.MessageBox.Show(projtree.sb.ToString());

           return retRes;
       }

   }

   //Create Pipes External Command
   [Transaction(TransactionMode.Manual)]
   public class SolidtoPipes : IExternalCommand
   {

       private static Autodesk.Revit.ApplicationServices.Application m_application;
       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;
       private ViperFormData vpdata;

       //EXECUTE FUNCTION
       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {
           Result retRes = Result.Succeeded;
           m_application = commandData.Application.Application;
           uidoc = commandData.Application.ActiveUIDocument;
           doc = commandData.Application.ActiveUIDocument.Document;
           ViperUtils vputil = new ViperUtils();

           Selection sel = uidoc.Selection;
           //    Reference ref1 = sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "please pick an import instance");
           //   ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
           //   if (dwg == null)
           //       return Result.Failed;
           //  XYZ point = sel.PickPoint("please pick a point");


           Reference target = uidoc.Selection.PickObject(ObjectType.Element,
               // new TargetElementSelectionFilter(),
                                                        "Select target");


           Element targetElement = doc.GetElement(target);
           TaskDialog.Show("ewr", targetElement.Category.ToString());
           IList<Solid> solids = ViperUtils.GetTargetSolids(targetElement);


           //FreeFormElement
           TaskDialog.Show("Adsa", solids.Count.ToString());
           //Object obj = sel.PickObject(;

           //  ExternalFileReference elu = ExternalFileUtils.GetExternalFileReference(doc, dwg.GetTypeId());
           //  ModelPath path = elu.GetAbsolutePath();
           //   string st = ModelPathUtils.ConvertModelPathToUserVisiblePath(path);

           List<TwoPoint> geolist = new List<TwoPoint>();

           // Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
           // tran.Start();

           vpdata = new ViperFormData(doc);
           //  Viper_Form vpform = new Viper_Form(vpdata);


           // if (vpform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
           //  {
           // TaskDialog.Show("ASD",  vpdata.pipetype.ToString() + " - " +vpdata.height.ToString() + " - " +  vpdata.diameter.ToString()  );
           //  geolist = vputil.analyzeimport2(doc, dwg, point, vpdata, geolist);

           //    tran.Commit();

           //}
           //  else { }

           //  TaskDialog.Show("ASD", "Entering fittings loop");

           //if (geolist.Count > 1)
           //{
           //    Makepipes mp = new Makepipes();
           //    mp.Connectsystemtree(geolist, doc, point);
           //}


           return retRes;
       }

   }

    #endregion
    /// <summary>
    /// Execute serialization of CAD geometry and dumpy to file
    /// </summary>
    [Transaction(TransactionMode.Manual)]
    public class ViperGrenadeDump : IExternalCommand
    {
        private static RApplication m_application;
        private static UIDocument uidoc;
        private static Document doc;
        private ViperFormData _vpdata;
        private List<XYZ> _root_points;
        public static string docu = @"
            Generate Revit pipes from autocad Drawing lines. Requires an linked dwg, and pipe families.";

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            _root_points = new List<XYZ>();
            _vpdata = new ViperFormData(doc);

            Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(ObjectType.Element, "please pick an import instance");
            ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
            if (dwg == null)
                return Result.Failed;

            _vpdata.level = dwg.LevelId;
            Makepipes mp = new Makepipes();

            AddPoint(sel);

            ShowForm(sel);
            List<ISerialGeom> geolist = SerializedGeom.FromCadImport(dwg);
            string path = @"C:\source\vipertools\data\dumps\test1.json";
            rvClient.DumpPoints(geolist, _root_points, path);

            return Result.Succeeded;
        }

        private void AddPoint(Selection sel)
        {
            XYZ pnt = sel.PickPoint("please pick a point");
            _root_points.Add(pnt);
        }

        public void ShowForm(Selection sel)
        {
            Viper_Form vpform = new Viper_Form(_vpdata);
            if (vpform.ShowDialog() == DialogResult.OK)
            {
                return;
            }
            else if (vpform.ShowDialog() == DialogResult.Yes)
            {
                AddPoint(sel);
                ShowForm(sel);
            }
        }

    }

    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class ViperGrenade : IExternalCommand
    {
        private static RApplication m_application;
        private static UIDocument uidoc;
        private static Document doc;
        private ViperFormData _vpdata;
        private List<XYZ> _root_points;
        public static string docu = @"
            Generate Revit pipes from autocad Drawing lines. Requires an linked dwg, and pipe families.";

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            _root_points = new List<XYZ>();
            _vpdata = new ViperFormData(doc);

            Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(ObjectType.Element, "please pick an import instance");
            ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
            if (dwg == null)
                return Result.Failed;

            _vpdata.level = dwg.LevelId;
            Makepipes mp = new Makepipes();

            AddPoint(sel);
            
            ShowForm(sel);
            List<ISerialGeom> geolist = SerializedGeom.FromCadImport(dwg);

            SystemData res = rvClient.SendPoints(geolist, _root_points);
            Makepipes.MakeOrdered(doc, _vpdata, res);
           
            return Result.Succeeded;
        }

        private void AddPoint(Selection sel)
        {
            XYZ pnt = sel.PickPoint("please pick a point");
            _root_points.Add(pnt);
        }

        public void ShowForm(Selection sel)
        {
            Viper_Form vpform = new Viper_Form(_vpdata);
            if (vpform.ShowDialog() == DialogResult.OK)
            {
                return;
            }
            else if (vpform.ShowDialog() == DialogResult.Yes)
            {
                AddPoint(sel);
                ShowForm(sel);
            }
        }

    }

    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class ViperGrenade2 : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Document doc;
        private ViperFormData vpdata;

        public static string docu = @"
            Generate Revit pipes from autocad Drawing lines. Requires an linked dwg, and pipe families.";

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Result retRes = Result.Succeeded;
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(ObjectType.Element, "please pick an import instance");
            ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
            if (dwg == null)
                return Result.Failed;
            XYZ point = sel.PickPoint("please pick a point");
            List<TwoPoint> geolist = new List<TwoPoint>();

            Transaction tran = new Transaction(doc, "Viper");
            tran.Start();

            vpdata = new ViperFormData(doc);
            Viper_Form vpform = new Viper_Form(vpdata);
            Makepipes mp = new Makepipes();
            ViperUtils vput = new ViperUtils();
            ElementId level = dwg.LevelId;

            if (vpform.ShowDialog() == DialogResult.OK)
            {
                geolist = ViperUtils.analyzeimport(dwg, geolist);
                // send data to server 
                TwoPoint closesttp = vput.nearest(geolist, point);
                List<TwoPoint> newlist = vput.GetLinesOnLayer(geolist, closesttp);
                //
                List<TwoPoint> newlistt = mp.Onaxistree(newlist, doc, point, vpdata);
                geolist = mp.MAKE_PIPES_new(newlistt, doc, vpdata, level);

                tran.Commit();

            }
            else { }


            if (geolist.Count > 1)
            {
                mp.Connectsystemtree(geolist, doc, point);
            }
            return retRes;
        }

      

    }

    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class ViperSome : IExternalCommand
    {

        private static RApplication m_application;
        private static UIDocument uidoc;
        private static Document doc;
        private ViperFormData vpdata;

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(ObjectType.Element, "please pick an import instance");
            ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
            if (dwg == null)
                return Result.Failed;
            XYZ point = sel.PickPoint("please pick a point");

            ExternalFileReference elu = ExternalFileUtils.GetExternalFileReference(doc, dwg.GetTypeId());
            ModelPath path = elu.GetAbsolutePath();
            string st = ModelPathUtils.ConvertModelPathToUserVisiblePath(path);
            List<TwoPoint> geolist = new List<TwoPoint>();

            Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran.Start();

            ViperUtils vpu = new ViperUtils();
            vpdata = new ViperFormData(doc);
            Viper_Form vpform = new Viper_Form(vpdata);

            if (vpform.ShowDialog() == DialogResult.OK)
            {
                tran.Commit();
            }
            else { }

            if (geolist.Count > 1)
            {
            }
            return Result.Succeeded;
        }


    }

    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class Viperblock : IExternalCommand
    {

        private static RApplication m_application;
        private static UIDocument uidoc;
        private static Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            //get the link
            Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(ObjectType.Element, "please pick an import instance");
            ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
            if (dwg == null)
                return Result.Failed;
            XYZ point = sel.PickPoint("please pick a point");

            StringBuilder sd = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            Options opt = new Options();

            ViperUtils vpu = new ViperUtils();
            Autodesk.Revit.DB.Transform transf = null;
            Makepipes mp = new Makepipes();

            List<BlockObject> finalblocks = new List<BlockObject>();
            BlockmapperFormData bmfd = new BlockmapperFormData(doc, doc.GetElement(dwg.LevelId) as Level);
            BlockMapForm vpform = new BlockMapForm(bmfd);

            Transaction trans = new Transaction(doc, "asd");
            trans.Start();

            if (vpform.ShowDialog() == DialogResult. OK)
            {
                //create block list
                foreach (GeometryObject geoObj in dwg.get_Geometry(opt))
                {
                    // TwoPoint closestitem = new TwoPoint(new XYZ(), new XYZ());
                

                    if (geoObj is GeometryInstance)
                    {
                        List<BlockObject> allblocks = vpu.determinegeoblock(geoObj, transf);
                        BlockObject bl = vpu.selectedblock(allblocks, point);

                        // determine which are same as selected
                        foreach (BlockObject blk in allblocks)
                        {

                            if (bl.ListsOfSameSize(blk) == true)
                            {
                                finalblocks.Add(blk);
                            }

                        }
                        TaskDialog.Show("fb", "all : " + allblocks.Count.ToString() + "final : " + finalblocks.Count.ToString());
                        FamilySymbol fs = bmfd.Vfamily ;

                        foreach (BlockObject blko in finalblocks)
                        {
                            // place the family instance
                            XYZ pt = blko.Centroid();
                            double M;
                            double B;
                            blko.getmidline2(out M, out B);

                            sb.AppendLine( "point " + pt.ToString() + " Line M - "
                                + M.ToString() + " Line B - " + B.ToString());
                            sd.AppendLine(blko.transform.IsTranslation.ToString()
                              + " origin " + blko.transform.Origin.ToString() 
                             + " basisX" + blko.transform.BasisX.ToString()
                             + " basisY" + blko.transform.BasisY.ToString() 
                             + "identity " + blko.transform.IsIdentity.ToString());

                            //get nearest intesection
                            if (fs == null)
                            {
                                TaskDialog.Show("no", " no family selected");
                                break;
                            }
                            
                            else
                            {
                                FamilyInstance instance = doc.Create.NewFamilyInstance
                                    (pt, fs, StructuralType.NonStructural);

                                XYZ pt2 = new GXYZ(pt.X + 2, (pt.X + 2)* M + B, pt.Z);
                                XYZ ptZ = new GXYZ(pt.X, pt.Y, pt.Z + 1);

                                GXYZ pt3 = new GXYZ(pt.X + blko.transform.BasisX.X, pt.Y + blko.transform.BasisX.Y, pt.Z);

                                Line lineZ = Line.CreateBound(pt, ptZ);
                                Line line = Line.CreateBound(pt, pt3);
                                GXYZ vector = pt.Add(pt3);
                                DetailCurve detailCurve = doc.Create.NewDetailCurve(doc.ActiveView, line);

                                double angle = blko.transform.BasisX.AngleTo(new GXYZ(1, 0 , 0));

                               LocationPoint location = instance.Location as LocationPoint;
                               location.Rotate(lineZ, angle);
                              
                            }
                        }
                        TaskDialog.Show("asd:", sb.ToString());
                        TaskDialog.Show("asd:", sd.ToString());
                        trans.Commit();
                    }
                }
            }
            else { trans.RollBack(); }

            return Result.Succeeded;
        }
    }

    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class ViperPipeblock : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;
        //  private ViperFormData vpdata;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.UI.Result retRes = Autodesk.Revit.UI.Result.Succeeded;
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            //get the link
            Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(ObjectType.Element, "please pick an import instance");
            ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
            if (dwg == null)
                return Result.Failed;
            XYZ point = sel.PickPoint("please pick a point");

            //get the cad
            ExternalFileReference elu = ExternalFileUtils.GetExternalFileReference(doc, dwg.GetTypeId());
            ModelPath path = elu.GetAbsolutePath();
            string st = ModelPathUtils.ConvertModelPathToUserVisiblePath(path);

            StringBuilder sb = new StringBuilder();
            Options opt = new Options();
            ViperUtils vpu = new ViperUtils();
            Autodesk.Revit.DB.Transform transf = null;
            Makepipes mp = new Makepipes();

            List<BlockObject> finalblocks = new List<BlockObject>();
            BlockmapperFormData bmfd = new BlockmapperFormData(doc,
                doc.GetElement(dwg.LevelId) as Level);

            // Initialize form and start transaction
            PipeBlockMapForm vpform = new PipeBlockMapForm(bmfd);
            Transaction trans = new Transaction(doc, "asd");
            trans.Start();

            if (vpform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                List<TwoPoint> tplist = new List<TwoPoint>();

                //create block list
                foreach (GeometryObject geoObj in dwg.get_Geometry(opt))
                {
                    TwoPoint closestitem = new TwoPoint(new XYZ(), new XYZ());

                    if (geoObj is GeometryInstance)
                    {
                        //convert all elements in the cad to the two point class
                        List<BlockObject> allblocks = vpu.determinegeoblock(geoObj, transf);
                        BlockObject bl = vpu.selectedblock(allblocks, point);
                        // TaskDialog.Show("SAs", allblocks.Count.ToString());

                        TaskDialog.Show("asd", "all blocks : " + allblocks.Count.ToString());
                        // determine which are same as selected
                        foreach (BlockObject blk in allblocks)
                        {
                            if (bl.cadlayer == blk.cadlayer)
                            {
                                if(bl.compareBlockObject(bl, blk) == true)
                                {
                                    finalblocks.Add(blk);
                                }
                            }
                        }

                        TaskDialog.Show("asd", "finalblocks : " + finalblocks.Count.ToString());
                        foreach (BlockObject blko in finalblocks)
                        {
                            // place the family instance
                            XYZ cntr = blko.Centroid();

                            Level lb = bmfd.cadlevel.revitobj as Level;
                            Level lt = bmfd.toplevel.revitobj as Level;

                            XYZ p1 = new GXYZ(cntr.X, cntr.Y, lb.Elevation + bmfd.bottomoffset);
                            XYZ p2 = new GXYZ(cntr.X, cntr.Y, lt.Elevation + bmfd.topoffset);
                            List<double> dimlist = new List<double>() { 0.16667 };

                            TwoPoint tp = new TwoPoint(p1, p2);
                            tp.dimensionlist = dimlist;
                            tp.pipefunction = 1;
                            tp.Revittypename = bmfd.Vtype.formname;
                            tplist.Add(tp);
                        } 
                    }
                    else { }
                }

                mp.MAKE_PIPES_simple(tplist, doc);
                trans.Commit();
            }
            else { trans.RollBack(); }
            return retRes;
        }
    }

    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class PipeEdit : IExternalCommand
    {
        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            Selection sel = uidoc.Selection;
            IList<Reference> targets = sel.PickObjects(ObjectType.Element, "Select target");

            ViperUtils vputil = new ViperUtils();
            VPipeEndFormData vpdata = new VPipeEndFormData(doc);
            PipeEnds vpform = new PipeEnds(vpdata);
            Makepipes mp = new Makepipes();

            Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran.Start();

            if (vpform.ShowDialog() == DialogResult.OK)
            {
                mp.rebuildpipes(doc, targets, vpdata);
                tran.Commit();
            }
            else { }

            return Result.Succeeded;
        }

    }

    //Create Pipes External Command
    [Transaction(TransactionMode.Manual)]
    public class TestPipes : IExternalCommand
    {
        private static RApplication m_application;
        private static UIDocument uidoc;
        private static Document doc;
         
        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            doc = commandData.Application.ActiveUIDocument.Document;
            var edgelist = new[]
            {
                new [] { 5, 5, 0, 5, 0.5, 0 },       // 0 - 0, 1 
                new [] { 4.5, 0, 0, 9.5, 0, 0},     // 1 - 0, 1
                new [] { 10, 0.5, 0, 10, 5, 0 },    // 2 - 0, 1

                new [] {15, 0, 0, 19.5, 0, 0 },    // 3 - 0, 1
                new [] { 20.5, 0, 0, 25, 0, 0},    // 4 - 0, 1
                new [] {20, 0.5, 0, 20, 5, 0 }          // 5 - 0, 1
            };

            var connlist = new[]
            {
                new []{ 0, 1, 1, 0},
                new []{ 1, 1, 2, 0 },
                new []{ 3, 1, 4, 0, 5, 0 }
            };
            
            SystemData ssd = new SystemData();
            ssd.geom = wraplist<double>(edgelist);
            ssd.symbols = new List<List<int>>();
            ssd.indicies = wraplist<int>(connlist);

            PipeType pipeType = ViParam.DefaultPipeType(doc);
            Level level = ViParam.DefaultLevel(doc);
            PipingSystemType sys = ViParam.DefaultMEPSystemType(doc);

            List<Pipe> pipes = new List<Pipe>();
            List<Connector> conns = new List<Connector>();

            // setup Form Data
            ViperFormData vpdata = new ViperFormData(doc);
            vpdata.Rpipetype = pipeType;
            vpdata.level = level.Id;
            vpdata.pipeSystem = sys;
            vpdata.diameter = 2 / 12 ;
            string headname = "Dry Pendent";
            vpdata.downHead = VpObjectFinders.FindFamilyTypes(doc, BuiltInCategory.OST_Sprinklers, headname).First();
            vpdata.upHead = VpObjectFinders.FindFamilyTypes(doc, BuiltInCategory.OST_Sprinklers, headname).First();
            vpdata.vertHead = VpObjectFinders.FindFamilyTypes(doc, BuiltInCategory.OST_Sprinklers, headname).First();

            // EXECUTE TEST 1
            Makepipes.MakeOrdered(doc, vpdata, ssd);

            //test 2 family placement
            var edgelist2 = new[]
            {
                new [] { 5.0, 5.0, 1.0, 5.0, 5.0, 10.0 },       // 0 - 0, 1 
            };

            var symlist = new[]
            {
                new [] { (int)HeadType.Down, 0, 0 },
                new [] { (int)HeadType.UP, 0, 1 }
            };

            ssd.geom = wraplist<double>(edgelist2);
            ssd.indicies = new List<List<int>>();
            ssd.symbols = wraplist<int>(symlist);

            // EXECUTE TEST 1
            Makepipes.MakeOrdered(doc, vpdata, ssd);

            return Result.Succeeded;
        }

        private static List<List<T>> wraplist<T>(T[][] items)
        {
            var olist = new List<List<T>>();
            for(int i = 0; i < items.Length; i++)
            {
                var list = items[i].ToList();
                olist.Add(list);
            }
            return olist;
        }

    }


    [Transaction(TransactionMode.Manual)]
    public class ParametertrizeBOP : IExternalCommand
    {
        private static RApplication m_application;
        private static UIDocument uidoc;
        private static Document doc;

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            StringBuilder sb = new StringBuilder();
            VpObjectFinders vfo = new VpObjectFinders();
            ViperUtils vputil = new ViperUtils();
            VPipeEndFormData vpdata = new VPipeEndFormData(doc);
            PipeEnds vpform = new PipeEnds(vpdata);
            Makepipes mp = new Makepipes();

            FilteredElementCollector gFilter = new FilteredElementCollector(doc);
            FilteredElementCollector finalCollector = new FilteredElementCollector(doc);
            var categories = new List<BuiltInCategory>() {
                BuiltInCategory.OST_PipeFitting, BuiltInCategory.OST_PipeFittingInsulation };
            finalCollector.WherePasses
                (new LogicalOrFilter
                (new List<ElementFilter>{
                new ElementClassFilter(typeof(Pipe)),
                new ElementClassFilter(typeof(Duct)),
                new ElementCategoryFilter(BuiltInCategory.OST_PipeFitting),
                new ElementCategoryFilter(BuiltInCategory.OST_PipeFittingInsulation),
                new ElementCategoryFilter(BuiltInCategory.OST_PipeInsulations),

                new ElementCategoryFilter(BuiltInCategory.OST_Conduit),
                new ElementCategoryFilter(BuiltInCategory.OST_ConduitFitting),

                new ElementCategoryFilter(BuiltInCategory.OST_DuctCurves),
                new ElementCategoryFilter(BuiltInCategory.OST_DuctFitting),
                new ElementCategoryFilter(BuiltInCategory.OST_DuctFittingInsulation),
                new ElementCategoryFilter(BuiltInCategory.OST_DuctCurvesInsulation)
            }));

            IList<Element> z = finalCollector.ToElements();

            Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran.Start();

            foreach (Element e in z)
            {
                try
                {
                    double bot1 = 0;
                    double bot2 = 0;
                    double radius = 0;
                    double pb1 = 0;
                    double pb2 = 0;
                    double addinsul = 0;

                    if (e.Category.Name == "Pipes")
                    {
                        Pipe pi = e as Pipe;
                        radius = e.LookupParameter("Diameter").AsDouble() / 2;
                        bot1 = e.LookupParameter("Start Offset").AsDouble();
                        bot2 = e.LookupParameter("End Offset").AsDouble();

                        pb1 = bot1 - radius;
                        pb2 = bot2 - radius;
                        e.LookupParameter("BOT1").Set(pb1);
                        e.LookupParameter("BOT2").Set(pb2);

                    }
                    else if (e.Category.Name == "Pipe Fittings")
                    {
                        radius = e.LookupParameter("Nominal Radius").AsDouble();
                        bot1 = e.LookupParameter("Offset").AsDouble();
                        pb1 = bot1 - radius - addinsul;

                        e.LookupParameter("BOT1").Set(pb1);

                    }
                    else if (e.Category.Name == "Pipe Insulation")
                    {
                        PipeInsulation insul = e as PipeInsulation;
                        ElementId eid = insul.HostElementId;
                        Pipe pi = doc.GetElement(eid) as Pipe;

                        radius = pi.LookupParameter("Diameter").AsDouble() / 2;
                        addinsul = insul.Thickness;

                        if (pi.LookupParameter("Start Offset") != null)
                        {
                            bot1 = pi.LookupParameter("Start Offset").AsDouble();
                            bot2 = pi.LookupParameter("End Offset").AsDouble();
                            pb1 = bot1 - radius - addinsul;
                            pb2 = bot2 - radius - addinsul;
                            insul.LookupParameter("BOT1").Set(pb1);
                            insul.LookupParameter("BOT2").Set(pb2);
                        }
                        else
                        {
                            bot1 = e.LookupParameter("Offset").AsDouble();
                            pb1 = bot1 - radius - addinsul;
                            insul.LookupParameter("BOT1").Set(pb1);
                        }
                    }
                    else if (e.Category.Name == "Ducts")
                    {
                        Duct pi = e as Duct;
                        bot1 = e.LookupParameter("Start Offset").AsDouble();
                        bot2 = e.LookupParameter("End Offset").AsDouble();
                        if (e.LookupParameter("Diameter") != null)
                        {
                            radius = e.LookupParameter("Diameter").AsDouble() / 2;
                        }
                        else
                        {
                            radius = e.LookupParameter("Height").AsDouble() / 2;
                        }
                        pb1 = bot1 - radius;
                        pb2 = bot2 - radius;
                        e.LookupParameter("BOT1").Set(pb1);
                        e.LookupParameter("BOT2").Set(pb1);
                    }

                    else if (e.Category.Name == "Duct Fittings")
                    {

                        bot1 = e.LookupParameter("Offset").AsDouble();
                        if (e.LookupParameter("Duct Radius") != null)
                        {
                            radius = e.LookupParameter("Duct Radius").AsDouble();
                        }
                        else
                        {
                            radius = e.LookupParameter("Duct Height").AsDouble() / 2;
                        }
                        pb1 = bot1 - radius;
                        e.LookupParameter("BOT1").Set(pb1);
                    }

                    else if (e.Category.Name == "Duct Insulations")
                    {
                        DuctInsulation insul = e as DuctInsulation;
                        ElementId eid = insul.HostElementId;

                        Duct pi = doc.GetElement(eid) as Duct;

                        if (e.LookupParameter("Duct Radius") != null)
                        {
                            radius = pi.LookupParameter("Duct Radius").AsDouble() / 2;
                        }
                        else
                        {
                            radius = pi.LookupParameter("Duct Height").AsDouble() / 2;
                        }
                        //radius = pi.LookupParameter("Diameter").AsDouble() / 2;

                        bot1 = pi.LookupParameter("Start Offset").AsDouble();
                        bot2 = pi.LookupParameter("End Offset").AsDouble();
                        addinsul = insul.LookupParameter("Insulation Thickness").AsDouble();
                        pb1 = bot1 - radius - addinsul;
                        pb2 = bot2 - radius - addinsul;

                        insul.LookupParameter("BOT1").Set(pb1);
                        insul.LookupParameter("BOT2").Set(pb2);
                    }


                    else { }

                }
                catch (Exception) { continue; }

            }

            tran.Commit();

            return Result.Succeeded;
        }

    }




}
