using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Viper.Forms
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

  

    }
}
