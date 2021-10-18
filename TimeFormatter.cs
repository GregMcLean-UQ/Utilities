using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    public class TimeFormatter
    {
        public static string format(double time)
        {
            string suffix;

            if (time < 12)
            {
                suffix = "am";
            }
            else
            {
                suffix = "pm";
            }

            int hour = (int)time;

            if (hour > 12)
            {
                hour -= 12;
            }

            int minutes = (int)((time - (double)((int)time)) * 60.0);

            return (hour.ToString("0") + ":" + minutes.ToString("00") + " " + suffix);
        }
    }
}
