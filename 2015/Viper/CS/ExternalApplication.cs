//
// (C) Copyright 2003-2013 by Autodesk, Inc.
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE. AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
// 


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

using GXYZ = Autodesk.Revit.DB.XYZ;
using Microsoft.Office.Interop.Excel;
using Autodesk.Revit.UI.Selection;



using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using RApplication = Autodesk.Revit.ApplicationServices.Application;
using Autodesk.Revit.DB.Structure;
using Revit.SDK.Samples.UIAPI.CS.Starwood;

namespace Revit.SDK.Samples.UIAPI.CS 
{
   public class ExternalApp : IExternalApplication
    {


        static String addinAssmeblyPath = typeof(ExternalApp).Assembly.Location;

        /// <summary>
        /// Loads the default Mass template automatically rather than showing UI.
        /// </summary>
        /// <param name="application">An object that is passed to the external application 
        /// which contains the controlled application.</param>
        void createCommandBinding(UIControlledApplication application)
        {
           RevitCommandId wallCreate = RevitCommandId.LookupCommandId("ID_NEW_REVIT_DESIGN_MODEL");
           AddInCommandBinding binding = application.CreateAddInCommandBinding(wallCreate);
           binding.Executed += new EventHandler<Autodesk.Revit.UI.Events.ExecutedEventArgs>(binding_Executed);
           binding.CanExecute += new EventHandler<Autodesk.Revit.UI.Events.CanExecuteEventArgs>(binding_CanExecute);
        }


        BitmapSource convertFromBitmap(System.Drawing.Bitmap bitmap)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                bitmap.GetHbitmap(),IntPtr.Zero,  Int32Rect.Empty,  BitmapSizeOptions.FromEmptyOptions());
        }

        void createRibbonButton(UIControlledApplication application)
        {
            application.CreateRibbonTab("Viper");
            RibbonPanel rp = application.CreateRibbonPanel("Viper", "Sleeves");
            RibbonPanel pipes = application.CreateRibbonPanel("Viper", "Pipe Tools");
            RibbonPanel apm = application.CreateRibbonPanel("Viper", "APM Tools");
            RibbonPanel dev = application.CreateRibbonPanel("Viper", "In development");

            #region Pipes
            PushButtonData pbd = new PushButtonData("Wall", "SelectBySize",
                    addinAssmeblyPath, "Revit.SDK.Samples.UIAPI.CS.SelectPipesBySize");
            ContextualHelp ch = new ContextualHelp(ContextualHelpType.ContextId, "HID_OBJECTS_WALL");
            pbd.SetContextualHelp(ch);
            pbd.LongDescription = "We redirect the wiki help for this button to Wall creation.";
            pbd.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_Select_by_Size);
            PushButton pb = pipes.AddItem(pbd) as PushButton;


            PushButtonData pbdsl = new PushButtonData("SelectBySlope", "SelectBySlope",
                  addinAssmeblyPath, "Revit.SDK.Samples.UIAPI.CS.SelectPipesBySlope");
            pbdsl.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_2D_Pipe_Granade);
            PushButton pbsl = pipes.AddItem(pbdsl) as PushButton;


            PushButtonData pbd1 = new PushButtonData("viperAll", "viperAll",
                    addinAssmeblyPath,  "Revit.SDK.Samples.UIAPI.CS.ViperGrenade");
            pbd1.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_2D_Pipe_Granade);
            PushButton pb1 = pipes.AddItem(pbd1) as PushButton;

            PushButtonData pbd3 = new PushButtonData("Pipe Edit", "Edit Pipes",
               addinAssmeblyPath, "Revit.SDK.Samples.UIAPI.CS.PipeRunEdit");
            pbd3.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_Extend_Pipe);
            PushButton pb3 = pipes.AddItem(pbd3) as PushButton;
            #endregion

            #region Sleeves
            PushButtonData pbd4 = new PushButtonData("@Floor", "Sleeve-Floor",
                addinAssmeblyPath, "Revit.SDK.Samples.UIAPI.CS.MakePenetrations");
            pbd4.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_Create_Floor_Sleeves);
            PushButton pb4 = rp.AddItem(pbd4) as PushButton;

            PushButtonData pbd5 = new PushButtonData("@Wall", "Sleeve-Wall",
             addinAssmeblyPath, "Revit.SDK.Samples.UIAPI.CS.MakePenetrations");
            pbd5.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_Create_Wall_Sleeves);
            PushButton pb5 = rp.AddItem(pbd5) as PushButton;

            PushButtonData pbd2 = new PushButtonData("Layout", "Layout",
           addinAssmeblyPath,  "Revit.SDK.Samples.UIAPI.CS.LocatePenetrations");
            pbd2.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_Update_Sleeve_Tag);
            PushButton pb2 = rp.AddItem(pbd2) as PushButton;

            PushButtonData slv2 = new PushButtonData("LayoutPolar", "LayoutPolar",
            addinAssmeblyPath,  "Revit.SDK.Samples.UIAPI.CS.LocatePenetrationsPolar");
            slv2.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_Update_Sleeve_Tag);
            PushButton bslv2 = rp.AddItem(slv2) as PushButton;
            #endregion

            //LocatePenetrationsPolar

            #region APM



            #endregion

            #region INDEV
            //second is visible
            PushButtonData v1 = new PushButtonData("Classic Import", "Classic Import",
            addinAssmeblyPath, "Revit.SDK.Samples.UIAPI.CS.ExceltoRevitBMC");
            v1.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_Viper_Classic);
            PushButton bv1 = dev.AddItem(v1) as PushButton;

            PushButtonData v2 = new PushButtonData("Readblock", "Readblock",
           addinAssmeblyPath, "Revit.SDK.Samples.UIAPI.CS.Viperblock");
            v2.LargeImage = convertFromBitmap(Revit.SDK.Samples.UIAPI.CS.Properties.Resources.viper_Block_Map);
            PushButton bv2 = dev.AddItem(v2) as PushButton;




            #endregion



        }


        /// <summary>
        /// Implement this method to implement the external application which should be called when 
        /// Revit is about to exit,Any documents must have been closed before this method is called.
        /// </summary>
        /// <param name="application">An object that is passed to the external application 
        /// which contains the controlled application.</param>
        /// <returns>Return the status of the external application.
        /// A result of Succeeded means that the external application successfully shutdown. 
        /// Cancelled can be used to signify that the user cancelled the external operation at 
        /// some point.                                                                        
        /// If false is returned then the Revit user should be warned of the failure of the external 
        /// application to shut down correctly.</returns> 
        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        /// Implement this method to implement the external application which should be called when 
        /// Revit starts before a file or default template is actually loaded.
        /// </summary>
        /// <param name="application">An object that is passed to the external application
        /// which contains the controlled application.</param>
        /// <returns>Return the status of the external application.
        /// A result of Succeeded means that the external application successfully started.
        /// Cancelled can be used to signify that the user cancelled the external operation at
        /// some point.
        /// If false is returned then Revit should inform the user that the external application
        /// failed to load and the release the internal reference.</returns> 
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

        public UIControlledApplication UIControlledApplication
        {
            get { return s_uiApplication; }
        }

        private UIControlledApplication s_uiApplication;

        void binding_CanExecute(object sender, Autodesk.Revit.UI.Events.CanExecuteEventArgs e)
        {
            e.CanExecute = true;
        }

        void binding_Executed(object sender, Autodesk.Revit.UI.Events.ExecutedEventArgs e)
        {
           UIApplication uiApp = sender as UIApplication;
           if (uiApp == null)
              return;

           String famTemplatePath = uiApp.Application.FamilyTemplatePath;
           String conceptualmassTemplatePath = famTemplatePath + @"\Conceptual Mass\Mass.rft";
           if (System.IO.File.Exists(conceptualmassTemplatePath))
           {
              //uiApp.OpenAndActivateDocument(conceptualmassTemplatePath);
              Document familyDocument = uiApp.Application.NewFamilyDocument(conceptualmassTemplatePath);
              if (null == familyDocument)
              {
                 throw new Exception("Cannot open family document");
              }

              String fileName = Guid.NewGuid().ToString() + ".rfa";
              familyDocument.SaveAs(fileName);
              familyDocument.Close();

              uiApp.OpenAndActivateDocument(fileName);

              FilteredElementCollector collector = new FilteredElementCollector(uiApp.ActiveUIDocument.Document);
              collector = collector.OfClass(typeof(View3D));

              var query = from element in collector

                          where element.Name == "{3D}"

                          select element; // Linq query  

              List<Autodesk.Revit.DB.Element> views = query.ToList<Autodesk.Revit.DB.Element>();

              View3D view3D = views[0] as View3D;
              if(view3D != null)
               uiApp.ActiveUIDocument.ActiveView = view3D;



           }
        }
    }

   /// <summary>
   /// Implement this method as an external command for Revit.
   /// </summary>
   /// <param name="commandData">An object that is passed to the external application
   /// which contains data related to the command,
   /// such as the application object and active view.</param>
   /// <param name="message">A message that can be set by the external application
   /// which will be displayed if a failure or cancellation is returned by
   /// the external command.</param>
   /// <param name="elements">A set of elements to which the external application
   /// can add elements that are to be highlighted in case of failure or cancellation.</param>
   /// <returns>Return the status of the external command.
   /// A result of Succeeded means that the API external method functioned as expected.
   /// Cancelled can be used to signify that the user cancelled the external operation 
   /// at some point. Failure should be returned if the application is unable to proceed with
   /// the operation.</returns>
   /// 
   



   #region DONE

   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class SelectPipesBySize : IExternalCommand
   {

       private static Autodesk.Revit.ApplicationServices.Application m_application;
       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;

       // Selects all instances of a pipe with a given size (Alan request - DONE)
       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {
         

           m_application = commandData.Application.Application;
           uidoc = commandData.Application.ActiveUIDocument;
           doc = commandData.Application.ActiveUIDocument.Document;
           Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;

            

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
                            select elem;

           //add all to selection set (---- better way?)
           foreach (Pipe p in pipesinter)
           { 
               sel.Elements.Add(p);
           }

           return Result.Succeeded;
       }
   }

       [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
     public class SelectPipesBySlope : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;
        // Selects all instances of a pipe with a given size (Val request E)
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;

            //prompt a selection
            Reference ref1 = sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "please pick a pipe");
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
            Autodesk.Revit.DB.Line pc = lc.Curve as Autodesk.Revit.DB.Line;


            //slope is 0 : pipe is horizantal
            if (lc.Curve.GetEndPoint(0).Z == lc.Curve.GetEndPoint(1).Z)
            {
                var pipesinter = from elem in fl
                                 where (elem.Location as LocationCurve).Curve.GetEndPoint(0).Z
                                 == (elem.Location as LocationCurve).Curve.GetEndPoint(1).Z
                                 where (elem as Pipe).PipeType.Name == (pp as Pipe).PipeType.Name
                                 select elem;

                foreach (Pipe p in pipesinter)
                { sel.Elements.Add(p);    }

            }

            // If pipe is vertical
            else if(lc.Curve.GetEndPoint(0).X == lc.Curve.GetEndPoint(1).X && 
                lc.Curve.GetEndPoint(0).Y == lc.Curve.GetEndPoint(1).Y)
            {
                var pipesinter = from elem in fl
                                 where (elem.Location as LocationCurve).Curve.GetEndPoint(0).X
                                 == (elem.Location as LocationCurve).Curve.GetEndPoint(1).X
                                 && (elem.Location as LocationCurve).Curve.GetEndPoint(0).Y
                                 == (elem.Location as LocationCurve).Curve.GetEndPoint(1).Y
                                 where (elem as Pipe).PipeType.Name == (pp as Pipe).PipeType.Name
                                 select elem;

                foreach (Pipe p in pipesinter)
                { sel.Elements.Add(p); }

            }
            else 
            {
             // calculate slope

            var pipesinter = from elem in fl
                             where elem.get_Parameter(BuiltInParameter.RBS_PIPE_SLOPE).AsValueString()
                             == pp.get_Parameter(BuiltInParameter.RBS_PIPE_SLOPE).AsValueString()
                             where (elem as Pipe).PipeType.Name == (pp as Pipe).PipeType.Name
                             select elem;
            foreach (Pipe p in pipesinter)
            { sel.Elements.Add(p); }
            }
        
            //add all to selection set (---- better way?)
            return Result.Succeeded;
        }

    }

   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class RemoveShared : IExternalCommand
   {

       private static Autodesk.Revit.ApplicationServices.Application m_application;
       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;


       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {

           m_application = commandData.Application.Application;
           uidoc = commandData.Application.ActiveUIDocument;
           doc = commandData.Application.ActiveUIDocument.Document;
           Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;

           //prompt a selection
           Reference ref1 = sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "please pick a pipe");
           Element wall = null;
           Wall e = doc.GetElement(ref1) as Wall;


           List<string> ToDelete = new List<string>()
                {
                    //"01 Issue ID",
                    //"02 Decription",
                    //"03 Location",

                    //"DD01 Issue",
                    //"DD02 RFI",
                    //"DD03 Status",
                    //"DD04 Closed Date",
                    //"DD05 RFI Number",
                    //"DD06 RFI Date",

                    //SUBMITTALS
                    "SL01 Submittal Number",

                    "SL02 Reviewed By",
                    "SL03 Item Description",
                    "SL04 Submittal Status",
                    "SL05 Approval Time (Days)",
                    "SL06 Recieved from Sub",
                    "SL07 To Arch/ Eng",
                    "SL08 From Arch/Eng",
                    "SL09 To Subcontractor",
                    "SL10 Recieved From Sub",

                    //"01 Spec Number",
                    //"02 Spec Title",
                    //"03 Subcontractor Name",

                    //"ME01 Approved Submittal to Sub",
                    //"ME02 Lead Time (days)",
                    //"ME03 Date Required on Job",
                    //"ME04 Promised Delivery Date",
                    //"ME05 Delivery Date",
                    //"ME06 Conversation 1",
                    //"ME07 Conversation 2",
                    //"ME08 Conversation",
                    //"ME09 Status",

                    //02 LEED SUBMITTALS
                    "01 Submittal Number",
               //     "02 Possible LEED Points",
                //    "03 Reviewed By",
                   // "04 LEED Submittal Description",
                    //"05 Submittal Status",
                    //"06 Approval Time",
                    //"07 Recieved from Sub Data",
                    //"08 To Arch/Eng",
                    //"09 From Arch/Eng",
                    //"10 To Subcontractor Date"

                };

           Transaction tran = new Transaction(doc, "writeparam");
           tran.Start();

           if (wall != null)
           {
               foreach (string s in ToDelete)
               {
                   Autodesk.Revit.DB.Parameter par = wall.get_Parameter(
                     s);

                   Definition def = par.Definition;

                   doc.ParameterBindings.Remove(def);
               }
           }

           tran.Commit();
           return Result.Succeeded;
       }





   }

   // Select a Pipe by its size
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class MakePenetrations : IExternalCommand
   {

       private static Autodesk.Revit.ApplicationServices.Application m_application;
       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;


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

           if (pn.ShowDialog() == System.Windows.Forms.DialogResult.OK)
           {
               // TaskDialog.Show("ASD", pendata.filename);
               pens.Sleeves_by_LinkedClash(app, pendata.filename, doc);
               tran.Commit();
           }

           return Result.Succeeded;
       }
   }


   // Locate penetrations by XY coordinate to a line
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class LocatePenetrations : IExternalCommand
   {

       private static Autodesk.Revit.ApplicationServices.Application m_application;
       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;


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

           // collect boundary lines
           // and sort into X and Y lists
           List<Element> nlx = GetGenericfams(doc, "TBC_3D ControlLine_X");
           List<Element> nly = GetGenericfams(doc, "TBC_3D ControlLine_Y");

           if (nlx.Count == 0)
           { TaskDialog.Show("error", " no gridlines are found - looking for 'TBC_3D ControlLine_X'"); }
           if (nly.Count == 0)
           { TaskDialog.Show("error", " no gridlines are found - looking for 'TBC_3D ControlLine_Y'"); }

           //get the sleeves
           List<Element> sleeves = GetGenericfams(doc, "TBC_Round Sleeve - Floor or Wall");
           List<Element> sleevesd = GetGenericfams(doc, "TBC_Square Sleeve - Floor or Wall");

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
               string ptlevel = ptlevelname(pt, doc);
             //  Element linenear;


               //test for sleevedir and level - as seperate funcitons
               foreach (Element line in lines)
               {
                   LocationCurve lineloc = line.Location as LocationCurve;
                   XYZ ptzright = new GXYZ(pt.X, pt.Y, lineloc.Curve.GetEndPoint(0).Z);
                   string linelev = ptlevelname(lineloc.Curve.GetEndPoint(0), doc);
                   double dl = lineloc.Curve.Distance(ptzright);


                   if (mindistance > dl && ptlevel == linelev)
                   {
                       mindistance = dl;
                       controlline = line.get_Parameter("Grid").AsString();
                       count++;
                       //  sb.AppendLine("point location  " + pt.ToString() + "  - Levels area " + ptlevel + " " + linelev);
                       //  sb.AppendLine(" Endpoints are " + lineloc.Curve.GetEndPoint(0) + " " +
                       //    lineloc.Curve.GetEndPoint(1) + " - " + dl.ToString());
                   }
               }
               if (mindistance != 1000) //&& st != "lol")
               {
                   double rounded = Math.Round(mindistance, 2);
                   //string st = linenear.get_Parameter("") + "-" + rounded.ToString();
                   sleeve.get_Parameter(paramtofill2).Set(controlline);
                   sleeve.get_Parameter(paramtofill1).Set(rounded);
                   //sleeve.s
               }
           }
       }


       private string ptlevelname(XYZ pt, Document doc)
       {
           List<Level> levels = new FilteredElementCollector(doc)
            .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();

           string levout = "null";
           double dl = 1000000;

           foreach (Level lev in levels)
           {

               if (Math.Abs(lev.Elevation - pt.Z) <= dl)
               {
                   dl = Math.Abs(lev.Elevation - pt.Z);
                   levout = lev.Name;
               }

           }
           return levout;
       }

       //Get list of all centerline objects. 
       // and sort into the X and Y types of grid
       public List<Element> GetGenericfams(Document doc, string name)
       {
           FilteredElementCollector gFilter = new FilteredElementCollector(doc);
           ICollection<Element> z = gFilter
               .OfClass(typeof(FamilyInstance))
               .OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
           List<Element> nl = new List<Element>();

           foreach (Element e in z)
           {
               Element elemtype = doc.GetElement(e.GetTypeId());
               if (elemtype.Name == name)
               {
                   nl.Add(e);
               }
           }
           return nl;
       }




   }


   // Locate penetrations by XY coordinate to a line
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class LocateTagGeneral : IExternalCommand
   {
       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {
           Autodesk.Revit.ApplicationServices.Application m_application = commandData.Application.Application;
           UIDocument uidoc = commandData.Application.ActiveUIDocument;
           Document doc = commandData.Application.ActiveUIDocument.Document;
           UIApplication app = commandData.Application;

           Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Penetrate");
           tran.Start();
           VpObjectFinders vpof = new VpObjectFinders();
           VpTesting vpt = new VpTesting();
           
           //// collect boundary lines and sort into X and Y lists
           List<Element> nlx = vpof.GetGenericfams(doc, "TBC_3D ControlLine_X");
           List<Element> nly = vpof.GetGenericfams(doc, "TBC_3D ControlLine_Y");

           if (nlx.Count == 0)
           { TaskDialog.Show("error", " no gridlines are found - looking for 'TBC_3D ControlLine_X'"); }
           if (nly.Count == 0)
           { TaskDialog.Show("error", " no gridlines are found - looking for 'TBC_3D ControlLine_Y'"); }

           ////get the sleeves
           FilteredElementCollector fg = new FilteredElementCollector(doc);
           List<Element> nl  = fg.OfCategory(BuiltInCategory.OST_PipeFitting)
               .OfClass(typeof(FamilyInstance)).ToElements().ToList();

           foreach (Element pf in nl)
           {
               LocationPoint pt = pf.Location as LocationPoint;

               vpt.LineDistanceToPoint(pf, pt.Point, doc, nlx, "Distance X", "Control Line X");
               vpt.LineDistanceToPoint(pf, pt.Point, doc, nly, "Distance Y", "Control Line Y");

               vpt.LineDistanceToPoint(pf, pt.Point, doc, nlx, "Distance X", "Control Line X");
               vpt.LineDistanceToPoint(pf, pt.Point, doc, nly, "Distance Y", "Control Line Y");
           }
           tran.Commit();
           return Result.Succeeded;
       }
   }


   // Locate penetrations by distance and angle
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class LocatePenetrationsPolar : IExternalCommand
   {

       private static Autodesk.Revit.ApplicationServices.Application m_application;
       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;


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

           // find boundary point
           List<Element> nlx = GetGenericfams(doc, "TBC_3D ControlPoint");
         //  List<Element> nly = GetGenericfams(doc, "TBC_3D ControlLine_Y");

           if (nlx.Count == 0)
           { TaskDialog.Show("error", " no gridlines are found - looking for 'TBC_3D ControlLine_X'"); }
        
           //get the sleeves
           List<Element> sleeves = GetGenericfams(doc, "TBC_Round Sleeve - Floor or Wall");
         //  List<Element> sleevesd = GetGenericfams(doc, "TBC_Square Sleeve - Floor or Wall");

           //locate realationship of sleeves to line by level
           Locatepointtopoint(sleeves, nlx, "Distance X", "Control Line Y");
      
           tran.Commit();

           return Result.Succeeded;
       }



       public void Locatepointtopoint(List<Element> sleeves, List<Element> lines, string paramtofill1, string paramtofill2)
       {
           StringBuilder sb = new StringBuilder();
           int count = 0;

           foreach (Element sleeve in sleeves)
           {
               LocationPoint sleeveloc = sleeve.Location as LocationPoint;
               LocationPoint Controlpoint = lines.ElementAt(0).Location as LocationPoint;

               XYZ pt = sleeveloc.Point;
               sb.AppendLine();
            //   double mindistance = 1000;
             //  string controlline = " ";
               string ptlevel = ptlevelname(pt, doc);
               //  Element linenear;

               double Distance = Controlpoint.Point.DistanceTo(sleeveloc.Point);

               //normalize the two points
               XYZ Controlpoint2 = new XYZ (Controlpoint.Point.X + 1, Controlpoint.Point.Y, Controlpoint.Point.Z);
               Autodesk.Revit.DB.Line baseline = Autodesk.Revit.DB.Line.CreateBound(Controlpoint.Point, Controlpoint2);

               Autodesk.Revit.DB.Line dline = Autodesk.Revit.DB.Line.CreateBound(Controlpoint.Point, sleeveloc.Point);

               double angle = XYZ.BasisY.AngleTo(dline.Direction);
               double angleDegrees = angle * 180 / Math.PI;

               if (sleeveloc.Point.X < Controlpoint.Point.X)
                   angle = 2 * Math.PI - angle;

               double angleDegreesCorrected = angle * 180 / Math.PI;
               sleeve.get_Parameter(paramtofill2).Set(angleDegreesCorrected.ToString());
               sleeve.get_Parameter(paramtofill1).Set(Distance);
           }
       }


       private string ptlevelname(XYZ pt, Document doc)
       {
           List<Level> levels = new FilteredElementCollector(doc)
            .OfClass(typeof(Level)).Cast<Level>().OrderBy(l => l.Elevation).ToList();

           string levout = "null";
           double dl = 1000000;

           foreach (Level lev in levels)
           {

               if (Math.Abs(lev.Elevation - pt.Z) <= dl)
               {
                   dl = Math.Abs(lev.Elevation - pt.Z);
                   levout = lev.Name;
               }

           }
           return levout;
       }

       //Get list of all centerline objects. 
       // and sort into the X and Y types of grid
       public List<Element> GetGenericfams(Document doc, string name)
       {
           FilteredElementCollector gFilter = new FilteredElementCollector(doc);
           ICollection<Element> z = gFilter
               .OfClass(typeof(FamilyInstance))
               .OfCategory(BuiltInCategory.OST_GenericModel).ToElements();
           List<Element> nl = new List<Element>();

           foreach (Element e in z)
           {
               Element elemtype = doc.GetElement(e.GetTypeId());
               if (elemtype.Name == name)
               {
                   nl.Add(e);
               }
           }
           return nl;
       }




   }

   // Push Guids to shared parameters
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class GuidPush : IExternalCommand
   {

       private static Autodesk.Revit.ApplicationServices.Application m_application;
       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;

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

           //  ElementIsElementTypeFilter filter1 = new ElementIsElementTypeFilter(false);
           //   FilteredElementCollector collector = new FilteredElementCollector(doc);
           //  collector.WherePasses(filter1).ToElements();
           //  Eleme


           // foreach (Element elem in collector)
           foreach (Reference r in pickedRef)
           {
               try
               {

                   Element elem = doc.GetElement(r);

                   elem.get_Parameter("GUID Inst").Set(elem.UniqueId.ToString());
                   elem.get_Parameter("Element ID Inst").Set(elem.Id.ToString());

                   Type tp = elem.GetType();

                   if (elem is FamilyInstance)
                   {
                       FamilyInstance fi = elem as FamilyInstance;
                       FamilySymbol fs = fi.Symbol;
                       fs.get_Parameter("Element ID Type").Set(fs.Id.ToString());
                       fs.get_Parameter("GUID Type").Set(fs.UniqueId.ToString());
                   }
                   else if (elem is Wall)
                   {
                       Wall wall = elem as Wall;
                       WallType wk = wall.WallType;
                       wk.get_Parameter("Element ID Type").Set(wk.Id.ToString());
                       wk.get_Parameter("GUID Type").Set(wk.UniqueId.ToString());
                   }

                   else if (elem is Autodesk.Revit.DB.Floor)
                   {
                       Autodesk.Revit.DB.Floor fi = elem as Autodesk.Revit.DB.Floor;
                       FloorType fs = fi.FloorType;
                       fs.get_Parameter("Element ID Type").Set(fs.Id.ToString());
                       fs.get_Parameter("GUID Type").Set(fs.UniqueId.ToString());
                   }
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
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class PipeRunEdit : IExternalCommand
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

           // USER INPUTS
           Autodesk.Revit.UI.Selection.Selection selall = uidoc.Selection;
           IList<Reference> sheeps = selall.PickObjects(ObjectType.Element, "Select target");

           // Conversions/ class references.
           Element elem = doc.GetElement(sheeps.ElementAt(0));

           MEPCurve leadpipe = elem as MEPCurve;
           LocationCurve lc = leadpipe.Location as LocationCurve;
           XYZ base1 = lc.Curve.GetEndPoint(1);
           FamilySymbol fs = vputil.GetGenericfams(doc, "Fakeline");

           // set sketchplane
           Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
           tran.Start();
               Plane plane = new Plane(uidoc.ActiveView.ViewDirection, base1);
               SketchPlane sp = SketchPlane.Create(doc, plane);
               uidoc.ActiveView.SketchPlane = sp;
              uidoc.ActiveView.HideActiveWorkPlane();
           tran.Commit();

           // place fake family which is the 'drag' command
           uidoc.PromptForFamilyInstancePlacement(fs);
           FamilyInstance inst = vputil.GetGenericinst(doc, "Fakeline");

           //retrieve the endpoint which is the final point of the chosen pipe
           Curve cc = (inst.Location as LocationCurve).Curve;
           XYZ userpoint = cc.GetEndPoint(1);

           //Get the nearest pipe to the start point of the fakeline
           Reference mainpipe = vputil.nearestref(doc, sheeps, cc as Autodesk.Revit.DB.Line);
           Element melem = doc.GetElement(mainpipe);
           MEPCurve selpipe = melem as MEPCurve;
           LocationCurve mlc = selpipe.Location as LocationCurve;

           XYZ vector = vputil.pipevector(selpipe, userpoint);

           // rebuild pipe
           Transaction tran2 = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
           tran2.Start();
         
           foreach (Reference sheep in sheeps)
           {
               Element elem3 = doc.GetElement(sheep);
               MEPCurve pipe = elem3 as MEPCurve;
               XYZ oldpoint = vputil.nearestpipepoints(pipe, userpoint);
               XYZ basepoint = vputil.farthestpipepoints(pipe, userpoint);
               vputil.rebuildpipevector(doc, pipe, basepoint, oldpoint, vector);      
           }

           tran2.Commit();

           Transaction tran3 = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
           tran3.Start();
           doc.Delete(inst);
           tran3.Commit();

           return retRes;
       }

   }


   //Create Pipes External Command
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class PipeRunRack : IExternalCommand
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
           HelperMethods hp = new HelperMethods();
   
           Autodesk.Revit.UI.Selection.Selection selall = uidoc.Selection;
           IList<Reference> sheeps = selall.PickObjects(ObjectType.Element, "Select target");
           List<MEPCurve> meps = new List<MEPCurve>();

           foreach(Reference pp in sheeps)
           {
               Element em = doc.GetElement(pp);
               MEPCurve crv = em as MEPCurve;
               if(crv != null)
               {meps.Add(crv);}

           }

           Element elem = doc.GetElement(sheeps.ElementAt(0));
           SketchPlane sp = hp.SetSketchplance(doc, elem, uidoc);

           // place fake family which is the 'drag' command
           FamilySymbol fs = vputil.GetGenericfams(doc, "Fakeline");
           uidoc.PromptForFamilyInstancePlacement(fs);
           List<FamilyInstance> instances = vputil.GetGenericinsts(doc, "Fakeline");        
        
           List<Autodesk.Revit.DB.Line> linelist = new List<Autodesk.Revit.DB.Line>();
           foreach (FamilyInstance inst in instances)
           {
               LocationCurve cc = inst.Location as LocationCurve;
               linelist.Add(cc.Curve as Autodesk.Revit.DB.Line);
           }

          Reference rf= vputil.nearestref(doc, sheeps, linelist.ElementAt(0));
          Element mpipe = doc.GetElement(rf);
          MEPCurve linethis = mpipe as MEPCurve;


           Viper2d.RackUtils Rackutil = new Viper2d.RackUtils();
           List<twopoint> tpout = Rackutil.BisectingAngles(linelist, meps, linethis);
           Makepipes mp = new Makepipes();
 
           // rebuild pipe
           Transaction tran2 = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
           tran2.Start();
           StringBuilder sd = new StringBuilder();
           foreach (twopoint tp in tpout)
           {
               sd.AppendLine(tp.reportall());
          }
           System.Windows.MessageBox.Show(sd.ToString());

           List<twopoint> geolist = mp.MAKE_PIPES_general (tpout, doc);


           tran2.Commit();
          
           

          // mp.Connectsystemtree2(geolist, doc, base1);
           List<Connector> allcons = mp.allconnectors(geolist);
           mp.conectorsall(doc, allcons);

           //List<Connector> allcons2 = mp.allconnectors(geolist);
           //mp.conectorsall(doc, allcons2);

          Transaction tran3 = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
           tran3.Start();

           foreach (Reference pp in sheeps)
           {
               Element em = doc.GetElement(pp);
               ElementId eid = em.Id;
               doc.Delete(eid);

           }

           foreach (FamilyInstance inst in instances)
           {
           ElementId eid = inst.Id;
           doc.Delete(eid);
           }
           tpout.Clear();
           selall.Dispose();
           sheeps.Clear();
           tran3.Commit();

           return retRes;
       }







   }

   //Create Pipes External Command
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
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
           Autodesk.Revit.UI.Selection.Selection selall = uidoc.Selection;
           Reference p1 = selall.PickObject(ObjectType.Element, "Select target");

           Autodesk.Revit.UI.Selection.Selection selall2 = uidoc.Selection;
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
               // only two dimensions affected




           }

           // get the perpendicular curve to a pipe
           XYZ vecp1 = vputil.MEPCurvetoVector(pp1);
           XYZ perpvec = vputil.GetPerpVector(vecp1);
           
          //TaskDialog.Show

           //create the line
           Autodesk.Revit.DB.Line ll = Autodesk.Revit.DB.Line.CreateBound(midpoint - perpvec, midpoint+ perpvec);

           //create the line from a location curve
           Autodesk.Revit.DB.Line pl1 = Autodesk.Revit.DB.Line.CreateBound(pc1.Curve.GetEndPoint(0), pc1.Curve.GetEndPoint(1));
           Autodesk.Revit.DB.Line plz1 = Autodesk.Revit.DB.Line.CreateBound(pl1.GetEndPoint(0).Subtract(pl1.Direction.Multiply(1000))
               , pl1.GetEndPoint(1).Add(pl1.Direction.Multiply(1000)));

           Autodesk.Revit.DB.Line pl2 = Autodesk.Revit.DB.Line.CreateBound(pc2.Curve.GetEndPoint(0), pc2.Curve.GetEndPoint(1));
           Autodesk.Revit.DB.Line plz2 = Autodesk.Revit.DB.Line.CreateBound(pl2.GetEndPoint(0).Subtract(pl2.Direction.Multiply(1000))
               , pl2.GetEndPoint(1).Add(pl2.Direction.Multiply(1000)));

           //intersect the lines
           IntersectionResultArray intarray1 = new IntersectionResultArray();
           ll.Intersect(plz1, out intarray1);

           IntersectionResultArray intarray2 = new IntersectionResultArray();
           ll.Intersect(plz2, out intarray2);

          
           XYZ nbp1 = vputil.farthestpipepoints(pp1, intarray1.get_Item(0).XYZPoint);
           XYZ nbp2 = vputil.farthestpipepoints(pp2, intarray2.get_Item(0).XYZPoint);


           Autodesk.Revit.DB.FilteredElementCollector collector = new Autodesk.Revit.DB.FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));
            PipeType pipeType = collector.FirstElement() as PipeType;

            Transaction tran3 = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran3.Start();

            Pipe pipe = doc.Create.NewPipe(intarray1.get_Item(0).XYZPoint,
                intarray2.get_Item(0).XYZPoint, pipeType);

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


   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class TagAllPipeEnds : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;

 
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
    //            doc.Create.NewTag(doc.ActiveView, p
    //elem,
    //False,
    //TagMode.TM_ADDBY_CATEGORY,
    //TagOrientation.TAG_HORIZONTAL,
    //panelCenter);

            }


            return Result.Succeeded;
        }
    }


   //Create Pipes External Command
   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
   [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
   public class SolidtoPipes : IExternalCommand
   {

       private static Autodesk.Revit.ApplicationServices.Application m_application;
       private static UIDocument uidoc;
       private static Autodesk.Revit.DB.Document doc;
       private ViperFormData vpdata;

       //EXECUTE FUNCTION
       public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
       {
           Autodesk.Revit.UI.Result retRes = Autodesk.Revit.UI.Result.Succeeded;


           m_application = commandData.Application.Application;
           uidoc = commandData.Application.ActiveUIDocument;
           doc = commandData.Application.ActiveUIDocument.Document;
           ViperUtils vputil = new ViperUtils();

           Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;
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

           List<twopoint> geolist = new List<twopoint>();

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

   #region Excel

   [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ExceltoRevitBMC : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            //string Path;

  
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    TaskDialog.Show("ADS", openFileDialog1.FileName.ToString());
                    string s = openFileDialog1.FileName.ToString();
 
                    ExcelFunctions ef = new ExcelFunctions();
                    Worksheet sheet = ef.Excelget(s, "Sheet1");

                    Microsoft.Office.Interop.Excel.Range last = sheet.Cells.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell, Type.Missing);
                    Microsoft.Office.Interop.Excel.Range range = sheet.get_Range("A1", last);
                    int lastUsedRow = last.Row;
                    int lastUsedColumn = last.Column;

                  
                    StringBuilder sb = new StringBuilder();
                    List<twopoint> listpipes = new List<twopoint>();
                    List<twopoint> listducts = new List<twopoint>();

                    for (int i = 2; i <= lastUsedRow; i++)
                    {
                        sb.AppendLine();

                        try
                        {
                            //NEW TWOPOINT
                            twopoint tpn = new twopoint();

                            // GET REVIT TYPE
                            string idtype = ef.excelval(sheet, i, 3);
                            tpn.Revittypename = idtype;

                            // GET ENDPOITNS
                            string id1 = ef.excelval(sheet, i, 4);
                            List<double> pts = ef.stringtodoublelist(id1);
                            tpn.pt1 = new GXYZ(pts.ElementAt(0), pts.ElementAt(1), pts.ElementAt(2));
                            tpn.pt2 = new GXYZ(pts.ElementAt(3), pts.ElementAt(4), pts.ElementAt(5));

                            // GET THE END SIZE
                            string id3 = ef.excelval(sheet, i, 2);
                            ef.determintductorpipe(tpn, idtype, id3);

                            if (tpn.Revitcategory == "Duct")
                            {
                            listducts.Add(tpn);
                            }
                            else if (tpn.Revitcategory == "Pipe")
                            {
                             listpipes.Add(tpn);

                            }
                        }
                        catch (Exception) { }
                    }

 
                    Makepipes mp = new Makepipes();
                    List<twopoint> dtsz = mp.MAKE_Ducts(listducts, doc);
                    mp.MAKE_PIPES_simple(listpipes, doc);

                  // TaskDialog.Show("ADS", sb.ToString());
                   
      
                }
                catch (Exception )
                {
                   // MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            return Result.Succeeded;
        }


        private void writeparam( Microsoft.Office.Interop.Excel.Worksheet sheet, Element xElement, int start,int end , int i )
        {
            for(int j = start; j <= end; j++)
                {
                    // Retrieve the parameter
                    Microsoft.Office.Interop.Excel.Range param =
                    (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, j];
                    string paramval = param.Value.ToString();
                    
                    //retrieve the name of the parameter
                    Microsoft.Office.Interop.Excel.Range paramname =
                    (Microsoft.Office.Interop.Excel.Range)sheet.Cells[2, j];
                   
                    string exparamname = paramname.Value.ToString();
                    //Retrieve the parameter storage type anc convert
                    
                    //Set the parameter to the new value
                    xElement.get_Parameter(exparamname).Set(paramval);
                }

        }


    }


    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class RevittoExcel : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;


        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            //string Path;
            int n = 10;

            ExcelFunctions ef = new ExcelFunctions();
            Worksheet sheet = ef.Excelget("da", "sheet1");


            Transaction tran = new Transaction(doc, "writeparam");
            tran.Start();



            for (int i = 2; i <= n; i++)
            {
                try
                {
                    Microsoft.Office.Interop.Excel.Range rngtype =
                        (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, 1];

                    string idvalue = rngtype.Value.ToString();
                    int idinteger = Convert.ToInt32(idvalue);

                    ElementId elid = new ElementId(idinteger);
                    Element xElement = doc.GetElement(elid);

                    writeparam(sheet, xElement, 2, 3, i);
                }
                catch (Exception) { }

            }

            tran.Commit();
            //  myexcel.Quit();

            return Result.Succeeded;
        }


        private void writeparam(Microsoft.Office.Interop.Excel.Worksheet sheet, Element xElement, int start, int end, int i)
        {
            for (int j = start; j <= end; j++)
            {
                // Retrieve the parameter
                Microsoft.Office.Interop.Excel.Range param =
                (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, j];
                string paramval = param.Value.ToString();

                //retrieve the name of the parameter
                Microsoft.Office.Interop.Excel.Range paramname =
                (Microsoft.Office.Interop.Excel.Range)sheet.Cells[2, j];

                string exparamname = paramname.Value.ToString();
                //Retrieve the parameter storage type anc convert

                //Set the parameter to the new value
                xElement.get_Parameter(exparamname).Set(paramval);
            }



        }


    }

    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class JSQExceltoRevit : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            StringBuilder sd = new StringBuilder(); 

            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.InitialDirectory = "c:\\";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    TaskDialog.Show("ADS", openFileDialog1.FileName.ToString());
                    string s = openFileDialog1.FileName.ToString();

                    ExcelFunctions ef = new ExcelFunctions();
                    Worksheet sheet = ef.Excelget(s, "Sheet1");


                    StringBuilder sb = new StringBuilder();
                    int good = 0;
                    int bad = 0;

                    Microsoft.Office.Interop.Excel.Range last = sheet.Cells.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell, Type.Missing);
                    Microsoft.Office.Interop.Excel.Range range = sheet.get_Range("A1", last);

                    int lastUsedRow = last.Row;
                    int lastUsedColumn = last.Column;

                 //   TaskDialog.Show("ad", last.Row.ToString() + " " + last.Column.ToString());
             
                    int strt = 0;

                    for (int i = 1; i <= last.Column; i++)
                    {
                        Microsoft.Office.Interop.Excel.Range rngtype =
                                (Microsoft.Office.Interop.Excel.Range)sheet.Cells[2, i];
                        string idvalue = rngtype.Value.ToString();
                        if (idvalue == "Type")
                        {
                            strt = i;
                            break;
                        }

                    }

                    if (strt == 0)
                    { return Result.Failed; }

                    else
                    {
                        Transaction tran = new Transaction(doc, "writeparam");
                        tran.Start();


                        for (int i = 3; i <= last.Row; i++)
                        {
                            Element xElement = null;
                            try
                            {
                                //Microsoft.Office.Interop.Excel.Range rngtype =
                                //    (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, 2];
                                //string idvalue = rngtype.Value.ToString();

                                Microsoft.Office.Interop.Excel.Range guidrngtype =
                                    (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, 1];
                                string guid = guidrngtype.Value.ToString();

                                //int idinteger = Convert.ToInt32(idvalue);
                                //ElementId elid = new ElementId(idinteger);

                               // Element xElement = doc.GetElement(elid);
                                
                                 xElement = doc.GetElement(guid);
                                TaskDialog.Show("Element", xElement.Category.Name.ToString());

                                writeparam(sheet, xElement, strt + 1, last.Column, i);
                                good++;
                            }
                            catch (Exception) { sd.AppendLine("fail " + xElement.Category.Name); }

                        }

                        tran.Commit();
                    }
                    TaskDialog.Show("a", sd.ToString());
                   // myexcel.Quit();

                }
                catch (Exception) { }
            }

            return Result.Succeeded;
        }

        private void writeparam(Microsoft.Office.Interop.Excel.Worksheet sheet, Element xElement, int start, int end, int i)
        {
            for (int j = start; j <= end; j++)
            {
                // Retrieve the parameter from excel
                Microsoft.Office.Interop.Excel.Range param =
                (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, j];
                string paramval = param.Value.ToString();

                //retrieve the name of the parameter from excel
                Microsoft.Office.Interop.Excel.Range paramname =
                (Microsoft.Office.Interop.Excel.Range)sheet.Cells[2, j];

                
                string exparamname = paramname.Value.ToString();
                Autodesk.Revit.DB.Parameter para =
                    xElement.get_Parameter(exparamname);

                if (para == null)
                {
                    DefinitionFile df = m_application.OpenSharedParameterFile();
                    DefinitionGroups myGroups = df.Groups;
                    DefinitionGroup myGroup = myGroups.get_Item("test");
                    Definitions myDefinitions = myGroup.Definitions;

                    //ExternalDefinition myExtDef = myDefinitions.get_Item("MyParam") as ExternalDefinition;
                    Definition ndef = myDefinitions.Create(exparamname, ParameterType.Text, true);

                    CategorySet myCategories = m_application.Create.NewCategorySet();
                    //StringBuilder strBuilder = new StringBuilder();

                    Category myCategory = xElement.Category;
                    myCategories.Insert(myCategory);

                    //Create an object of TypeBinding according to the Categories
                    InstanceBinding typeBinding = m_application.Create.NewInstanceBinding(myCategories);


                    // Get the BingdingMap of current document.
                    BindingMap bindingMap = doc.ParameterBindings;

                    // Bind the definitions to the document
                   // bool typeBindOK =
                      bindingMap.Insert(ndef, typeBinding, BuiltInParameterGroup.PG_TEXT);

                }


                if (para.StorageType == StorageType.String)
                {
                    xElement.get_Parameter(exparamname).Set(paramval);

                }
                else if(para.StorageType == StorageType.Integer)
                {
                    int prm = Convert.ToInt32(paramval);
                    xElement.get_Parameter(exparamname).Set(prm);

                }
              
                
               
            }

        }


    }


    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class JSQExceltoRevittest : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            FilteredElementCollector col = new FilteredElementCollector(doc).OfClass(typeof(ViewSchedule));
            ViewScheduleExportOptions opt = new ViewScheduleExportOptions();
            //opt.
            string _export_folder_name = "C:\\Users\\psavine\\Desktop\\JSQ Push\\daa";
            // string _ext = ".xlsx";
            string _ext = ".txt";

            Microsoft.Office.Interop.Excel.Application myexcel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook mybook = myexcel.Workbooks.Open(
            @"C:\Users\psavine\Desktop\JSQ Push\W-01SP-Sub Procurement-Items.xlsx"
            , 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, " ", true, false, 0, true, true, false);

            Microsoft.Office.Interop.Excel.Sheets sheets = mybook.Worksheets;
            Microsoft.Office.Interop.Excel.Worksheet sheet =
            (Microsoft.Office.Interop.Excel.Worksheet)mybook.Worksheets["Sheet1"];
            sheet.UsedRange.Clear();

            foreach (ViewSchedule vs in col)
            {
                if (vs.Name == "W-01SP-Sub Procurement-Items")
                {

                    Directory.CreateDirectory(_export_folder_name);
                    vs.Export(_export_folder_name, vs.Name + "ggj" + _ext, opt);


                    string stt = _export_folder_name + "\\" + vs.Name + "ggj" + _ext;

                    List<string> a;


                    int count = 0;
                    using (StreamReader sr = new StreamReader(stt))
                    {
                        string delimiter = ";";
                        string line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            count++;
                            int j = 0;
                            a = line.Split('\t').Select<string, string>(s => s.Trim('"')).ToList();

                            foreach (string str in a)
                            {
                                j++;
                                Microsoft.Office.Interop.Excel.Range rngtype =
                                    (Microsoft.Office.Interop.Excel.Range)sheet.Cells[count, j];
                                rngtype.Value = str;
                            }

                            //     TaskDialog.Show("fds", count.ToString() + " " + a.ElementAt(0).ToString());
                        }

                    }
                }

            }
            object missing = System.Reflection.Missing.Value;
            mybook.SaveAs(@"C:\Users\psavine\Desktop\JSQ Push\W-01SP-Sub Procurement-Items33.xlsx"
             , false, false, false, false, false, XlSaveAsAccessMode.xlNoChange, false, false, false, false, false);

            mybook.Close(0);
            myexcel.Quit();
            // myexcel.


            return Result.Succeeded;
        }
  
        private void writeparam(Microsoft.Office.Interop.Excel.Worksheet sheet, Element xElement, int start, int end, int i)
        {
            for (int j = start; j <= end; j++)
            {
                // Retrieve the parameter
                Microsoft.Office.Interop.Excel.Range param =
                (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, j];
                string paramval = param.Value.ToString();

                //retrieve the name of the parameter
                Microsoft.Office.Interop.Excel.Range paramname =
                (Microsoft.Office.Interop.Excel.Range)sheet.Cells[2, j];


                string exparamname = paramname.Value.ToString();
                Autodesk.Revit.DB.Parameter para =
                    xElement.get_Parameter(exparamname);
               


                if (para.StorageType == StorageType.String)
                {
                    xElement.get_Parameter(exparamname).Set(paramval);

                }
                else if (para.StorageType == StorageType.Integer)
                {
                    int prm = Convert.ToInt32(paramval);
                    xElement.get_Parameter(exparamname).Set(prm);

                }



            }

        }


    }

    //Create Pipes External Command
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ViperGrenade : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;
        private ViperFormData vpdata;

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.UI.Result retRes = Autodesk.Revit.UI.Result.Succeeded;
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            //C:\ProgramData\Autodesk\Revit\Addins\2014
            //
            //BranchList<Branch> branchlist = new BranchList<Branch>();

            Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "please pick an import instance");
            ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
            if (dwg == null)
                return Result.Failed;
            XYZ point = sel.PickPoint("please pick a point");
            //sel.PickObject(

            ExternalFileReference elu = ExternalFileUtils.GetExternalFileReference(doc, dwg.GetTypeId());
            ModelPath path = elu.GetAbsolutePath();
            string st = ModelPathUtils.ConvertModelPathToUserVisiblePath(path);
            List<twopoint> geolist = new List<twopoint>();

            Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran.Start();

            StringBuilder sb = new StringBuilder();

            vpdata = new ViperFormData(doc);
            Viper_Form vpform = new Viper_Form(vpdata);
            Makepipes mp = new Makepipes();
            ViperUtils vput = new ViperUtils();
            ElementId level = dwg.LevelId;

            if (vpform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // TaskDialog.Show("ASD",  vpdata.pipetype.ToString() + " - " +vpdata.height.ToString() + " - " +  vpdata.diameter.ToString()  );
 
               geolist = analyzeimport(dwg, geolist);
               twopoint closesttp = vput.nearest(geolist, point);
               List<twopoint> newlist = vput.getlinesonlayer(geolist, closesttp);
                //
               List<twopoint> newlistt = mp.Onaxistree(newlist, doc, point, vpdata);
               geolist = mp.MAKE_PIPES_new(newlistt, doc, vpdata, level);

                tran.Commit();

            }
            else { }

          //  TaskDialog.Show("ASD", "Entering fittings loop");

            if (geolist.Count > 1)
            {          
                 mp.Connectsystemtree(geolist, doc, point);
               //  mp.Connectsystemtree(geolist, doc, point);
            }


            return retRes;
        }

        private List<twopoint> analyzeimport(ImportInstance dwg,  List<twopoint> geolist)
        {
            StringBuilder sb = new StringBuilder();
            Options opt = new Options();
            List<twopoint> pipesmade = new List<twopoint>();

            //Try exploding hte cad using sendkey command here
            twopoint closestitemf = new twopoint();
            ViperUtils vpu = new ViperUtils();
            Autodesk.Revit.DB.Transform transf = null;

            foreach (GeometryObject geoObj in dwg.get_Geometry(opt))
            {
                twopoint closestitem = new twopoint(new XYZ(), new XYZ());

                if (geoObj is GeometryInstance)
                { 

                   //convert all elements in the cad to the two point class
                   vpu.determinegeo(geoObj, geolist, transf);
                }
                else  {         }

            }
          
            return geolist;
        }


        //test the distance of a twopoint object to the current closest point to the 
        //point where the use clicked to select the main.
        private twopoint testdist(twopoint currentclosest, XYZ point, twopoint candidate)
        {
            //Test the which twopoint element is closest to the clicked point
            if (currentclosest != null)
            {
                XYZ avgptcurrent = new GXYZ((currentclosest.pt1.X + currentclosest.pt2.X) / 2,
                                            (currentclosest.pt1.Y + currentclosest.pt2.Y) / 2,
                                            (currentclosest.pt1.Z + currentclosest.pt2.Z) / 2);

                XYZ avgptcand = new GXYZ((candidate.pt1.X + candidate.pt2.X) / 2,
                                        (candidate.pt1.Y + candidate.pt2.Y) / 2,
                                        (candidate.pt1.Z + candidate.pt2.Z) / 2);

                if (avgptcurrent.DistanceTo(point) >= avgptcand.DistanceTo(point))
                {
                    currentclosest = candidate;

                }

                else { }

            }
            else
            {
                currentclosest = candidate;

            }
            return currentclosest;
        }

    }


   #endregion

    //Create Pipes External Command
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ViperSome : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;
        private ViperFormData vpdata;

        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.UI.Result retRes = Autodesk.Revit.UI.Result.Succeeded;
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;
            //C:\ProgramData\Autodesk\Revit\Addins\2014
            //
            //BranchList<Branch> branchlist = new BranchList<Branch>();

            Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "please pick an import instance");
            ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
            if (dwg == null)
                return Result.Failed;
            XYZ point = sel.PickPoint("please pick a point");

            ExternalFileReference elu = ExternalFileUtils.GetExternalFileReference(doc, dwg.GetTypeId());
            ModelPath path = elu.GetAbsolutePath();
            string st = ModelPathUtils.ConvertModelPathToUserVisiblePath(path);
            List<twopoint> geolist = new List<twopoint>();

            Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran.Start();

            ViperUtils vpu = new ViperUtils();
            vpdata = new ViperFormData(doc);
            Viper_Form vpform = new Viper_Form(vpdata);


            if (vpform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                // TaskDialog.Show("ASD",  vpdata.pipetype.ToString() + " - " +vpdata.height.ToString() + " - " +  vpdata.diameter.ToString()  );
               // geolist = vpu.analyzeimport(doc, dwg, point, vpdata, geolist);

                tran.Commit();

            }
            else { }


            if (geolist.Count > 1)
            {
               // Makepipes mp = new Makepipes();
              //  mp.Connectsystemtree(geolist, doc, point);
            }


            return retRes;
        }


    }

    //Create Pipes External Command
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class ViperExcel : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;
        private ViperFormData vpdata;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.UI.Result retRes = Autodesk.Revit.UI.Result.Succeeded;
            m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            vpdata = new ViperFormData(doc);
            Viper_Form vpform = new Viper_Form(vpdata);
            ExcelFunctions ef = new ExcelFunctions();
            Worksheet sheet = ef.Excelget("da", "sheet1");


  
            Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran.Start();

            

            if (vpform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                

                tran.Commit();
            }
            return retRes;
        }


      
    }

    //Create Pipes External Command
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class Viperblock : IExternalCommand
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
            Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "please pick an import instance");
            ImportInstance dwg = doc.GetElement(ref1) as ImportInstance;
            if (dwg == null)
                return Result.Failed;
            XYZ point = sel.PickPoint("please pick a point");

            //get the cad
            ExternalFileReference elu = ExternalFileUtils.GetExternalFileReference(doc, dwg.GetTypeId());
            ModelPath path = elu.GetAbsolutePath();
            string st = ModelPathUtils.ConvertModelPathToUserVisiblePath(path);
            StringBuilder sd = new StringBuilder();
            StringBuilder sb = new StringBuilder();
            Options opt = new Options();

            ViperUtils vpu = new ViperUtils();
            Autodesk.Revit.DB.Transform transf = null;
            Makepipes mp = new Makepipes();
            RevitgetUtils rgu = new RevitgetUtils();



            List<BlockObject> finalblocks = new List<BlockObject>();
            BlockmapperFormData bmfd = new BlockmapperFormData(doc, 
                doc.GetElement(dwg.LevelId) as Level);
            BlockMapForm vpform = new BlockMapForm(bmfd);

            Transaction trans = new Transaction(doc, "asd");
            trans.Start();

            if (vpform.ShowDialog() == System.Windows.Forms.DialogResult. OK)
            {

           
                //create block list
                foreach (GeometryObject geoObj in dwg.get_Geometry(opt))
                {
                    twopoint closestitem = new twopoint(new XYZ(), new XYZ());
                

                    if (geoObj is GeometryInstance)
                    {
                        List<BlockObject> allblocks = vpu.determinegeoblock(geoObj, transf);
                        BlockObject bl = vpu.selectedblock(allblocks, point);

                        // determine which are same as selected
                        foreach (BlockObject blk in allblocks)
                        {
                            if (bl.cadlayer == blk.cadlayer)
                            {
                                if (bl.comparevectorsizes(bl, blk) == true)
                                {
                                    finalblocks.Add(blk);
                                }
                            }
                        }
                        TaskDialog.Show("fb", "all : " + allblocks.Count.ToString() + "final : " + finalblocks.Count.ToString());
                        FamilySymbol fs = bmfd.Vfamily ;

                        foreach (BlockObject blko in finalblocks)
                        {
                            // place the family instance
                            XYZ pt = blko.getblockcentroid();
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

                                
                               // 

                                GXYZ pt3 = new GXYZ(pt.X + blko.transform.BasisX.X, pt.Y + blko.transform.BasisX.Y, pt.Z);

                                Autodesk.Revit.DB.Line lineZ = Autodesk.Revit.DB.Line.CreateBound(pt, ptZ);
                                Autodesk.Revit.DB.Line line = Autodesk.Revit.DB.Line.CreateBound(pt, pt3);
                                GXYZ vector = pt.Add(pt3);
                                DetailCurve detailCurve = doc.Create.NewDetailCurve(doc.ActiveView, line);

                                double angle = blko.transform.BasisX.AngleTo(new GXYZ(1, 0 , 0));
                             //  TaskDialog.Show("angle", angle.ToString());

                               LocationPoint location = instance.Location as LocationPoint;
                               location.Rotate(lineZ, angle);
                              
                                //ElementTransformUtils.RotateElement(doc, instance.Id, lineZ, angle);
                            }
                        }
                        TaskDialog.Show("asd:", sb.ToString());
                        TaskDialog.Show("asd:", sd.ToString());
                        trans.Commit();

                    }
                    else { }

                }
            }
            else { trans.RollBack(); }
       

            return retRes;
        }



    }


    //Create Pipes External Command
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
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
            Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;
            Reference ref1 = sel.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "please pick an import instance");
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
                List<twopoint> tplist = new List<twopoint>();

                //create block list
                foreach (GeometryObject geoObj in dwg.get_Geometry(opt))
                {
                    twopoint closestitem = new twopoint(new XYZ(), new XYZ());


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
                        //create twopoint list
                        foreach (BlockObject blko in finalblocks)
                        {
                            // place the family instance
                            XYZ cntr = blko.getblockcentroid();

                            Level lb = bmfd.cadlevel.revitobj as Level;
                            Level lt = bmfd.toplevel.revitobj as Level;

                            XYZ p1 = new GXYZ(cntr.X, cntr.Y, lb.Elevation + bmfd.bottomoffset);
                            XYZ p2 = new GXYZ(cntr.X, cntr.Y, lt.Elevation + bmfd.topoffset);
                            List<double> dimlist = new List<double>() { 0.16667 };

                            twopoint tp = new twopoint(p1, p2);
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
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class PipeEdit : IExternalCommand
    {

        private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;


        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.UI.Result retRes = Autodesk.Revit.UI.Result.Succeeded;
            // m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;
            IList<Reference> targets = sel.PickObjects(ObjectType.Element, "Select target");

            ViperUtils vputil = new ViperUtils();
            VPipeEndFormData vpdata = new VPipeEndFormData(doc);
            PipeEnds vpform = new PipeEnds(vpdata);
            Makepipes mp = new Makepipes();

            Transaction tran = new Transaction(commandData.Application.ActiveUIDocument.Document, "Viper");
            tran.Start();

            if (vpform.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                mp.rebuildpipes(doc, targets, vpdata);
                tran.Commit();
            }
            else { }

            return retRes;
        }

    }


    //Create Pipes External Command
    [Autodesk.Revit.Attributes.Transaction(Autodesk.Revit.Attributes.TransactionMode.Manual)]
    [Autodesk.Revit.Attributes.Regeneration(Autodesk.Revit.Attributes.RegenerationOption.Manual)]
    public class XXaloft : IExternalCommand
    {

      //  private static Autodesk.Revit.ApplicationServices.Application m_application;
        private static UIDocument uidoc;
        private static Autodesk.Revit.DB.Document doc;


        //EXECUTE FUNCTION
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Autodesk.Revit.UI.Result retRes = Autodesk.Revit.UI.Result.Succeeded;
            // m_application = commandData.Application.Application;
            uidoc = commandData.Application.ActiveUIDocument;
            doc = commandData.Application.ActiveUIDocument.Document;

            Project proj = new Project(doc);
            List<Revit.SDK.Samples.UIAPI.CS.Starwood.UnitType> unittypes =proj.controlleddistribution();
            proj.typelist = unittypes;

            Areatobedivided star = new Areatobedivided(proj, 0);
            Autodesk.Revit.UI.Selection.Selection sels = uidoc.Selection;
            IList<Reference> targets = sels.PickObjects(ObjectType.Element, "Select target");


            foreach (Reference r in targets)
            {
                Autodesk.Revit.DB.Line l = runloop(r);
                star.blines.Add(new bLine(l, false));
            }

            Autodesk.Revit.UI.Selection.Selection sel = uidoc.Selection;
            IList<Reference> masters = sel.PickObjects(ObjectType.Element, "Select target");
            foreach (Reference r in masters)
            {
                Autodesk.Revit.DB.Line l = runloop(r);
                star.blines.Add(new bLine(l, true));     
            }

            foreach (Revit.SDK.Samples.UIAPI.CS.Starwood.UnitType ut in proj.typelist)
            {
                proj.so.WriteLine("unittype " + ut.idealarea.ToString());
            }
           // TaskDialog.Show("no!", "fuck you");
            //System.Windows.MessageBox.Show("fuck you");

            star.subdivisioniteration1(0);
            Linesdone(proj, doc);
           
            
            return retRes;
        }

        private Autodesk.Revit.DB.Line runloop (Reference target)
        {
         ModelLine ml = doc.GetElement(target.ElementId) as ModelLine;
        if(ml == null)
            {TaskDialog.Show("no!" ,"fuck you");
            return null;
            }
         else
           {
            LocationCurve loc = ml.Location as LocationCurve;
            return Autodesk.Revit.DB.Line.CreateBound(loc.Curve.GetEndPoint(0), loc.Curve.GetEndPoint(1));
            }
        }


        private void Linesdone(Project project, Document doc)
        {
            // TaskDialog.Show("as", "DONE");
            project.so.WriteLine();
            project.so.WriteLine("**************************");
            project.so.WriteLine();
            StringBuilder sdr = new StringBuilder();
            StarUtils su = new StarUtils();

        //    try
         //   {
                foreach (Unit unit in project.units)
                {
                    List<Autodesk.Revit.DB.Line> lines = new List<Autodesk.Revit.DB.Line>();
                    sdr.AppendLine("****");
                    //project.so.WriteLine("new area" + unit.unittype.name);
                  //  sdr.AppendLine("new area" + unit.unittype.name);

                    foreach (bLine bl in unit.boundaries)
                    {
                        //sdr.AppendLine("final pt " + bl.line.GetEndPoint(0) + " " + bl.line.GetEndPoint(1));
                        lines.Add(bl.line);
                        project.so.WriteLine("final pt " + bl.line.GetEndPoint(0) + " " +
                           bl.line.GetEndPoint(1));
                        sdr.AppendLine("final pt " + bl.line.GetEndPoint(0) + " " +
                           bl.line.GetEndPoint(1));
                    }
                    su.BuildLinesAreaInRevit(lines, doc, 0, 50, 0);
                }
                System.Windows.MessageBox.Show("asa" + sdr.ToString());
         //   }
       //     catch (Exception)
        //    {
                //  this.project.so.WriteLine("exception");
        //    }
        }


    }





    class TargetElementSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element element)
        {
            // Element must have at least one usable solid
            IList<Solid> solids = ViperUtils.GetTargetSolids(element);

            return solids.Count > 0;
        }

        public bool AllowReference(Reference refer, XYZ point)
        {
            return true;
        }
    }

}
