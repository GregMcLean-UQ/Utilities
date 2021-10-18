using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
    {
    public class Date
        {
        private int _JDN;

        public int JDN
            {
            get { return _JDN; }
            set { _JDN = value; }
            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a DateClass from a
        //              Julian Day Number
        //---------------------------------------------------------------------------
        public Date(int jdn)
            {
            _JDN = jdn;
            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a DateClass with a
        //              'Day of Month' and a Month  and a Year.
        //              The date must be legitimate.
        //---------------------------------------------------------------------------
        public Date(int year, int month, int day)
            {
            int a = (14 - month) / 12;
            int y = year + 4800 - a;
            int m = month + 12 * a - 3;
            _JDN = day + (153 * m + 2) / 5 + 365 * y + y / 4 - y / 100 + y / 400 - 32045;
            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a DateClass with a
        //              'Day of Year' and a Year. Dates constructed with this 
        //              constructor outside the class must be a legitimate date.   
        //---------------------------------------------------------------------------
        public Date(int year, int doy)
            : this(new Date(year, 1, 1).JDN + doy - 1)
            {
            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a DateClass from a
        //              System.DateTime.
        //---------------------------------------------------------------------------
        public Date(DateTime date)
            : this(date.Year, date.Month, date.Day)
            {

            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a DateClass from a
        //              System.DateTime.
        //---------------------------------------------------------------------------
        public Date(string dateStr)
            {
            int year, month, day;

            //Assume either y-m-d or d-m-y
            char[] delimiters = new char[] { '/', '\\', '-', '.' };

            string[] dateItems = dateStr.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            //Try to determine which way around the string is
            int first = int.Parse(dateItems[0]);
            int last = int.Parse(dateItems[2]);

            if (first > 31)
                {
                //then probably y-m-d
                year = int.Parse(dateItems[0]);
                month = int.Parse(dateItems[1]);
                day = int.Parse(dateItems[2]);
                }
            else //if (last > 12)
                {
                year = int.Parse(dateItems[2]);
                month = int.Parse(dateItems[1]);
                day = int.Parse(dateItems[0]);
                }

            _JDN = (new Date(year, month, day))._JDN;

            }
        //---------------------------------------------------------------------------
        // Description: Operator - - Calculates the number of days between 'date' and this
        //---------------------------------------------------------------------------
        public static int operator -(Date lhs, Date rhs)
            {
            return lhs.JDN - rhs.JDN;
            }
        //---------------------------------------------------------------------------
        // Description: operator-(int) - Subtracts 'days' number of days from this.
        //---------------------------------------------------------------------------
        public static Date operator -(Date date, int days)
            {
            return new Date(date.JDN - days);
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public static Date operator +(Date date, int days)
            {
            return new Date(date.JDN + days);
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public static bool operator >(Date lhs, Date rhs)
            {
            if (lhs.JDN > rhs.JDN)
                {
                return true;
                }
            return false;
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public static bool operator <(Date lhs, Date rhs)
            {
            if (lhs.JDN < rhs.JDN)
                {
                return true;
                }
            return false;
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public override bool Equals(object obj)
            {
            return base.Equals(obj);
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public override int GetHashCode()
            {
            return base.GetHashCode();
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public static bool operator ==(Date lhs, Date rhs)
            {
            if (lhs.JDN == rhs.JDN)
                {
                return true;
                }
            return false;
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public static bool operator !=(Date lhs, Date rhs)
            {
            if (lhs.JDN != rhs.JDN)
                {
                return true;
                }
            return false;
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public static bool operator <=(Date lhs, Date rhs)
            {
            if (lhs.JDN <= rhs.JDN)
                {
                return true;
                }
            return false;
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public static bool operator >=(Date lhs, Date rhs)
            {
            if (lhs.JDN >= rhs.JDN)
                {
                return true;
                }
            return false;
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public static Date operator --(Date date)
            {
            return new Date(date.JDN - 1);
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public static Date operator ++(Date date)
            {
            return new Date(date.JDN + 1);
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public void getDOY(out int year, out int doy)
            {
            int y, m, d;

            getYMD(out y, out m, out d);

            year = y;

            doy = _JDN - new Date(year, 1, 1).JDN + 1;
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public int getDOY()
            {
            int y, m, d;

            getYMD(out y, out m, out d);

            return (_JDN - new Date(y, 1, 1).JDN + 1);
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public void getYMD(out int year, out int month, out int day)
            {
            int j = _JDN + 32044;
            int g = j / 146097;
            int dg = j % 146097;
            int c = (dg / 36524 + 1) * 3 / 4;
            int dc = dg - c * 36524;
            int b = dc / 1461;
            int db = dc % 1461;
            int a = (db / 365 + 1) * 3 / 4;
            int da = db - a * 365;
            int y = g * 400 + c * 100 + b * 4 + a;
            int m = (da * 5 + 308) / 153 - 2;
            int d = da - (m + 4) * 153 / 5 + 122;
            year = y - 4800 + (m + 2) / 12;
            month = (m + 2) % 12 + 1;
            day = d + 1;
            }
        //---------------------------------------------------------------------------
        public string toAccessString()
            {

            int y, m, d;

            getYMD(out y, out m, out d);

            return m.ToString() + "/" + d.ToString() + "/" + y.ToString();
            }

        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public string getApsimDate()
            {
            string[] months = { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };
            int y, m, d;

            getYMD(out y, out m, out d);

            return d.ToString() + "-" + months[m - 1];
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public DateTime getDate()
            {
            int y, m, d;

            getYMD(out y, out m, out d);

            return new DateTime(y, m, d);
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public int getYear()
            {
            int y, m, d;

            getYMD(out y, out m, out d);

            return y;
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public int getMonth()
            {
            int y, m, d;

            getYMD(out y, out m, out d);

            return m;
            }
        //---------------------------------------------------------------------------
        // Description:
        //---------------------------------------------------------------------------
        public int getDay()
            {
            int y, m, d;

            getYMD(out y, out m, out d);

            return d;
            }
        }
    }
