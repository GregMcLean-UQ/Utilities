using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;

namespace Utilities
   {
   public class ZipApsimMet : ApsimMet
      {
      public ZipApsimMet(string fileName)
         {
         ZipFile ZF = new ZipFile(fileName);

         initMet(ZF.sr);

         ZF.close();
         }
      }
   }
