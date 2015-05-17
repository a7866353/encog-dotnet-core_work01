using MongoDB.Bson;
using MongoDB.Driver;
using MyProject01.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.Util
{
    enum DataTimeType
    {
        None = 0,
        Time1Min = 1,
        Time5Min = 5,
        Time10Min = 10,
        Timer1Day = 24*60,
    }
    class MTDataLoader : DataLoader
    {
        private string _tickerName;
        private int _dataCountLimit = 500;
        public string TickerName
        {
            get { return _tickerName; }
        }
        public MTDataLoader(string tickerName, DataTimeType type = DataTimeType.None)
        {
            _tickerName = tickerName;
            int countLimit = _dataCountLimit;
            switch(type)
            {
                case DataTimeType.None:
                    break;
                case DataTimeType.Time10Min:
                    countLimit *= 10;
                    break;
                case DataTimeType.Time1Min:
                    countLimit *= 1;
                    break;
                case DataTimeType.Time5Min:
                    countLimit *= 5;
                    break;
                case DataTimeType.Timer1Day:
                    countLimit *= 60 * 24;
                    break;
                default:
                    break;
            }
            MTDataBuffer dataBuffer = MTDataBuffer.GetLoader(_tickerName, countLimit);
            if( type == DataTimeType.None )
                AddAll(dataBuffer);
            else
            {
                AddByTime(dataBuffer, (int)type);
            }

            DataValueAdjust();
        }

        private void AddAll(MTDataBuffer buffer)
        {
            foreach(MtDataObject mtData in buffer)
            {
                RateSet currRateSet;
                currRateSet = new RateSet(mtData.Date, (mtData.HighPrice + mtData.LowPrice) / 2);
                Add(currRateSet);
                if (Count >= _dataCountLimit)
                    break;

            }
        }
        private void AddByTime(MTDataBuffer buffer, int interval)
        {
            DateCheck checker = new DateCheck();

            checker.Interval = interval;
            checker.Set(buffer[0]);
            RateSet currRateSet;

            foreach (MtDataObject mtData in buffer)
            {
                if (checker.IsOver(mtData.Date) == true)
                {
                    currRateSet = new RateSet(checker.startDate, (checker.highPrice + checker.lowPrice) / 2);
                    Add(currRateSet);

                    checker.Set(mtData);

                    if (Count >= _dataCountLimit-1)
                        break;
                }
                else
                {
                    checker.Add(mtData);
                }

            }

            // add last
            currRateSet = new RateSet(checker.startDate, (checker.highPrice + checker.lowPrice) / 2);
            Add(currRateSet);
        }



    }

    class DateCheck
    {
        public double highPrice;
        public double lowPrice;
        public DateTime startDate;
        public DateTime targetTime;

        public int Interval;

        public void Set(MtDataObject mtData)
        {
            highPrice = mtData.HighPrice;
            lowPrice = mtData.LowPrice;
            startDate = mtData.Date;
            targetTime = CalcEndTime(startDate, Interval);
             
        }

        public bool IsOver(DateTime date)
        {
            if (date >= targetTime)
                return true;

            else
                return false;
        }

        public void Add(MtDataObject mtData)
        {
            if (highPrice < mtData.HighPrice)
                highPrice = mtData.HighPrice;
            if (lowPrice > mtData.LowPrice)
                lowPrice = mtData.LowPrice;
        }

        private DateTime CalcEndTime(DateTime date, int interval)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;
            int hour = date.Hour;
            int min = date.Minute;

            min = min / interval * interval;
            DateTime res ;
            try
            {
                res = new DateTime(year, month, day, hour, min, 0, 0);
                res = res.AddMinutes(interval);
            }catch(Exception e)
            {
                throw (e);
            }
            return res;
        }
    }
}
