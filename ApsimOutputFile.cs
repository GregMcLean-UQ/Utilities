using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Utilities
    {
    public class ApsimOutputFile
        {
        //Members--------------------------------------------------------------
        List<DateTime> _dates;
        List<List<double>> _data;
        List<string> _varNames;
        List<string> _units;

        List<string> _factors;
        List<string> _levels;
        //Methods--------------------------------------------------------------
        public ApsimOutputFile(string fileName) : this(fileName, true)
            {
            }
        //------------------------------------------------------------------
        public ApsimOutputFile(string fileName, bool fullParse)
            {
            StreamReader sr = new StreamReader(fileName);

            _data = new List<List<double>>();
            _dates = new List<DateTime>();
            _factors = new List<string>();
            _levels = new List<string>();

            sr.ReadLine(); //Get rid of the summary file line
            parseTitle(sr.ReadLine()); // (actually factors and levels)

            sr.ReadLine(); //Get rid of the title line

            _varNames = parseVarInfo(sr.ReadLine());
            _units = parseVarInfo(sr.ReadLine());
            if (fullParse == true)
                {
                while (!sr.EndOfStream)
                    {
                    _data.Add(parseData(sr.ReadLine()));
                    }
                }
            sr.Close();
            }
        //------------------------------------------------------------------
        void parseTitle(string title)
            {
            title = title.Replace(" =", "=");
            title = title.Replace("= ", "=");
            title = title.Replace("; ", ";");
            title = title.Replace(" ;", ";");

            title = title.Replace("Title=", "");
            title = title.Replace("factors=", "");

            string[] FacLevs = title.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (!FacLevs[0].Contains("="))
                {
                _factors.Add("Title");
                _levels.Add(FacLevs[0]);
                return;
                }

            foreach (string s in FacLevs)
                {
                string[] _FacLevs = s.Split('=');
                _factors.Add(_FacLevs[0]);
                _levels.Add(_FacLevs[1]);
                }
            }
        //------------------------------------------------------------------
        string[] parseLine(string line)
            {
            char[] seps = { ',', ' ' };

            string[] vals = line.Split(seps, StringSplitOptions.RemoveEmptyEntries);

            return vals;
            }
        //------------------------------------------------------------------
        List<string> parseVarInfo(string line)
            {
            string[] vals = parseLine(line);

            List<string> values = new List<string>(vals);

            return values;
            }
        //------------------------------------------------------------------
        List<double> parseData(string line)
            {
            List<double> values = new List<double>();
            string[] vals = parseLine(line);

            bool dateFound = false;
            for (int i = 0; i < vals.Length; i++)
                {
                if (!dateFound)
                    {
                    try
                        {
                        DateTime dt = Convert.ToDateTime(vals[i]);
                        _dates.Add(dt);
                        dateFound = true;
                        continue;
                        }
                    catch (Exception e)
                        {
                        string temp = e.Message;
                        }
                    }
                try
                    {
                    values.Add(double.Parse(vals[i]));
                    }
                catch (Exception e)
                    {
                    string temp = e.Message;
                    values.Add(-99);
                    }
                }

            return values;
            }
        //------------------------------------------------------------------
        public List<DateTime> varDate()
            {
            List<DateTime> vals = new List<DateTime>();
            for (int i = 0; i < _data.Count; i++)
                {
                vals.Add(_dates[i]);
                }
            return vals;
            }
        //------------------------------------------------------------------
        public List<double> varData(int index)
            {
            List<double> vals = new List<double>();
            for (int i = 0; i < _data.Count; i++)
                {
                vals.Add(_data[i][index]);
                }
            return vals;
            }
        //------------------------------------------------------------------
        public bool varExists(string varName)
            {
            if (_varNames.Contains(varName))
                {
                return true;
                }
            else
                {
                return false;
                }
            }
        //------------------------------------------------------------------

        public List<double> data(int index)
            {
            return _data[index];
            }
        //Properties--------------------------------------------------------
        public List<string> levels
            {
            get { return _levels; }
            }
        //------------------------------------------------------------------
        public List<string> factors
            {
            get { return _factors; }
            }
        //------------------------------------------------------------------
        public List<string> varNames
            {
            get { return _varNames; }
            }

        }
    }
