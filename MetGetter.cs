using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;

namespace Utilities
   {
   public class MetGetter
      {
      enum MetExt
         {
         met, p51
         }

      DateTime _latestUpdate;
      WebClient _WC;
      string _remoteUri;
      public MetGetter(string remoteUri)
         : base()
         {
         _remoteUri = remoteUri;

         }
      //---------------------------------------------------------------------------
      //
      //---------------------------------------------------------------------------   
      public MetGetter()
         {
         _WC = new WebClient();
         _WC.Proxy = WebRequest.DefaultWebProxy;

         _WC.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
         _WC.Headers.Add(HttpRequestHeader.UserAgent, "anything");

         DateTime now = DateTime.Now;
         _latestUpdate = new DateTime(now.AddDays(-1).Year, now.AddDays(-1).Month, now.AddDays(-1).Day);
         }
      //---------------------------------------------------------------------------
      // 
      //---------------------------------------------------------------------------   
      public DateTime latestUpdate
         {
         get
            {
            return _latestUpdate;
            }
         set
            {
            }
         }
      //---------------------------------------------------------------------------
      // 
      //---------------------------------------------------------------------------   
      public bool getFile(DateTime start, DateTime end, int station, string filePath, MetFormat format, bool zip, int attempt)
         {
         _WC = new WebClient();
         _WC.Proxy = WebRequest.GetSystemWebProxy();  //.DefaultWebProxy;

         //if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName == "dpi.qld.gov.au")
         //    {
         //_WC.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
         //    }
         //_WC.Headers.Add(HttpRequestHeader.UserAgent, "anything");
         // _WC.Proxy.Credentials = System.Net.CredentialCache.

         MetExt ext = new MetExt();
         ext = (MetExt)format;

         FileStream outFile;
         GZipStream gz;

         string fileName = filePath + "\\" + station.ToString() + "." + ext.ToString();

         byte[] data;

         try
            {

            string cgi = buildHTTPRequestString(start, end, station, ext.ToString(), format.ToString());
            string uri = _remoteUri + "/" + cgi;

            data = _WC.DownloadData(uri);


            // data = new byte[2];
            if (zip)
               {
               fileName += ".gz";
               }

            outFile = File.Create(fileName);

            if (zip)
               {
               gz = new GZipStream(outFile, CompressionMode.Compress);
               gz.Write(data, 0, data.Length);

               gz.Close();
               }
            else
               {
               outFile.Write(data, 0, data.Length);
               }
            outFile.Close();
            return true;
            }
         catch (Exception e)
            {
            System.Windows.Forms.MessageBox.Show(e.ToString(), "MetGetter Error",
              System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            return false;

            }
         // return false;
         }
      //---------------------------------------------------------------------------
      // 
      //---------------------------------------------------------------------------   
      private string buildHTTPRequestString(DateTime start, DateTime end, int station, string ext, string format)
         {
         string command = "cgi-bin/getData." + ext + "?format=" + format + "&station=" + station.ToString() + "&ddStart=" +
            start.Day.ToString() + "&mmStart=" + start.Month.ToString() + "&yyyyStart=" + start.Year.ToString() + "&ddFinish=" +
            end.Day.ToString() + "&mmFinish=" + end.Month.ToString() + "&yyyyFinish=" + end.Year.ToString();

         return (command);
         }
      }
   }
