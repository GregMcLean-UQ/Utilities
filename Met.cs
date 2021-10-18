using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;

namespace Utilities
{
    public enum MetFormat
    {
        apsim, p51
    }

    public enum MetValueType
    {
        sum, average, maximum, minimum
    }

    public abstract class Met
    {
        protected List<List<double>> metData;
        protected List<string> metHeaders;

        protected Date _startDate;
        protected Date _endDate;

        protected double _latitude;
        protected double _longitude;
        protected int _statNo;
        protected string _statName;
        //---------------------------------------------------------------------------
        // Description: Default Constructor       
        //---------------------------------------------------------------------------
        public Met()
        {
            metHeaders = new List<string>();
            metData = new List<List<double>>();
        }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a Met class with a met 
        //                            filename.
        //---------------------------------------------------------------------------
        public Met(string fileName)
            : this()
        {
            StreamReader sr;

            try
            {
                sr = new StreamReader(fileName);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString());
                //Error
                return;
            }

            initMet(sr);
        }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a Met class with a met 
        //                            filename.
        //---------------------------------------------------------------------------
        public Met(Met met)
            : this()
        {
            _latitude = met.latitude;
            _longitude = met.longitude;
            _statName = met.statName;
            _statNo = met.statNo;

            _startDate = met.startDate;
            _endDate = met.endDate;

            metHeaders = new List<string>(met.MetHeaders.ToArray());

            List<List<double>> MetData = met.MetData;

            for (int i = 0; i < metHeaders.Count; i++)
            {
                metData.Add(new List<double>(MetData[i].ToArray()));
            }
        }
        //---------------------------------------------------------------------------
        public string toDevel()
        {
            List<string> develHeaders = new List<string>() { "year", "day", "maxt", "mint", "pp" };
            List<int> develHeaderPos = new List<int>();

            for (int i = 0; i < develHeaders.Count - 1; i++)
            {
                develHeaderPos.Add(metHeaders.IndexOf(develHeaders[i].ToUpper()));
            }

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(String.Join("  ", develHeaders.ToArray()));
            sb.AppendLine("() () ()");

            for (int i = 0; i < metData[0].Count; i++)
            {
                int year = (int)metData[develHeaderPos[0]][i];
                int day = (int)metData[develHeaderPos[1]][i];
                double maxt = metData[develHeaderPos[2]][i];
                double mint = metData[develHeaderPos[3]][i];

                double pp = calcDayLengthHours(_latitude, day);

                sb.AppendLine(year.ToString() + "  " + day.ToString() + "  " + maxt.ToString("0.0") + "  " + mint.ToString("0.0") + "  " + pp.ToString("0.0"));
            }
            return sb.ToString();
        }

        //---------------------------------------------------------------------------
        public abstract void write(string path);
        //---------------------------------------------------------------------------
        public abstract void addLocalData(List<List<double>> data, List<string> headers, Date stDate, bool adjustEndDate);
        //---------------------------------------------------------------------------
        public abstract void overlayMetFile(Date stWind, Date endWind);
        //---------------------------------------------------------------------------
        public abstract void getAtmospherics(Atmospheric atmos, Date date);
        //---------------------------------------------------------------------------
        public abstract void getAtmosphericsOverlay(Atmospheric atmos, Date stWind, Date endWind, Date date);
        //---------------------------------------------------------------------------
        protected abstract void setStartDate();
        //---------------------------------------------------------------------------
        protected abstract void setStationInfo(string line);
        //---------------------------------------------------------------------------
        protected abstract void parseMet(StreamReader sr);
        //---------------------------------------------------------------------------
        protected void initMet(StreamReader sr)
        {
            parseMet(sr);

            //Set the start date
            setStartDate();

            sr.Close();
        }
        //---------------------------------------------------------------------------
        public void setMetVar(string var, double value, Date date)
        {
            int dateIndex = date - _startDate;
            int varIndex = metHeaders.IndexOf(var.ToUpper());

            if (dateIndex > metData[varIndex].Count - 1)
            {
                metData[varIndex].Add(value);
            }
            else
            {
                metData[varIndex][dateIndex] = value;
            }
        }
        //---------------------------------------------------------------------------
        public double getMetVar(string var, Date date)
        {
            int dateIndex = date - _startDate;
            return metData[metHeaders.IndexOf(var.ToUpper())][dateIndex];
        }
        //---------------------------------------------------------------------------
        public double getMetVar(string var, Date dateStart, Date dateEnd)
        {
            double total = 0;
            int metVarIndex = metHeaders.IndexOf(var.ToUpper());

            for (int dateIndex = (dateStart - _startDate); dateIndex <= (dateEnd - _startDate); dateIndex++)
            {
                total += metData[metVarIndex][dateIndex];
            }

            return total;
        }
        //---------------------------------------------------------------------------
        public double[] getMetVar(string var, Date dateStart, Date dateEnd, bool array)
        {
            double[] values = new double[dateEnd - dateStart + 1];
            int metVarIndex = metHeaders.IndexOf(var.ToUpper());

            int dateIndex = (dateStart - _startDate);

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = metData[metVarIndex][dateIndex + i];
            }

            return values;
        }
        //---------------------------------------------------------------------------
        public abstract void adjust(string var, double[] months, bool absolute);
        //---------------------------------------------------------------------------
        public List<double[]> getMonthlyMetVar(string var, MetValueType type)
        {
            int varIndex = metHeaders.IndexOf(var.ToUpper());
            int currYear = 0;
            int yearCount = -1;

            List<double[]> monthData = new List<double[]>();

            int[] yearMonthCounts = new int[12];

            for (int i = 0; i < metData[0].Count; i++)
            {
                int year, month, day;
                (_startDate + i).getYMD(out year, out month, out day);

                if (currYear != year)
                {
                    yearCount++;
                    currYear = year;
                    if (yearCount > 0 && type == MetValueType.average)
                    //Do the averaging - if required
                    {
                        for (int j = 0; j < 12; j++)
                        {
                            monthData[yearCount - 1][j] /= yearMonthCounts[j];
                        }
                    }
                    //Add news years data
                    monthData.Add(new double[12]);
                    for (int j = 0; j < 12; j++)
                    {
                        if (type == MetValueType.sum || type == MetValueType.average)
                        {
                            monthData[yearCount][j] = 0.0;
                        }
                        else if (type == MetValueType.minimum)
                        {
                            monthData[yearCount][j] = 1000.00;
                        }
                        else if (type == MetValueType.maximum)
                        {
                            monthData[yearCount][j] = -1000.00;
                        }
                        yearMonthCounts[j] = 0;
                    }
                }
                try
                {
                    double val = metData[varIndex][i];
                    if (type == MetValueType.sum || type == MetValueType.average)
                    {
                        monthData[yearCount][month - 1] += val;
                        yearMonthCounts[month - 1]++;
                    }
                    else if (type == MetValueType.minimum)
                    {
                        if (monthData[yearCount][month - 1] > val)
                        {
                            monthData[yearCount][month - 1] = val;
                        }
                    }
                    else if (type == MetValueType.maximum)
                    {
                        if (monthData[yearCount][month - 1] < val)
                        {
                            monthData[yearCount][month - 1] = val;
                        }
                    }
                }
                catch (Exception e)
                {
                    string error = e.ToString();
                    monthData[yearCount][month - 1] = -1.0;
                    yearMonthCounts[month - 1] = 1;
                }
            }
            if (type == MetValueType.average)
            //Do the averaging - if required - After the main loop is finished
            {
                for (int j = 0; j < 12; j++)
                {
                    if (yearMonthCounts[j] == 0)
                    {
                        monthData[yearCount][j] = 0;
                    }
                    else
                    {
                        monthData[yearCount][j] /= yearMonthCounts[j];
                    }
                }
            }
            return monthData;
        }
        //---------------------------------------------------------------------------
        public List<double[]> getMonthlyMetVar(string var, int firstYear, int lastYear, MetValueType type)
        {
            int varIndex = metHeaders.IndexOf(var.ToUpper());

            List<double[]> monthData = new List<double[]>();

            for (int i = firstYear; i <= lastYear; i++)
            {
                Date date = new Date(i, 1, 1);  //Start of the year
                Date endDate = new Date(i, 12, 31);

                double[] yearMonthVals = new double[12];
                int[] yearMonthCounts = new int[12];

                for (int j = 0; j < 12; j++)
                {
                    if (type == MetValueType.sum || type == MetValueType.average)
                    {
                        yearMonthVals[j] = 0.0;
                    }
                    else if (type == MetValueType.minimum)
                    {
                        yearMonthVals[j] = 1000.0;
                    }
                    else if (type == MetValueType.maximum)
                    {
                        yearMonthVals[j] = -1000.0;
                    }
                    yearMonthCounts[j] = 0;
                }

                while (date <= endDate)
                {
                    int year, month, day;
                    date.getYMD(out year, out month, out day);

                    try
                    {
                        double val = metData[varIndex][date - _startDate];
                        if (type == MetValueType.sum || type == MetValueType.average)
                        {
                            yearMonthVals[month - 1] += val;
                            yearMonthCounts[month - 1]++;
                        }
                        else if (type == MetValueType.minimum)
                        {
                            if (yearMonthVals[month - 1] > val)
                            {
                                yearMonthVals[month - 1] = val;
                            }
                        }
                        else if (type == MetValueType.maximum)
                        {
                            if (yearMonthVals[month - 1] < val)
                            {
                                yearMonthVals[month - 1] = val;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //These are incomplete or missing months
                        string error = e.ToString();
                        yearMonthVals[month - 1] = -1.0;
                        yearMonthCounts[month - 1] = 1;
                    }
                    date++;
                }
                //Do the averaging - if required
                if (type == MetValueType.average)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        yearMonthVals[j] /= yearMonthCounts[j];
                    }
                }
                //Add to the output
                monthData.Add(yearMonthVals);
            }
            return monthData;
        }
        //---------------------------------------------------------------------------
        public double[] getMonthlyMetVarAv(string var, MetValueType type)
        {
            List<double[]> metVals = getMonthlyMetVar(var, type);

            double[] monthVals = new double[12];

            Stats s = new Stats();
            for (int i = 0; i < 12; i++)
            {
                double[] yearMonthVals = new double[metVals.Count];

                for (int j = 0; j < metVals.Count; j++)
                {
                    yearMonthVals[j] = metVals[j][i];
                }

                monthVals[i] = s.calcMean(yearMonthVals);
            }
            return monthVals;
        }
        //---------------------------------------------------------------------------
        public double[] getMonthlyMetVarMax(string var, MetValueType type)
        {
            List<double[]> metVals = getMonthlyMetVar(var, type);

            double[] monthVals = new double[12];

            Stats s = new Stats();
            for (int i = 0; i < 12; i++)
            {
                double[] yearMonthVals = new double[metVals.Count];

                for (int j = 0; j < metVals.Count; j++)
                {
                    yearMonthVals[j] = metVals[j][i];
                }

                monthVals[i] = s.calcMax(yearMonthVals);
            }
            return monthVals;
        }
        //---------------------------------------------------------------------------
        public double[] getMonthlyMetVarMin(string var, MetValueType type)
        {
            List<double[]> metVals = getMonthlyMetVar(var, type);

            double[] monthVals = new double[12];

            Stats s = new Stats();
            for (int i = 0; i < 12; i++)
            {
                double[] yearMonthVals = new double[metVals.Count];

                for (int j = 0; j < metVals.Count; j++)
                {
                    yearMonthVals[j] = metVals[j][i];
                }

                monthVals[i] = s.calcMin(yearMonthVals);
            }
            return monthVals;
        }
        //---------------------------------------------------------------------------
        public double calcDayLength(double lat, int doy)
        {
            double rLat = lat * (Math.PI / 180.0);

            double SolarDec = calcSolarDec(doy);

            return Math.Acos(-1 * Math.Tan(rLat) * Math.Tan(SolarDec));
        }
        //---------------------------------------------------------------------------
        public double calcDayLengthHours(double lat, int doy)
        {
            double dayLength = calcDayLength(lat, doy);

            return (180.0 / Math.PI) * (2.0 / 15.0 * dayLength);
        }
        //---------------------------------------------------------------------------
        public double calcDayLengthHours(double lat, int doy, double twilight)
        {
            double aeqnox = 82.25;              // average day number of autumnal equinox
            double dg2rdn = (2.0 * Math.PI) / 360.0; // convert degrees to radians
            double dy2rdn = (2.0 * Math.PI) / 365.25;//  convert days to radians
            double decsol = 23.45116 * dg2rdn;  // amplitude of declination of sun
            //    - declination of sun at solstices.
            double rdn2hr = 24.0 / (2.0 * Math.PI);  //  convert radians to hours

            double alt;                          // twilight altitude limited to max/min
            //   sun altitudes end of twilight
            //   - altitude of sun. (radians)
            double altmn;                        // altitude of sun at midnight
            double altmx;                        // altitude of sun at midday
            double clcd;                         // cos of latitude * cos of declination
            double coshra;                      // cos of hour angle - angle between the
            //   sun and the meridian.
            double dec;                          // declination of sun in radians - this
            //   is the angular distance at solar
            //   noon between the sun and the equator.
            double hrangl;                       // hour angle - angle between the sun
            //   and the meridian (radians).
            double hrlt;                         // day_length in hours
            double latrn;                        // latitude in radians
            double slsd;                         // sin of latitude * sin of declination
            double sun_alt;                      // angular distance between
            // sunset and end of twilight - altitude
            // of sun. (radians)
            // Twilight is defined as the interval
            // between sunrise or sunset and the
            // time when the true centre of the sun
            // is 6 degrees below the horizon.
            // Sunrise or sunset is defined as when
            // the true centre of the sun is 50'
            // below the horizon.


            sun_alt = twilight * dg2rdn;
            // calculate daylangth in hours by getting the
            // solar declination (radians) from the day of year, then using
            // the sin and cos of the latitude.

            // declination ranges from -.41 to .41 (summer and winter solstices)

            dec = decsol * Math.Sin(dy2rdn * ((double)doy - aeqnox));

            // get the max and min altitude of sun for today and limit
            // the twilight altitude between these.

            latrn = lat * dg2rdn;
            slsd = Math.Sin(latrn) * Math.Sin(dec);
            clcd = Math.Cos(latrn) * Math.Cos(dec);

            double mn = slsd - clcd; mn = Math.Max(mn, -1.0); mn = Math.Min(mn, 1.0);
            altmn = Math.Asin(mn);
            double mx = slsd + clcd; mx = Math.Max(mx, -1.0); mx = Math.Min(mx, 1.0);
            altmx = Math.Asin(mx);
            alt = sun_alt; alt = Math.Max(alt, altmn); alt = Math.Min(alt, altmx);

            // get cos of the hour angle

            coshra = (Math.Sin(alt) - slsd) / clcd; coshra = Math.Max(coshra, -1.0);
            coshra = Math.Min(coshra, 1.0);

            // now get the hour angle and the hours of light

            hrangl = Math.Acos(coshra);
            hrlt = hrangl * rdn2hr * 2.0;
            return hrlt;
        }
        //---------------------------------------------------------------------------
        public double calcRadiation(double lat, int doy, double solarRatio)
        {
            double rLat = lat * (Math.PI / 180.0);

            double SolarDec = calcSolarDec(doy);

            double dayLength = calcDayLength(lat, doy);

            double So = 24.0 * 3600.0 * 1360.0 * (dayLength * Math.Sin(rLat) * Math.Sin(SolarDec) +
                  Math.Cos(rLat) * Math.Cos(SolarDec) * Math.Sin(dayLength)) / (Math.PI * 1000000.0);

            return So * solarRatio;
        }
        //---------------------------------------------------------------------------
        public double calcSolarDec(int doy)
        {
            return (Math.PI / 180.0) * 23.45 * Math.Sin(2 * Math.PI * (284.0 + (double)doy) / 365.0);
        }
        //---------------------------------------------------------------------------
        public void setSeasonRainOut(int startIndex, int endIndex)
        {
            int rainIndex = metHeaders.IndexOf("RAIN");

            for (int i = startIndex; i <= endIndex; i++)
            {
                try
                {
                    metData[rainIndex][i] = 0.0;
                }
                catch (Exception)
                {

                }
            }
        }
        //---------------------------------------------------------------------------
        public void setRainOut(Date start, Date end, bool allYears)
        {
            int stDOY, stYear, enDOY, enYear;
            int stIndex, enIndex;
            int yearDiff;

            start.getDOY(out stYear, out stDOY);
            end.getDOY(out enYear, out enDOY);

            yearDiff = enYear - stYear;

            if (!allYears)
            {
                stIndex = start - startDate - 1;
                enIndex = end - startDate - 1;

                setSeasonRainOut(stIndex, enIndex);
            }
            else
            {
                int metStYear = startDate.getYear();
                int metEndYear = endDate.getYear();

                for (int i = metStYear; i <= metEndYear; i++)
                {
                    stIndex = new Date(i, stDOY) - startDate;
                    enIndex = new Date(i + yearDiff, enDOY) - startDate;

                    setSeasonRainOut(stIndex, enIndex);
                }
            }
        }
        //---------------------------------------------------------------------------
        public int statNo
        {
            get { return _statNo; }
        }
        //---------------------------------------------------------------------------
        public double latitude
        {
            get { return _latitude; }
        }
        //---------------------------------------------------------------------------
        public double longitude
        {
            get { return _longitude; }
        }
        //---------------------------------------------------------------------------
        public String statName
        {
            get { return _statName; }
        }
        //---------------------------------------------------------------------------
        public Date startDate
        {
            get
            {
                return _startDate;
            }
            set
            {
            }
        }
        //---------------------------------------------------------------------------
        public Date endDate
        {
            get
            {
                return _endDate;
            }
            set
            {
            }
        }
        //---------------------------------------------------------------------------
        public List<string> MetHeaders
        {
            get
            {
                return metHeaders;
            }
            set
            {
            }
        }
        //---------------------------------------------------------------------------
        public List<List<double>> MetData
        {
            get
            {
                return metData;
            }
            set
            {
            }
        }
    }
}
