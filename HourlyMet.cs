using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LayerCanopyPhotosynthesis;
//using System.Threading.Tasks;
using Utilities;

namespace Utilities
{
    public class HourlyMet
    {

        private double _maxT = 25;
        private double _minT = 12;
        private int _DOY = 298;
        private double _latR;
        private double _radn = 22;
        private double _rhMax = 85;
        private double _rhMin = 30;

        private double _x = 1.8;
        private double _y = 2.2;
        private double _z = 1;

        private double _solar;
        private double _atmTransCoeff;

        private TableFunction _temps;
        private TableFunction _radns;
        private TableFunction _svps;
        private TableFunction _vpds;
        private TableFunction _rhs;

        public TableFunction temps
        {
            get { return _temps; }
        }

        public TableFunction radns
        {
            get { return _radns; }
        }

        public TableFunction svps
        {
            get { return _svps; }
        }

        public TableFunction vpds
        {
            get { return _vpds; }
        }

        public TableFunction rhs
        {
            get { return _rhs; }
        }

        public int DOY
        {
            get { return _DOY; }
            set
            {
                _DOY = value;
                doUpdate();
            }
        }

        public double atmTransCoeff
        {
            get { return _atmTransCoeff; }
            set
            {
                _atmTransCoeff = value;
            }
        }

        public double solar
        {
            get { return _solar; }
            set
            {
                _solar = value;
            }
        }

        public double latR
        {
            get { return _latR; }
            set
            {
                _latR = value;
                doUpdate();
            }
        }

        public double maxT
        {
            get { return _maxT; }
            set
            {
                _maxT = value;
                doUpdate();
            }
        }

        public double minT
        {
            get { return _minT; }
            set
            {
                _minT = value;
                doUpdate();
            }
        }

        public double minRH
        {
            get { return _rhMin; }
            set
            {
                _rhMin = value;
                doUpdate();
            }
        }

        public double maxRH
        {
            get { return _rhMax; }
            set
            {
                _rhMax = value;
                doUpdate();
            }
        }

        public double x
        {
            get { return _x; }
            set
            {
                _x = value;
                doUpdate();
            }
        }

        public double y
        {
            get { return _y; }
            set
            {
                _y = value;
                doUpdate();
            }
        }

        public double z
        {
            get { return _z; }
            set
            {
                _z = value;
                doUpdate();
            }
        }

        public double radn
        {
            get { return _radn; }
            set
            {
                _radn = value;
                doUpdate();
            }
        }

        public HourlyMet() { }

        public void run()
        {
            calcTemps();
            calcRadns();
            calcSVPs();
            calcRHs();
            calcVPDs();
            calcTransCoeff();
        }

        public virtual void doUpdate() { }
        //------------------------------------------------------------------------------------------------
        protected void calcTransCoeff()
        {

        }
        //------------------------------------------------------------------------------------------------
        protected double calcSVP(double TAir)
        {
            // calculates SVP at the air temperature
            return 6.1078 * Math.Exp(17.269 * TAir / (237.3 + TAir)) * 0.1;
        }
        //------------------------------------------------------------------------------------------------
        protected void calcSVPs()
        {
            // calculates SVP at the air temperature
            List<double> svp = new List<double>();
            List<double> time = new List<double>();

            for (int i = 0; i < 24; i++)
            {
                svp.Add(calcSVP(temps.yVals[i]));
                time.Add(i);
            }
            _svps = new TableFunction(time.ToArray(), svp.ToArray(), false);
        }
        //------------------------------------------------------------------------------------------------
        protected void calcRHs()
        {
            List<double> time = new List<double>();
            List<double> rh = new List<double>();
            // calculate relative humidity
            double wP;
            if (_rhMax < 0.0 || _rhMin < 0.0)
            {	
                wP = calcSVP(_minT) / 100 * 1000 * 90;         // svp at Tmin
            }
            else
            {
                wP = (calcSVP(_minT) / 100 * 1000 * _rhMax + calcSVP(_maxT) / 100 * 1000 * _rhMin) / 2.0;
            }

            for (int i = 0; i < 24; i++)
            {
                rh.Add(wP / (10 * svps.yVals[i]));
            }

            _rhs = new TableFunction(time.ToArray(), rh.ToArray(), false);

        }
        //------------------------------------------------------------------------------------------------
        protected void calcVPDs()
        {
            List<double> time = new List<double>();
            List<double> vpd = new List<double>();
            for (int i = 0; i < 24; i++)
            {
                ////return 6.1078 * Math.Exp(17.269 * TAir / (237.3 + TAir)) * 0.1;
                //vpd.Add(0.6106 * (Math.Exp((17.27 * temps.yVals[i]) / (temps.yVals[i] + 237.3)) - rhs.yVals[i] / 100 * Math.Exp((17.27 * temps.yVals[i]) / (temps.yVals[i] + 237.3))));
                //time.Add(i);

                //AD - Have chacked. This formula is equivalent to above
                vpd.Add((1-(rhs.yVals[i] / 100)) * svps.yVals[i]);
                time.Add(i);
            }

            _vpds = new TableFunction(time.ToArray(), vpd.ToArray(), false);
        }
        //------------------------------------------------------------------------------------------------
        protected double CalcSolarDeclination(int doy)
        {
            return (23.45 * (Math.PI / 180)) * Math.Sin(2 * Math.PI * (284.0 + doy) / 365.0);
        }
        ////------------------------------------------------------------------------------------------------
        protected double CalcDayLength(double LatR, double SolarDec)
        {
            return Math.Acos(-Math.Tan(LatR) * Math.Tan(SolarDec));
        }
        ////------------------------------------------------------------------------------------------------
        protected double CalcSolarRadn(double RATIO, double DayL, double LatR, double SolarDec) // solar radiation
        {
            return (24.0 * 3600.0 * 1360.0 * (DayL * Math.Sin(LatR) * Math.Sin(SolarDec) +
                     Math.Cos(LatR) * Math.Cos(SolarDec) * Math.Sin(DayL)) / (Math.PI * 1000000.0)) * RATIO;
        }
        ////------------------------------------------------------------------------------------------------
        protected double GlobalRadiation(double oTime, double latitude, double SolarDec, double DayLH, double Solar)
        {
            double Alpha = Math.Asin(Math.Sin(latitude) * Math.Sin(SolarDec) +
                  Math.Cos(latitude) * Math.Cos(SolarDec) * Math.Cos((Math.PI / 12.0) * DayLH * (oTime - 0.5))); //global variable
            double ITot = Solar * (1.0 + Math.Sin(2.0 * Math.PI * oTime + 1.5 * Math.PI)) / (DayLH * 60.0 * 60.0); //global variable
            double IDiff = 0.17 * 1360.0 * Math.Sin(Alpha) / 1000000.0; //global variable
            if (IDiff > ITot)
            {
                IDiff = ITot;
            }
            return ITot - IDiff; //global variable
        }

        //------------------------------------------------------------------------------------------------
        public void calcTemps()
        {
            List<double> hours = new List<double>();
            List<double> temps = new List<double>();

            Angle aDelt = new Angle(23.45*Math.Sin(2*Math.PI*(284+_DOY)/365), AngleType.Deg);
            Angle temp2 = new Angle(Math.Acos(-Math.Tan(_latR) * Math.Tan(aDelt.rad)), AngleType.Rad);
            
            double ady = (temp2.deg / 15) * 2; 
            double ani = (24.0 - ady);                     // night hours
            // determine if the hour is during the day or night
            double bb = 12.0 - ady / 2.0 + z;              // corrected dawn
            double be = 12.0 + ady / 2.0;                  // sundown

            for (int hr = 1; hr <= 24; hr++)
            {
                double bt = hr;
                double bbd = 0;
                double temperature;
                if (bt >= bb && bt < be)         //day
                {
                    bbd = bt - bb;
                    temperature = (maxT - minT) * Math.Sin((3.14 * bbd) / (ady + 2 * x)) + minT;
                }
                else                             // night
                {
                    if (bt > be)
                    {
                        bbd = bt - be;
                    }
                    if (bt < bb)
                    {
                        bbd = (24.0 - be) + bt;
                    }
                    double ddy = ady - z;
                    double tsn = (maxT - minT) * Math.Sin((3.14 * ddy) / (ady + 2 * x)) + minT;
                    temperature = minT + (tsn - minT) * Math.Exp(-y * bbd / ani);
                }
                hours.Add(hr - 1);
                temps.Add(temperature);
            }
            _temps = new TableFunction(hours.ToArray(), temps.ToArray(), false);
        }
        //------------------------------------------------------------------------------------------------
        protected void calcRadns()
        {
            double[] rad = new double[24];
            // some constants
            //double RATIO = 0.75; //Hammer, Wright (Aust. J. Agric. Res., 1994, 45)

            // some calculations
            double SolarDec = CalcSolarDeclination(_DOY);
            double DayL = CalcDayLength(_latR, SolarDec);            // day length (radians)
            double DayLH = (2.0 / 15.0 * DayL) * (180 / Math.PI);             // day length (hours)
            //_solar = CalcSolarRadn(RATIO, DayL, _latR, SolarDec); // solar radiation
            _solar = CalcSolarRadn(1.0, DayL, _latR, SolarDec); // solar radiation

            _atmTransCoeff = _radn / _solar;

            //do the radiation calculation zeroing at dawn
            for (int i = 0; i < 24; i++)
            {
                rad[i] = 0.0;
            }

            double DuskDawnFract = (DayLH - (int)DayLH) / 2; //the remainder part of the hour at dusk and dawn
            double DawnTime = 12 - (DayLH / 2);

            //   DawnTime = (180 - RadToDeg(acos(-1 * tan(LatR) * tan(SolarDec)))) / 360 * 24; //Wikipedia ???

            //The first partial hour
            rad[(int)DawnTime] += (GlobalRadiation(DuskDawnFract / DayLH, _latR, SolarDec, DayLH, _solar) * 3600 * DuskDawnFract);

            //Add the next lot
            for (int i = 0; i < (int)DayLH - 1; i++)
            {
                rad[(int)DawnTime + i + 1] += (GlobalRadiation((DuskDawnFract / DayLH) + ((double)(i + 1) * 1.0 / (double)((int)DayLH)), _latR, SolarDec, DayLH, _solar) * 3600);
            }

            //Add the last one
            rad[(int)DawnTime + (int)DayLH + 1] += (GlobalRadiation(1, _latR, SolarDec, DayLH, _solar) * 3600 * DuskDawnFract);

            double TotalRad = 0;
            for (int i = 0; i < 24; i++)
            {
                TotalRad += rad[i];
            }

            for (int i = 0; i < 24; i++)
            {
                rad[i] = rad[i] / TotalRad * _radn;
            }
        }
    }
}
