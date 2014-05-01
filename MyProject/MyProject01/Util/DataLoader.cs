﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace MyProject01.Util
{
    class RateSet
    {
        public DateTime Date;
        public double Value;
        public RateSet(DateTime date, double value)
        {
            this.Date = date;
            this.Value = value;
        }
        public override string ToString()
        {
            return Date.ToShortDateString() + ": " + Value.ToString();
        }
    }
    class DataLoader : List<RateSet>
    {
        private double _dataMaxValue;
        private double _dataMinValue;
        private double _dataOffset;
        private double _dataRadio;

        private const double _targDataMax = 1.0;
        private const double _targDataMin = 0.0;

        public DataLoader(string path)
        {
            StreamReader sr = new StreamReader(path);
            string str = null;
            RateSet currRateSet;
            // read title
            str = sr.ReadLine();
            while (true)
            {
                str = sr.ReadLine();
                if (str == null)
                    break;

                string[] strArr = str.Split(',');
                // Data Check
                if( string.IsNullOrWhiteSpace(strArr[0]) == true )
                    continue;
                if( string.IsNullOrWhiteSpace(strArr[1]) == true )
                    continue;
                currRateSet = new RateSet(DateTime.Parse(strArr[0]), double.Parse(strArr[1]));
                Add(currRateSet);
            }
            sr.Close();
            DataNormalize();
        }
        public double[] GetArr(DateTime date)
        {
            int index = SerachByDate(date);
            if (index >= 0)
            {
                double[] valueArr = new double[1];
                valueArr[0] = this[index].Value;
                return valueArr;
            }
            else
                return null;

        }
        public double[] GetArr(DateTime startDate, int days)
        {
            return GetArr(startDate, startDate.AddDays(days));
        }

        public double[] GetArr(DateTime startDate, DateTime endDate)
        {
            if( endDate < startDate)
                return null;

            List<double> valueList = new List<double>();
            int index=0;
            while(startDate < endDate)
            {
                index = SerachByDate(startDate);
                if(index >= 0)
                {
                    valueList.Add(this[index].Value);
                }
                startDate = startDate.AddDays(1);
            }
            return valueList.ToArray();
        }
        public void SortByDate()
        {
            Sort(new DateComparer());
        }



        private void DataNormalize()
        {
            SortByDate();
            DataValueAdjust();
        }
        private void DataValueAdjust()
        {
            _dataMaxValue = _dataMinValue = this[0].Value;
            foreach (RateSet data in this)
            {
                if (data.Value > _dataMaxValue)
                    _dataMaxValue = data.Value;
                else if (data.Value < _dataMinValue)
                    _dataMinValue = data.Value;
            }

            _dataRadio = (_dataMaxValue - _dataMinValue) / (_targDataMax - _targDataMin);
            _dataOffset = _dataMinValue - _targDataMin;

            foreach (RateSet data in this)
            {
                data.Value = DataConv(data.Value,_dataOffset, _dataRadio);
            }


        }
        private double DataConv(double value, double offset, double radio)
        {
            return (value - offset) / radio;
        }
        private int SerachByDate(DateTime date)
        {
            return BinarySearch(new RateSet(date, 0), new DateComparer());
        }
    }
    // Compare By Date
    class DateComparer : IComparer<RateSet>  
    {
        public int Compare(RateSet x, RateSet y)
        {
            if (true)
            {
                if (x.Date > y.Date)
                    return 1;
                else if (x.Date == y.Date)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (x.Date < y.Date)
                    return 1;
                else if (x.Date == y.Date)
                    return 0;
                else
                    return -1;
            }

        }
    }

}