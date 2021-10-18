using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
   {
   public class Stats
      {
      //---------------------------------------------------------------------------
      // Description: Returns the sum of an array
      //---------------------------------------------------------------------------
      public double calcSum(double[] arr)
         {
         double sum = 0.0;
         foreach (double f in arr)
            {
            sum += f;
            }
         return sum;
         }
      //---------------------------------------------------------------------------
      // Description: 
      //---------------------------------------------------------------------------
      public double calcMax(double[] arr)
         {
         double[] sortVals = sortArray(arr);
         return sortVals[sortVals.Length - 1];
         }
      //---------------------------------------------------------------------------
      // Description: 
      //---------------------------------------------------------------------------
      public double calcMin(double[] arr)
         {
         double[] sortVals = sortArray(arr);
         return sortVals[0];
         }
      //---------------------------------------------------------------------------
      // Description: Returns the mean of an array
      //---------------------------------------------------------------------------
      public double calcMean(double[] arr)
         {
         return (calcSum(arr) / arr.Length);
         }
      //---------------------------------------------------------------------------
      // Description: Returns the median of an array
      //---------------------------------------------------------------------------
      public double calcMedian(double[] arr)
         {
         double median;

         //Create a working array
         double[] workArr = sortArray(arr);

         //Calc median if the size of the array of years is even
         if (arr.Length % 2 == 0)
            {
            double totalMed = workArr[(arr.Length / 2) - 1] + workArr[(arr.Length / 2)];
            median = totalMed / 2;
            }
         else
            {
            median = workArr[((arr.Length + 1) / 2) - 1];
            }

         return median;
         }
      //---------------------------------------------------------------------------
      // Description: Returns the median of an array
      //---------------------------------------------------------------------------
      public List<double> calcMedian(double[] arr, double[] arr2)
         {
         //arr2 is a dependent array - if we want the median of the first array and the corresponding values
         //in the dependent array - not necessarily th e median of the dependent array
         if (arr.Length != arr2.Length)
            {
            throw new Exception("Arrays are not the same length");
            }

         List<double> median = new List<double>();

         //Create a working array
         double[] workArr = new double[arr.Length];
         double[] workArr2 = new double[arr.Length]; //Should be the same length

         arr.CopyTo(workArr, 0);
         arr2.CopyTo(workArr2, 0);

         sortArray(workArr, workArr2);
         //Calc median if the size of the array of years is even
         if (arr.Length % 2 == 0)
            {
            //Calculate median for the primary array
            double totalMed = workArr[(arr.Length / 2) - 1] + workArr[(arr.Length / 2)];
            median.Add(totalMed / 2);
            //Calculate median for the dependent array
            totalMed = workArr2[(arr.Length / 2) - 1] + workArr2[(arr.Length / 2)];
            median.Add(totalMed / 2);
            }
         else
            {
            //Add median for the primary array
            median.Add(workArr[((arr.Length + 1) / 2) - 1]);
            //Add median for the dependent array
            median.Add(workArr2[((arr.Length + 1) / 2) - 1]);
            }

         return median;
         }
      //---------------------------------------------------------------------------
      // Description: Sorts an array from lowest to highest
      //---------------------------------------------------------------------------
      public double[] sortArray(double[] arr)
         {
         double[] sorted = new double[arr.Length];
         arr.CopyTo(sorted, 0);
         double temp;
         
         //Bubblesort array
         for (int i = (sorted.Length - 1); i >= 0; i--)
            {
            for (int j = 0; j < i; j++)
               {
               if (sorted[j] > sorted[j + 1])
                  {
                  temp = sorted[j];
                  sorted[j] = sorted[j + 1];
                  sorted[j + 1] = temp;
                  }
               }
            }
         return sorted;
         }
      //---------------------------------------------------------------------------
      public void sortArray(double[] arr, double[] arr2)
         {
         //arr2 is the dependent array so what we do to the first one we do to the second
         double temp;
         //Bubblesort array
         for (int i = (arr.Length - 1); i >= 0; i--)
            {
            for (int j = 0; j < i; j++)
               {
               if (arr[j] > arr[j + 1])
                  {
                  temp = arr[j];
                  arr[j] = arr[j + 1];
                  arr[j + 1] = temp;

                  //Repeat to the dependent array
                  temp = arr2[j];
                  arr2[j] = arr2[j + 1];
                  arr2[j + 1] = temp;
                  }
               }
            }
         }

      //---------------------------------------------------------------------------
      // Description: Returns the Regression parameters for two given arrays
      //---------------------------------------------------------------------------
      public void calcRegression(double[] x, double[] y, out double slope, out double intercept, out double rsq)
      {

          rsq = calcRsq(x, y);

          //Calc the means
          double meanX = calcMean(x);
          double meanY = calcMean(y);
          
          //calc the std devs
          double stDevX = calcStDev(x, meanX);
         
          //calc the co-variance
          double coVarXY = calcCoVar(x, meanX, y, meanY);

          slope = coVarXY / Math.Pow(stDevX, 2);

          intercept = (-1 * slope * meanX) + meanY;
      }

      //---------------------------------------------------------------------------
      // Description: Returns the Rsq between two arrays
      //---------------------------------------------------------------------------
      public double calcRsq(double[] obs, double[] pred)
         {
         //Calc the means
         double meanObs = calcMean(obs);
         double meanPred = calcMean(pred);
         //calc the std devs
         double stDevObs = calcStDev(obs, meanObs);
         double stDevPred = calcStDev(pred, meanPred);
         //calc the co-variance
         double coVar = calcCoVar(obs, meanObs, pred, meanPred);
         //Calc r
         double r = coVar / (stDevPred * stDevObs);

         return (double)Math.Pow(r, 2);
         }
      //---------------------------------------------------------------------------
      // Description: Returns the standard deviation of an array given only the array
      //---------------------------------------------------------------------------
      public double calcStDev(double[] arr)
         {
         double meanArr = calcMean(arr);

         return calcStDev(arr, meanArr);
         }
      //---------------------------------------------------------------------------
      // Description: Returns the standard deviation of an array given the array and 
      //              its mean
      //---------------------------------------------------------------------------
      public double calcStDev(double[] arr, double meanArr)
         {
         double tot = 0.0;
         for (int i = 0; i < arr.Length; i++)
            {
            tot = tot + (double)Math.Pow((arr[i] - meanArr), 2);
            }

         return (double)Math.Sqrt((tot / arr.Length));
         }
      //---------------------------------------------------------------------------
      // Description: Calculates the co-variance between two arrays given only the 
      //              arrays
      //---------------------------------------------------------------------------
      public double calcCoVar(double[] x, double[] y)
         {
         double meanX = calcMean(x);
         double meanY = calcMean(y);

         return calcCoVar(x, meanX, y, meanY);
         }
      //---------------------------------------------------------------------------
      // Description: Calculates the co-variance between two arrays given the 
      //              arrays and their means
      //---------------------------------------------------------------------------
      public double calcCoVar(double[] x, double meanX, double[] y, double meanY)
         {
         double tot = 0.0;
         double sX, sY;
         for (int i = 0; i < x.Length; i++)
            {
            sX = x[i] - meanX;
            sY = y[i] - meanY;

            tot += (sX * sY);
            }

         return (tot / x.Length);
         }
      //---------------------------------------------------------------------------
      // Description: Returns the value from an array given the percentile
      //---------------------------------------------------------------------------
      public double calcValueFromPercentile(double[] arr, double percentile)
         {
         if (arr.Length == 1)
             {
             return arr[0];
             }

         if (arr.Length < 3)
             {
             if (arr[0] == arr[1])
                 {
                 return arr[0];
                 }
             }

         //Create a working array
         double[] workArr = sortArray(arr);

         //Get the position of the percentile
         double[] percentiles = calcCDF(workArr.Length);

         LookupTable LT = new LookupTable(percentiles, workArr);

         return LT.getYVal(percentile);
         }
      //---------------------------------------------------------------------------
      // Description: Return the percentile of a value within an array 
      //---------------------------------------------------------------------------
      public double calcPercentileFromValue(double[] arr, double val)
         {
         double perc;
         //Create a working array
         double[] workArr = sortArray(arr);

         //Check value is in the range of the array if not make it probability 0 or 100%
         if (val <= workArr[0])
            {
            perc = 0.0;
            }
         else if (val > workArr[arr.Length - 1])
            {
            perc = 1.0;
            }
         else
            {
            double[] percentiles = calcCDF(workArr.Length);

            LookupTable LT = new LookupTable(workArr, percentiles);

            try
               {
               perc = LT.getYVal(val);
               }
            catch (IndexOutOfRangeException e)
               {
               string temp = e.Message;

               if (val < workArr[0])
                  {
                  perc = 0.00;
                  }
               else
                  {
                  perc = 100.00;
                  }
               }
            }

         return perc;
         }
      //---------------------------------------------------------------------------
      //  
      //---------------------------------------------------------------------------
      public double[] calcCDF(int length)
          {
          double[] percentiles = new double[length];

          //Create the percentile array
          percentiles[0] = 0.0;
          percentiles[length - 1] = 1.0;

          for (int i = 1; i < (percentiles.Length - 1); i++)
              {
              percentiles[i] = (1 / ((double)percentiles.Length - 1)) * i;
              }
          return percentiles;
          }
      }
   }
