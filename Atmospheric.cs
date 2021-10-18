using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
   {
   public class Atmospheric
      {
      private double _Rain;
      private double _MaxT;
      private double _MinT;
      private double _Rad;
      private double _PanE;
      //---------------------------------------------------------------------------
      public double Rain
         {
         get
            {
            return _Rain;
            }
         set
            {
            _Rain = value;
            }
         }
      //---------------------------------------------------------------------------
      public double MaxT
         {
         get
            {
            return _MaxT;
            }
         set
            {
            _MaxT = value;
            }
         }
      //---------------------------------------------------------------------------
      public double MinT
         {
         get
            {
            return _MinT;
            }
         set
            {
            _MinT = value;
            }
         }
      //---------------------------------------------------------------------------
      public double Rad
         {
         get
            {
            return _Rad;
            }
         set
            {
            _Rad = value;
            }
         }
      //---------------------------------------------------------------------------
      public double PanE
         {
         get
            {
            return _PanE;
            }
         set
            {
            _PanE = value;
            }
         }
      }
   }
