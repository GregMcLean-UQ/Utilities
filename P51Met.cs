using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Utilities
   {
   public class P51Met : Met
      {

      //---------------------------------------------------------------------------
      // Description: Default Constructor       
      //---------------------------------------------------------------------------
      public P51Met()
         {
         }
      //---------------------------------------------------------------------------
      // Description: Constructor - Creates an instance of a Met class with a met 
      //                            filename.
      //---------------------------------------------------------------------------
      public P51Met(string fileName) : base(fileName)
         {
         }
      //---------------------------------------------------------------------------
      // Description: Constructor - Creates an instance of a Met class with a met 
      //                            filename.
      //---------------------------------------------------------------------------
      public P51Met(Met met)
          : base(met)
          {
          }
      //---------------------------------------------------------------------------
      // Description: Parses the column names and the data within the met file
      //---------------------------------------------------------------------------
      protected override void parseMet(StreamReader sr)
         {
         char[] CharSeparators = { ' ' };

         //Set other info
         setStationInfo(sr.ReadLine());
         
         //Parse the headers
         string[] words = sr.ReadLine().Split(CharSeparators, StringSplitOptions.RemoveEmptyEntries);

         for (int i = 0; i < words.Length; i++)
            {
            metHeaders.Add(words[i].ToUpper());
            }

         //Dimension the List arrays
         for (int i = 0; i < metHeaders.Count; i++)
            {
            metData.Add(new List<double>());
            }

         while(!sr.EndOfStream)
            {
            String line = sr.ReadLine();

            words = line.Split(CharSeparators, StringSplitOptions.RemoveEmptyEntries);

            if (words[0] != "")
               {
               for (int j = 0; j < metHeaders.Count; j++)
                  {
                  double item;
                  double.TryParse(words[j], out item);
                  metData[j].Add(item);
                  }
               }
            }
         }
      //---------------------------------------------------------------------------
      // Description: Sets the values inside an atmospherics container on a specific
      //              date.
      //---------------------------------------------------------------------------
      public override void getAtmospherics(Atmospheric atmos, Date date)
         {
         atmos.Rain = getMetVar("RAIN", date);
         atmos.Rad = getMetVar("RAD", date);
         atmos.MaxT = getMetVar("TMAX", date);
         atmos.MinT = getMetVar("TMIN", date);
         atmos.PanE = getMetVar("EVAP", date);
         }
      //---------------------------------------------------------------------------      
      public override void addLocalData(List<List<double>> data, List<string> headers, Date stDate, bool adjustEndDate)
          {
          }
      //---------------------------------------------------------------------------
      public override void overlayMetFile(Date stWind, Date endWind)
          {
          }
      //---------------------------------------------------------------------------
      public override void getAtmosphericsOverlay(Atmospheric atmos, Date stWind, Date endWind, Date date)
         {
         throw new Exception("The method or operation is not implemented.");
         }

      //---------------------------------------------------------------------------
      // Description: Sets global start date.
      //---------------------------------------------------------------------------
      protected override void setStartDate()
         {
         int year;
         int doy;
         
         year = (int)metData[metHeaders.IndexOf("DATE")][0] / 10000;       
         doy = (int)metData[metHeaders.IndexOf("JDAY")][0];
         
         _startDate = new Date(year, doy);
         }
      //---------------------------------------------------------------------------
      // Description: Parses the header section of the met file and sets the global
      //              information
      //---------------------------------------------------------------------------
      protected override void setStationInfo(string header)
         {
         char[] CharSeparators = { ' ' };

         String[] words = header.Split(CharSeparators, StringSplitOptions.RemoveEmptyEntries);

         _latitude = double.Parse(words[0]);
         _longitude = double.Parse(words[1]);
         _statNo = int.Parse(words[4]);

         for (int i = 5; i < words.Length; i++)
            {
            _statName += words[i];
            if (i < words.Length - 1)
               {
               _statName += " ";
               }
            }
 
         }
      //---------------------------------------------------------------------------
      public override void write(string path)
         {
         throw new Exception("The method or operation is not implemented.");
         }
      //---------------------------------------------------------------------------
      public override void adjust(string var, double[] months, bool absolute)
         {
         throw new Exception("The method or operation is not implemented.");
         }
      }
   }
