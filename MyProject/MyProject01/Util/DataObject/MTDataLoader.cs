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
        // M1 = 1,
        M5 = 5,
        M10 = 10,
        M30 = 30,
        H1 = 60,
        // H2 = 120,
        // H3 = 180, 
        // H4 = 240,
        D1 = 1440,
        // W1 = 10080,
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
                case DataTimeType.M10:
                    countLimit *= 10;
                    break;
                    /*
                case DataTimeType.M1:
                    countLimit *= 1;
                    break;
                     */
                case DataTimeType.M5:
                    countLimit *= 5;
                    break;
                case DataTimeType.D1:
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
    abstract class BasicTestDataLoader : DataLoader
    {
        private string _tickerName;
        private DataTimeType _type;
        protected int _dir = 1;
        public string TickerName
        {
            get { return _tickerName; }
        }


        public BasicTestDataLoader(string tickerName, DataTimeType type)
        {
            _tickerName = tickerName;
            _type = type;
        }

        abstract protected MongoCursor<MtDataObject> GetData(MongoCollection collection);
        abstract protected bool CheckEnd(RateSet obj);
        private void AddByTime(MongoCursor<MtDataObject> buffer, int interval)
        {
            DateCheck checker = new DateCheck();
            bool isFirst = true;

            checker.Interval = _dir * interval;
            RateSet currRateSet;

            foreach (MtDataObject mtData in buffer)
            {
                if (isFirst == true)
                {
                    isFirst = false;
                    checker.Set(mtData);
                }
                else
                {
                    if (checker.IsOver(mtData.Date) == true)
                    {
                        currRateSet = new RateSet(checker.startDate, (checker.highPrice + checker.lowPrice) / 2);
                        if (CheckEnd(currRateSet) == true)
                            break;
                        checker.Set(mtData);
                    }
                    else
                    {
                        checker.Add(mtData);
                    }
                }

            }
        }

        public void Load()
        {
            MarketRateDatabaseConnector connector = new MarketRateDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection collection = db.GetCollection(_tickerName);
            MongoCursor<MtDataObject> curs = GetData(collection);

            AddByTime(curs, (int)_type);

            connector.Close();

            SortByDate();
            // DataValueAdjust();
        }



    }
    class TestDataCountLoader : BasicTestDataLoader
    {
        private DateTime _endDate;
        private int _dataCountLimit;
        public TestDataCountLoader(string tickerName, DataTimeType type, DateTime endDate, int count)
            : base(tickerName, type)
        {
            _dir = -1;
            _dataCountLimit = count;
            _endDate = endDate;
        }
  
        protected override MongoCursor<MtDataObject> GetData(MongoCollection collection)
        {
            // Add by date limit
            QueryDocument query = new QueryDocument();
            BsonDocument b = new BsonDocument();
            b.Add("$lt", _endDate);
            query.Add("Date", b);

            SortByDocument sd = new SortByDocument();
            sd.Add("_id", -1);

            var curst = collection.FindAs<MtDataObject>(query);
            curst.BatchSize = 1000;
            curst.SetSortOrder(sd);
            // curst.SetLimit(count);  // set limit
            return curst;
        }

        protected override bool CheckEnd(RateSet obj)
        {
            Add(obj);
            if (Count >= _dataCountLimit)
                return true;
            else
                return false;
        }
    }

    class TestDataDateRangeLoader : BasicTestDataLoader
    {
        private DateTime _startDate;
        private DateTime _endDate;
        private int _preCount;
        private int _addCount;

        public TestDataDateRangeLoader(string tickerName, DataTimeType type, DateTime startDate, DateTime endDate, int preCount)
            : base(tickerName, type)
        {
            _dir = -1;
            _preCount = preCount;
            _addCount = 0;
            _startDate = startDate;
            _endDate = endDate;
        }

        protected override MongoCursor<MtDataObject> GetData(MongoCollection collection)
        {
            // Add by date limit
            QueryDocument query = new QueryDocument();
            BsonDocument b = new BsonDocument();
            b.Add("$lt", _endDate);
            query.Add("Date", b);

            SortByDocument sd = new SortByDocument();
            sd.Add("_id", -1);

            var curst = collection.FindAs<MtDataObject>(query);
            curst.BatchSize = 1000;
            curst.SetSortOrder(sd);
            // curst.SetLimit(count);  // set limit
            return curst;        }

        protected override bool CheckEnd(RateSet obj)
        {
            if (obj.Date >= _startDate)
            {
                Add(obj);
                return false;
            }
            else
            {
                Add(obj);
                _addCount++;
                if (_addCount >= _preCount)
                    return true;
                else
                    return false;
            }
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
            if (Interval >= 0)
            {
                if (date >= targetTime)
                    return true;

                else
                    return false;
            }
            else
            {
                if (date <= targetTime)
                    return true;
                else
                    return false;
            }
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
