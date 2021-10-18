using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace Utilities
   {
   public class MonthlyData
      {
      double[] Data;
      int[] Counts;

      public MonthlyData()
         {
         Data = new double[12];
         Counts = new int[12];

         for (int i = 0; i < 12; i++)
            {
            Data[i] = 0.0;
            Counts[i] = 0;
            }
         }

      public void calcAverages()
         {
         Stats s = new Stats();

         for (int i = 0; i < 12; i++)
            {
            Data[i] /= (double)Counts[i];
            }
         }

      public void addData(double value, int month)
         {
         Data[month - 1] += value;
         Counts[month - 1]++;
         }

      public double getData(int month)
         {
         return Data[month - 1];
         }
      }


   public class MetMonthlyAnalyser
      {
      public enum TotalType { Average, Total };
      MonthlyData[] Years;
      int _startYear;
      int _endYear;
      ApsimMet _met;

      public MetMonthlyAnalyser(string fileName, string variable, int startYear, int endYear,
         TotalType type)
         {

         _startYear = startYear;
         _endYear = endYear;

         if (fileName.IndexOf(".gz") < 0)
            {
            _met = new ApsimMet(fileName, true);
            }
         else
            {
            _met = new ZipApsimMet(fileName);
            }

         Years = new MonthlyData[endYear - startYear + 1];

         for (int i = 0; i < (endYear - startYear + 1); i++)
            {
            Years[i] = new MonthlyData();
            }

         for (Date i = new Date(startYear, 1); i <= new Date(endYear, 12, 31); i++)
            {
            int day, month, year;
            i.getYMD(out year, out month, out day);
            
            if (i <= _met.endDate)
               {
               Years[year - startYear].addData(_met.getMetVar(variable, i), month);
               }
            }

         if (type == TotalType.Average)
            {
            foreach (MonthlyData Year in Years)
               {
               Year.calcAverages();
               }
            }
         }

      public override string ToString()
         {
         StringBuilder s = new StringBuilder();
         for (int i = _startYear; i <= _endYear; i++)
            {
            for (int j = 0; j < 12; j++)
               {
               string st = _met.statNo.ToString() + "," + i.ToString() + "," +
                  (j + 1).ToString() + "," + Years[i - _startYear].getData(j + 1).ToString() + "\n";

               s.Append(st);
               }
            }
         return s.ToString();
         }
      }
   }
