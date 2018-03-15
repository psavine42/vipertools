using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Revit.SDK.Samples.UIAPI.CS
{
   public class Pendata
    {

       private List<MEPtoObjectLink> meplinks;
       public string filename {get ; set; } 

      // public 

       public List<MEPtoObjectLink> Meplinks
       {
           get
           {
               return meplinks;
           }
           set
           {
               meplinks = value;
           }
       }

       //public string Filename
       //{
       //    get
       //    {
       //        return meplinks;
       //    }
       //    set
       //    {
       //        meplinks = value;
       //    }
       //}

    }
}
