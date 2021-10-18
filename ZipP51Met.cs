using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Compression;
using System.IO;

namespace Utilities
   {
   public class ZipP51Met : P51Met
      {
      public ZipP51Met(string fileName)
         {
         ZipFile ZF = new ZipFile(fileName);

         initMet(ZF.sr);

         ZF.close();
         }
      }
   }
