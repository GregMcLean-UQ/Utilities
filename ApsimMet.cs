using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Utilities
    {
    public class ApsimMet : Met
        {
        protected double _amp;
        protected double _tav;

        //---------------------------------------------------------------------------
        // Description: Default Constructor       
        //---------------------------------------------------------------------------
        public ApsimMet()
            {
            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a Met class with a met 
        //                            filename.
        //---------------------------------------------------------------------------
        public ApsimMet(string fileName)
            : base(fileName)
            {
            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a Met class with a met 
        //                            filename.
        //---------------------------------------------------------------------------
        public ApsimMet(Met met)
            : base(met)
            {
            _amp = ((ApsimMet)met).amp;
            _tav = ((ApsimMet)met).tav;

            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a Met class with a met 
        //                            filename but if includeAveT is true adds the 
        //                            average temperature variable also.
        //---------------------------------------------------------------------------
        public ApsimMet(string fileName, bool includeAveT)
            : base(fileName)
            {
            if (includeAveT)
                {
                addAverageTemp();
                }
            }
        //---------------------------------------------------------------------------
        // Description: Parses the column names and the data within the met file
        //---------------------------------------------------------------------------
        protected override void parseMet(StreamReader sr)
            {
            StringBuilder header = new StringBuilder();

            char[] CharSeparators = { ' ', '\t' };
            char[] TrimChars = { ' ' };

            bool inHeader = true;

            //Parse the Met File
            while (!sr.EndOfStream)
                {
                String line = sr.ReadLine();

                if (inHeader)
                    {
                    if (Regex.IsMatch(line, @"(^[!\s\(\[\n])|=") || line == "")
                        {
                        header.Append(line.TrimEnd(TrimChars));
                        header.Append("\n");
                        }
                    else
                        {
                        inHeader = false;
                        }
                    }

                if (!inHeader)
                    {
                    string[] words = line.Split(CharSeparators, StringSplitOptions.RemoveEmptyEntries);

                    if (metHeaders.Count == 0) //Should be the headers line
                        {
                        for (int i = 0; i < words.Length; i++)
                            {
                            metHeaders.Add(words[i].ToUpper());
                            }

                        //Dimension the Lists
                        for (int i = 0; i < metHeaders.Count; i++)
                            {
                            metData.Add(new List<double>());
                            }

                        //Throw away the next line
                        sr.ReadLine();
                        }
                    else
                        {
                        if (words.Length == metHeaders.Count) //Should be the data lines
                            {
                            for (int i = 0; i < metHeaders.Count; i++)
                                {
                                double item;
                                double.TryParse(words[i], out item);
                                metData[i].Add(item);
                                }
                            }
                        }
                    }
                }

            //Set other info
            setStationInfo(header.ToString());
            }
        //---------------------------------------------------------------------------
        private void addAverageTemp()
            {
            //Add the Average Temp var and index the temperature vars
            metHeaders.Add("AVET");
            metData.Add(new List<double>());
            int aveTIndex = metHeaders.Count - 1;
            int minTIndex = metHeaders.IndexOf("MINT");
            int maxTIndex = metHeaders.IndexOf("MAXT");

            //Add the daily average Temp
            for (int i = 0; i < metData[0].Count; i++)
                {
                metData[aveTIndex].Add((metData[maxTIndex][i] + metData[minTIndex][i]) / 2.0);
                }
            }
        //---------------------------------------------------------------------------
        // Description: Sets the values inside an atmospherics container on a specific
        //              date.
        //---------------------------------------------------------------------------
        public override void getAtmospherics(Atmospheric atmos, Date date)
            {
            atmos.Rain = getMetVar("RAIN", date);
            atmos.Rad = getMetVar("RADN", date);
            atmos.MaxT = getMetVar("MAXT", date);
            atmos.MinT = getMetVar("MINT", date);
            atmos.PanE = getMetVar("PAN", date);
            }
        //---------------------------------------------------------------------------
        // Description: 
        //              
        //---------------------------------------------------------------------------
        public override void getAtmosphericsOverlay(Atmospheric atmos, Date stWind, Date endWind, Date date)
            {
            int d, m, y;
            int std, stm, sty;
            int endd, endm, endy;

            bool afterStart = false;
            bool beforeEnd = false;

            date.getYMD(out y, out m, out d);
            stWind.getYMD(out sty, out stm, out std);
            endWind.getYMD(out endy, out endm, out endd);

            if (sty < endy)
            //Is the window spread accross years
                {
                endm += 12;
                if (y == endy)
                //Date is in the second year
                    {
                    m += 12;
                    }
                }

            if (d >= std && m >= stm)
                {
                afterStart = true;
                }

            if (d <= endd && m <= endm)
                {
                beforeEnd = true;
                }

            if (afterStart && beforeEnd)
                {
                //In the window
                if (m > 12)
                    {
                    getAtmospherics(atmos, new Date(endy, m - 12, d));
                    }
                else
                    {
                    getAtmospherics(atmos, new Date(sty, m, d));
                    }
                }
            else
                {
                getAtmospherics(atmos, date);
                }
            }
        //---------------------------------------------------------------------------
        // Description: Sets global start date.
        //---------------------------------------------------------------------------
        protected override void setStartDate()
            {
            int year;
            int doy;

            year = (int)metData[metHeaders.IndexOf("YEAR")][0];
            doy = (int)metData[metHeaders.IndexOf("DAY")][0];

            _startDate = new Date(year, doy);
            _endDate = _startDate + metData[0].Count - 1;
            }
        //--------------------------------------------------------------------------  -
        // Description: Parses the header section of the met file and sets the global
        //              information
        //---------------------------------------------------------------------------
        protected override void setStationInfo(string header)
            {

            double fvalue;
            int ivalue;

            Match m;

            //Get the latitiude
            m = Regex.Match(header, @".*latitude =\s*([-\.0-9]+)");
            double.TryParse(m.Groups[1].ToString(), out fvalue);
            _latitude = fvalue;

            string s = m.Groups[0].ToString();
            string s1 = m.Groups[1].ToString();


            //Get the latitiude
            m = Regex.Match(header, @".*longitude\s*=\s*([-\.0-9]+)");
            double.TryParse(m.Groups[1].ToString(), out fvalue);
            _longitude = fvalue;

            //Get the amplitude
            m = Regex.Match(header, @".*amp\s*=\s*([-\.0-9]+)");
            double.TryParse(m.Groups[1].ToString(), out fvalue);
            _amp = fvalue;

            //Get the avaerage temp
            m = Regex.Match(header, @".*tav\s*=\s*([-\.0-9]+)");
            double.TryParse(m.Groups[1].ToString(), out fvalue);
            _tav = fvalue;

            //Get the station number
            m = Regex.Match(header, @".*station number\s*=\s*([0-9]+)");
            int.TryParse(m.Groups[1].ToString(), out ivalue);
            _statNo = ivalue;

            //Get the station name 
            m = Regex.Match(header, @".*station name\s*=\s*(.*)(\n)");
            _statName = m.Groups[1].ToString();

            }
        //---------------------------------------------------------------------------   
        public override void addLocalData(List<List<double>> data, List<string> headers, Date stDate, bool adjustEndDate)
            {
            //Assume that the data is continuous and has been sanitized with dates removed
            Date end = _endDate;
            

            for (int i = 0; i < headers.Count; i++)
                {
                headers[i] = headers[i].ToUpper();
                // if it doesn't exist, add
                int varIndex = metHeaders.IndexOf(headers[i].ToUpper());
                if (varIndex == -1)
                    {
                    metHeaders.Add("TT");
                    List<double> newVals = new List<double>();
                    metData.Add(newVals);
                    }
                for (int j = 0; j < data[0].Count; j++)
                    {
                    setMetVar(headers[i], data[i][j], stDate + j);
                    }
                }

            int year, doy;
            //Add dates , ammend Pan, VP, Code
            for (int i = 0; i < data[0].Count; i++)
                {
                Date curr = stDate + i;
                curr.getDOY(out year, out doy);

                setMetVar("YEAR", year, stDate + i);
                setMetVar("DAY", doy, stDate + i);

          //      setMetVar("CODE", 88, stDate + i);

                if (headers.Contains("VP"))
                    {
                    setMetVar("VP", 0, stDate + i);
                    }
                if (headers.Contains("PAN"))
                    {
                    setMetVar("PAN", 0, stDate + i);
                    }

                }
            if (adjustEndDate || (_endDate.JDN - stDate.JDN) < data[0].Count )
                {
                _endDate = new Date(Math.Max(_endDate.JDN, (stDate.JDN + data[0].Count - 1)));
                }
            }
        //---------------------------------------------------------------------------
        public override void overlayMetFile(Date stWind, Date endWind)
            {
            int stWindYear = stWind.getYear();
            int stWindMonth = stWind.getMonth();
            int stWindDay = stWind.getDay();

            int stYear = _startDate.getYear();

            int currentYear = stYear;

            Date currentStWind = new Date(currentYear, stWindMonth, stWindDay);

            while (currentStWind < _endDate)
                {
                for (int i = 0; i <= (endWind - stWind); i++)
                    {
                    if (currentYear != stWindYear)
                        {
                        for (int j = 0; j < metHeaders.Count; j++)
                            {
                            if (!(metHeaders[j].ToUpper() == "YEAR" || metHeaders[j].ToUpper() == "DAY" || metHeaders[j].ToUpper() == "DATE"))
                                {
                                try
                                    {
                                    //Do the replacement
                                    if (metHeaders[j].ToUpper() == "CODE")
                                        {
                                        //Date d = new Date(stWind.JDN + i);
                                        //int year, doy;
                                        //d.getDOY(out year, out doy);
                                        //metData[j][currentStWind.JDN - _startDate.JDN + i] = 990000000 + year * 1000 + doy ;
                                        metData[j][currentStWind.JDN - _startDate.JDN + i] = 99;
                                        }
                                    else
                                        {
                                        metData[j][currentStWind.JDN - _startDate.JDN + i] = metData[j][stWind.JDN - _startDate.JDN + i];
                                        }
                                    }
                                catch (Exception)
                                    {
                                    //Do nothing
                                    }
                                }
                            }
                        }
                    }
                currentStWind = new Date(++currentYear, stWindMonth, stWindDay);
                }
            }

        //---------------------------------------------------------------------------   
        public override void write(string path)
            {
            StreamWriter outfile = new StreamWriter(path);

            outfile.WriteLine("[weather.met.weather]");
            outfile.WriteLine("latitude = " + latitude.ToString());
            outfile.WriteLine("longitude = " + longitude.ToString());
            outfile.WriteLine("tav = " + tav.ToString());
            outfile.WriteLine("amp = " + amp.ToString());

            StringBuilder line = new StringBuilder();

            //Give data a size for correct output
            List<int> colwidths = new List<int>();
            int dataWidth = 6;
            for (int i = 0; i < metHeaders.Count; i++)
                {
                colwidths.Add(dataWidth + 1);
                }

            colwidths[metHeaders.IndexOf("YEAR")] = 5; //4+1
            colwidths[metHeaders.IndexOf("DAY")] = 4;  //3+1

            for (int i = 0; i < metHeaders.Count; i++)
                {
                line.Append(pad(metHeaders[i], colwidths[i]));
                }

            outfile.WriteLine(line.ToString());

            line.Remove(0, line.Length);

            for (int i = 0; i < metHeaders.Count; i++)
                {
                line.Append(pad("()", colwidths[i]));
                }

            outfile.WriteLine(line.ToString());

            for (int i = 0; i < metData[0].Count; i++)
                {
                line.Remove(0, line.Length);
                for (int j = 0; j < metHeaders.Count; j++)
                    {
                    if (metHeaders[j].ToUpper() == "YEAR" || metHeaders[j].ToUpper() == "DAY" || metHeaders[j].ToUpper() == "CODE")
                        {
                        line.Append(pad(metData[j][i].ToString("0"), colwidths[j]));
                        }
                    //else if (metHeaders[j].ToUpper() == "CODE")
                    //    {
                    //    line.Append(metData[j][i].ToString("0"));
                    //    }
                    else
                        {
                        line.Append(pad(metData[j][i].ToString("0.00"), colwidths[j]));
                        }
                    }
                outfile.WriteLine(line.ToString());
                }
            outfile.Close();
            }
        //---------------------------------------------------------------------------
        string pad(string value, int size)
            {
            char[] newVal = new string(' ', size).ToCharArray();

            for (int i = value.Length - 1; i >= 0; i--)
                {
                newVal[i] = value[i];
                }
            return new string(newVal);
            }
        //---------------------------------------------------------------------------
        public override void adjust(string var, double[] months, bool absolute)
            {
            int varIndex = metHeaders.IndexOf(var.ToUpper());

            for (int i = 0; i < metData[0].Count; i++)
                {
                int year, month, day;

                (startDate + i).getYMD(out year, out month, out day);

                if (absolute)
                    {
                    metData[varIndex][i] += months[month - 1];
                    }
                else  //percentage
                    {
                    metData[varIndex][i] += metData[varIndex][i] * months[month - 1];
                    }
                }
            }
        //---------------------------------------------------------------------------
        public double tav
            {
            get { return _tav; }
            }
        //---------------------------------------------------------------------------
        public double amp
            {
            get { return _amp; }
            }
        }
    }
