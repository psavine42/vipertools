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
using GXYZ = Autodesk.Revit.DB.XYZ;
using Microsoft.Office.Interop.Excel;
using Autodesk.Revit.UI.Selection;



using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Revit.SDK.Samples.UIAPI.CS
{
     class ExcelFunctions
    {

        public Microsoft.Office.Interop.Excel.Worksheet Excelget (string filepath, string sheetname)
        {
            Microsoft.Office.Interop.Excel.Application myexcel = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook mybook = myexcel.Workbooks.Open(
               filepath , 0, true, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, " ", true, false, 0, true, true, false);

            Microsoft.Office.Interop.Excel.Sheets sheets = mybook.Worksheets;
            Microsoft.Office.Interop.Excel.Worksheet sheet =
                (Microsoft.Office.Interop.Excel.Worksheet)mybook.Worksheets[sheetname];

            return sheet;

        }


        public List<twopoint> listfromexcel(Worksheet sheet, int i , int start, int end)
        {
            List<twopoint> tpl = new List<twopoint>();
            StringBuilder sb = new StringBuilder();

            for (int j = start; j <= end; j++)
            {
                Microsoft.Office.Interop.Excel.Range param1 =  (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, j];
                string id1 = Convert.ToString(param1);

                string[] stringSeparators = new string[] { ":","," , ";" };
                string[] res;

                res = id1.Split(stringSeparators, StringSplitOptions.None);
                int count = 0;
                foreach(string s in res)
                {
                    count ++;
                    sb.AppendLine(res.ElementAt(count));
                }

              //  TaskDialog.Show("sda", res.ToString());
               
                //Microsoft.Office.Interop.Excel.Range param1 =  (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, 2];
                //double id1 = Convert.ToDouble(param1) / 12;
                //Microsoft.Office.Interop.Excel.Range param2 = (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, 3];
                //double id2 = Convert.ToDouble(param2) / 12;
                //Microsoft.Office.Interop.Excel.Range param3 =  (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, 4];
                //double id3 = Convert.ToDouble(param3) / 12;
                //Microsoft.Office.Interop.Excel.Range param4 = (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, 5];
                //double id4 = Convert.ToDouble(param4) / 12;
                //Microsoft.Office.Interop.Excel.Range param5 = (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, 6];
                //double id5 = Convert.ToDouble(param5) / 12;
                //Microsoft.Office.Interop.Excel.Range param6 = (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, 7];
                //double id6 = Convert.ToDouble(param6) / 12;

                //XYZ pt1 = new GXYZ(id1, id2, id3);
               // XYZ pt2 = new GXYZ(id4, id5, id6);

              //  twopoint tp = new twopoint(pt1, pt2);
               // tpl.Add(tp);
            }
            TaskDialog.Show("ADS", sb.ToString());

            return tpl;
        }
        
        public void Importdata(List<ElementId> xElements, Document doc, Microsoft.Office.Interop.Excel.Worksheet sheet)
        {

            foreach (ElementId elid in xElements)
            {
                


            }

        }

         //get the endpoints out of a ENDOINT cadmept stream
        public List<double> stringtodoublelist(string id1)
        {
            List<double> pts = new List<double>();
            string[] stringSeparators = new string[] { ":", ",", ";", " ", "x" };
            string[] res;
            res = id1.Split(stringSeparators, StringSplitOptions.None);

            int count = 0;
            foreach (string s in res)
            {
                count++;
                //      sb.Append(res.ElementAt(count + 1));
                double db = 01010101;
                try
                {
                    db = Convert.ToDouble(s);
                    db = db / 12;
                }
                catch (FormatException) { } // sb.AppendLine("format exception:"); }
                if (db != 01010101)
                {
                    pts.Add(db);
                }
            }
            return pts;
        }


        //get the endpoints out of a ENDOINT cadmept stream
        public void determintductorpipe(twopoint tp , string id1, string id2)
        {

                if (id1.Contains("[") == true)
                {
                  //  its a pipe
                    tp.Revitcategory = "Pipe";
                    List<double> dimlist = new List<double>();
                    double diam = 0;
                    if (id1.Contains("/") == true)
                    {
                        int div = id1.First(c => c.Equals("/"));
                        double dl = id1.ElementAt(div - 1) / id1.ElementAt(div + 1);
                        diam = dl;
                    }
                    if (id1.Contains("-") == true)
                    {
                        int div = id1.First(c => c.Equals("-"));
                        double dld = id1.ElementAt(div - 1);
                        diam = diam + dld;
                    }
                    dimlist.Add(diam);
                    tp.dimensionlist = dimlist;

                }
                else
                { 
                //its a duct
                 tp.Revitcategory = "Duct";
                 List<double> size =  stringtodoublelist(id2);
                 tp.dimensionlist = size;
                } 
       
        }


        public string excelval(Worksheet sheet, int i, int j)
        {
            Microsoft.Office.Interop.Excel.Range param1 =
            (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, j];
            string id1 = Convert.ToString(param1.Value);
            return id1;
        }

        public double excelvaldub(Worksheet sheet, int i, int j)
        {
            Microsoft.Office.Interop.Excel.Range param1 =
            (Microsoft.Office.Interop.Excel.Range)sheet.Cells[i, j];
            double id1 = Convert.ToDouble(param1.Value);
            return id1;
        }

        public void ImportRhinoClassic(Document doc, string filepath)
        {
            string s = " placeholder ";
            ExcelFunctions ef = new ExcelFunctions();
            Worksheet sheet = Excelget(s, "Sheet1");

           Microsoft.Office.Interop.Excel.Range last = sheet.Cells.SpecialCells(Microsoft.Office.Interop.Excel.XlCellType.xlCellTypeLastCell, Type.Missing);
           Microsoft.Office.Interop.Excel.Range range = sheet.get_Range("A1", last);

            List<twopoint> listpipes = new List<twopoint>();
      //      List<twopoint> listducts = new List<twopoint>();

            for (int i = 2; i <= last.Row; i++)
            {
                try
                {
                    twopoint tpn = new twopoint();

                    //Create Revit type
                    string idtype = excelval(sheet, i, 1);
                    if (idtype != null)
                    {
                        tpn.Revittypename = idtype;
                    }

                    tpn.pt1 = new GXYZ(excelvaldub(sheet, i, 2), excelvaldub(sheet, i, 3), excelvaldub(sheet, i, 4));
                    tpn.pt2 = new GXYZ(excelvaldub(sheet, i, 5), excelvaldub(sheet, i, 6), excelvaldub(sheet, i, 7));

                    List<double> diam = new List<double>(){excelvaldub(sheet, i, 8)};
                    tpn.dimensionlist = diam;
                    listpipes.Add(tpn);

                }
                catch (Exception){}

            }

            Makepipes mp = new Makepipes();
            mp.MAKE_PIPES_simple(listpipes, doc);
        }




    }


     //class ScheduleDataParser
     //{
     //    /// <summary>
     //    /// Default schedule data file field delimiter.
     //    /// </summary>
     //    static char[] _tabs = new char[] { '\t' };

     //    /// <summary>
     //    /// Strip the quotes around text strings 
     //    /// in the schedule data file.
     //    /// </summary>
     //    static char[] _quotes = new char[] { '"' };

     //    string _name = null;
     //    DataTable _table = null;

     //    /// <summary>
     //    /// Schedule name
     //    /// </summary>
     //    public string Name
     //    {
     //        get { return _name; }
     //    }

     //    /// <summary>
     //    /// Schedule columns and row data
     //    /// </summary>
     //    public DataTable Table
     //    {
     //        get { return _table; }
     //    }

     //    public ScheduleDataParser(string filename)
     //    {
     //        StreamReader stream = File.OpenText(filename);

     //        string line;
     //        string[] a;

     //        while (null != (line = stream.ReadLine()))
     //        {
     //            a = line
     //              .Split(_tabs)
     //              .Select<string, string>(s => s.Trim(_quotes))
     //              .ToArray();

     //            // First line of text file contains 
     //            // schedule name
     //            if (null == _name)
     //            {
     //                _name = a[0];
     //                continue;
     //            }

     //            // Second line of text file contains 
     //            // schedule column names
     //            if (null == _table)
     //            {
     //                _table = new DataTable();

     //                foreach (string column_name in a)
     //                {
     //                    DataColumn column = new DataColumn();
     //                    column.DataType = typeof(string);
     //                    column.ColumnName = column_name;
     //                    _table.Columns.Add(column);
     //                }

     //                _table.BeginLoadData();

     //                continue;
     //            }

     //            // Remaining lines define schedula data

     //            DataRow dr = _table.LoadDataRow(a, true);
     //        }
     //        _table.EndLoadData();
     //    }
     //}












   

    class Excelparammap
    {

        //List<string> 

        public Excelparammap()
        {
            

        }


    }


    class Excelrow
    {
       // string 

    }


}
