using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace Utilities
   {
   public class ZipFile
      {
      private StreamReader _sr;
      private GZipStream _gz;
      private FileStream _inFile;

      public ZipFile(string fileName)
         {
         _inFile = null;

         try
            {
            _inFile = File.OpenRead(fileName);
            }
         catch (Exception e)
            {
            System.Windows.Forms.MessageBox.Show(e.ToString());
            }

         _gz = new GZipStream(_inFile, CompressionMode.Decompress);

         _sr = new StreamReader(_gz);
         
         }
      //---------------------------------------------------------------------------
      public void close()
         {
         _sr.Close();
         _gz.Close();
         _inFile.Close();
         }
      //---------------------------------------------------------------------------
      public StreamReader sr
         {
         get
            {
            return _sr;
            }
         set
            {
            }
         }
      }
   }
