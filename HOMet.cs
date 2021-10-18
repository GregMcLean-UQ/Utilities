using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Utilities
    {
    public class HOMet : Met
        {
        //---------------------------------------------------------------------------
        // Description: Default Constructor       
        //---------------------------------------------------------------------------
        public HOMet()
            {
            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a Met class with a met 
        //                            filename.
        //---------------------------------------------------------------------------
        public HOMet(string fileName)
            : base(fileName)
            {
            }
        //---------------------------------------------------------------------------
        // Description: Constructor - Creates an instance of a Met class with a met 
        //                            filename.
        //---------------------------------------------------------------------------
        public HOMet(Met met)
            : base(met)
            {
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
        public override void write(string path)
            {
            throw new Exception("The method or operation is not implemented.");
            }
        //---------------------------------------------------------------------------
        public override void getAtmospherics(Atmospheric atmos, Date date)
            {
            throw new Exception("The method or operation is not implemented.");
            }
        //---------------------------------------------------------------------------
        public override void getAtmosphericsOverlay(Atmospheric atmos, Date stWind, Date endWind, Date date)
            {
            throw new Exception("The method or operation is not implemented.");
            }
        //---------------------------------------------------------------------------
        protected override void setStartDate()
            {
            int year;
            int doy;

            year = (int)metData[metHeaders.IndexOf("YEAR")][0];
            doy = (int)metData[metHeaders.IndexOf("DOY")][0];

            _startDate = new Date(year, doy);
            _endDate = _startDate + metData[0].Count - 1;
            }
        //---------------------------------------------------------------------------
        protected override void setStationInfo(string line)
            {
            char[] charSeparators = { ' ' };

            String[] words = line.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);

            _statName = words[0];
            _statNo = int.Parse(words[1]);
            }
        //---------------------------------------------------------------------------
        protected override void parseMet(StreamReader sr)
            {
            setStationInfo(sr.ReadLine());

            metHeaders.Add("YEAR");
            metHeaders.Add("DOY");
            metHeaders.Add("RAIN");

            for (int i = 0; i < metHeaders.Count; i++)
                {
                metData.Add(new List<double>());
                }

            int currYear = 0;
            int currMonth = 0;
            int currDay = 0;

            Date currDate = null;

            while (!sr.EndOfStream)
                {
                int dataDoy;
                int dataYear;

                String line = sr.ReadLine();

                if (line != "")
                    {
                    //Parse each line
                    int? dayVal = null;
                    int? monthVal = null;
                    int? yearVal = null;

                    int rainVal = int.Parse(line.Substring(0, 3));  //Rain will allways be the first 3 chars;

                    if (line.Length > 3)
                        {
                        dayVal = int.Parse(line.Substring(3, 2));
                        }

                    if (line.Length > 5)
                        {
                        monthVal = int.Parse(line.Substring(5, 2));
                        }

                    if (line.Length > 7)
                        {
                        yearVal = int.Parse(line.Substring(7, 4));
                        }

                    if (currDay == 0)    //Initial date has not been set
                        {
                        //Make sure there ar values in the date holders
                        if (!dayVal.HasValue || !monthVal.HasValue || !yearVal.HasValue)
                            {
                            throw (new Exception("File Start Date Error"));
                            }
                        currDate = new Date(yearVal.Value, monthVal.Value, dayVal.Value);
                        currDate.getDOY(out dataYear, out dataDoy);

                        metData[0].Add(dataYear);
                        metData[1].Add(dataDoy);
                        metData[2].Add(rainVal);

                        currYear = yearVal.Value;
                        currMonth = monthVal.Value;
                        currDay = dayVal.Value;
                        }
                    else
                        {
                        Date newDate;
                        int newYear;
                        int newMonth;
                        int newDay;

                        if (!yearVal.HasValue)
                            {
                            newYear = currYear;
                            }
                        else
                            {
                            newYear = yearVal.Value;
                            }

                        if (!monthVal.HasValue)
                            {
                            newMonth = currMonth;
                            }
                        else
                            {
                            newMonth = monthVal.Value;
                            }

                        if (!dayVal.HasValue)
                            {
                            newDay = currDay;
                            }
                        else
                            {
                            newDay = dayVal.Value;
                            }

                        newDate = new Date(newYear, newMonth, newDay);

                        for (int i = currDate.JDN + 1; i < newDate.JDN; i++)
                            {
                            Date tempDate = new Date(i);

                            tempDate.getDOY(out dataYear, out dataDoy);

                            metData[0].Add(dataYear);
                            metData[1].Add(dataDoy);
                            metData[2].Add(0);
                            }

                        //Add the existing data
                        newDate.getDOY(out dataYear, out dataDoy);

                        metData[0].Add(dataYear);
                        metData[1].Add(dataDoy);
                        metData[2].Add(rainVal);

                        //Update the date trackers
                        currDate = newDate;
                        currYear = newYear;
                        currMonth = newMonth;
                        currDay = newDay;
                        }
                    }
                }
            }
        //---------------------------------------------------------------------------
        public override void adjust(string var, double[] months, bool absolute)
            {
            throw new Exception("The method or operation is not implemented.");
            }
        }
    }
