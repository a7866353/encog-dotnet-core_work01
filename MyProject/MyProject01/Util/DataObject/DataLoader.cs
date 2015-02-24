using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using MongoDB.Driver;
using MongoDB.Bson;
using MyProject01.DAO;

namespace MyProject01.Util
{
    class MarketData
    {
        public ObjectId _id;
        public string Data { get; set; }
        public string MiddleRate { get; set; }

    }

     class DataProvider
     {
         public static MarketData[] GetAllMarketData()
         {
             MarketRateDatabaseConnector connector = new MarketRateDatabaseConnector();
             MongoDatabase db = connector.Connect();

             string collectionName = "MiddleRate";
             MongoCollection collection = db.GetCollection(collectionName);
              
             MongoCursor cursor = collection.FindAllAs<MarketData>();
             MarketData[] dataArr = new MarketData[cursor.Count()];

             long index = 0;
             foreach (MarketData dataObj in cursor)
             {
                 dataArr[index++] = dataObj;
             }
             connector.Close();
             return dataArr;
         }
     }

    public class RateSet
    {
        public DateTime Date;
        public double Value;
        public double RealValue;
        public RateSet(DateTime date, double value)
        {
            this.Date = date;
            this.Value = value;
            this.RealValue = value;
        }
        public override string ToString()
        {
            return Date.ToShortDateString() + ": " + Value.ToString();
        }
        public RateSet Clone()
        {
            return (RateSet)MemberwiseClone();
        }
    }
    public abstract class DataLoader : List<RateSet>
    {
        private double _dataMaxValue;
        private double _dataMinValue;
        private double _dataOffset;
        private double _dataScale;

        private const double _targetDataMargin = 0.25;
        private const double _targDataMax = 0 + _targetDataMargin;
        private const double _targDataMin = 0 - _targetDataMargin;

        public double Offset
        {
            get { return _dataOffset; }
        }
        public double Scale
        {
            get { return _dataScale; }
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

        public double[] GetArr(int startIndex, int length)
        {

            if ((startIndex + length) > this.Count)
                //     length = this.Count - startIndex;
                return null;
            double[] res = new double[length];
            for (int i = 0; i < length; i++)
            {
                res[i] = this[startIndex + i].Value;
            }
            return res;
        }

        public RateDataBlock CreateDataBlock(int startIndex, int length, int blockLength)
        {
            return new RateDataBlock(this, startIndex, length, blockLength);
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

        public void Normallize(double offset, double scale)
        {
            _dataOffset = offset;
            _dataScale = scale;
            foreach (RateSet data in this)
            {
                data.Value = DataConv(data.RealValue, _dataOffset, _dataScale);
            }
        }
        public void Fillter(DateTime startTime, DateTime endTime)
        {
            int startIndex = -1;
            int stopIndex = -1;

            for(int idx=0;idx<this.Count;idx++)
            {
                if(startIndex == -1)
                {
                    if (this[idx].Date >= startTime)
                        startIndex = idx;
                }
                if(stopIndex == -1)
                {
                    if (this[idx].Date > endTime)
                        stopIndex = idx;
                }
                if(stopIndex != -1 && startIndex != -1)
                    break;
            }

            
            if( stopIndex != -1)
            {
                // Remove after
                this.RemoveRange(stopIndex, this.Count - stopIndex);
            }
            if( startIndex != -1)
            {
                // Remove before
                this.RemoveRange(0, startIndex);
            }

        }

        protected void SortByDate()
        {
            Sort(new DateComparer(true));
        }

        protected void DataValueAdjust()
        {
            _dataMaxValue = _dataMinValue = this[0].Value;
            foreach (RateSet data in this)
            {
                if (data.Value > _dataMaxValue)
                    _dataMaxValue = data.Value;
                else if (data.Value < _dataMinValue)
                    _dataMinValue = data.Value;
            }

            _dataScale = (_targDataMax - _targDataMin) / (_dataMaxValue - _dataMinValue);

            _dataOffset = (_dataMaxValue - _dataMinValue) * _targDataMax / (_targDataMax - _targDataMin) - _dataMaxValue;
            

            Normallize(_dataOffset, _dataScale);
        }

        private double DataConv(double value, double offset, double scale)
        {
            return (value + offset) * scale;
        }

        private int SerachByDate(DateTime date)
        {
            return BinarySearch(new RateSet(date, 0), new DateComparer(true));
        }
    }
    // Compare By Date
    class DateComparer : IComparer<RateSet>  
    {
        private bool _isIncr = false;

        public DateComparer(bool isIncr)
        {
            _isIncr = isIncr;
        }
        public int Compare(RateSet x, RateSet y)
        {
            if (_isIncr)
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
