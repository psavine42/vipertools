using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revit.SDK.Samples.UIAPI.CS
{
    class Misc
    {
        //SENDKEYS


        //[DllImport("USER32.DLL")]
        //public static extern IntPtr FindWindow(
        //  string lpClassName, string lpWindowName);

        //[DllImport("USER32.DLL")]
        //public static extern bool SetForegroundWindow(
        //  IntPtr hWnd);

        //const string _window_class_name_zero
        //  = "Afx:00400000:8:00010011:00000000:007B05A1";

        //const string _window_class_name_project_open
        //  = "Afx:00400000:8:00010011:00000000:007B05A1";

        //static int Main(string[] args)
        //{
        //    IntPtr revitHandle
        //      = FindWindow(_window_class_name_project_open, null);

        //    if (IntPtr.Zero == revitHandle)
        //    {
        //        revitHandle = FindWindow(_window_class_name_zero, null);
        //    }

        //    if (IntPtr.Zero == revitHandle)
        //    {
        //        Console.WriteLine("Unable to find Revit window."
        //          + " Is Revit Architecture up and running yet?");
        //        return 1;
        //    }

        //    SetForegroundWindow(revitHandle);
        //    SendKeys.SendWait("{F1}");

        //    SetForegroundWindow(revitHandle);
        //    SendKeys.SendWait("^{F10}{LEFT}{LEFT}{DOWN}{UP}{ENTER}");

        //    return 0;
       // }
    }
}
