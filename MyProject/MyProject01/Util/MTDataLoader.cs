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
    }
    class MTDataLoader : DataLoader
    {
        private string _tickerName;
        public string TickerName
        {
            get { return _tickerName; }
        }
        public MTDataLoader(string tickerName, DataTimeType type = DataTimeType.None)
        {
            _tickerName = tickerName;

            MTDataBuffer dataBuffer = MTDataBuffer.GetLoader(_tickerName);
            if( type == DataTimeType.None )
                AddAll(dataBuffer);
            else
            {
                AddByTime(dataBuffer, (int)type);
            }
        }

        private void AddAll(MTDataBuffer buffer)
        {
            foreach(MtDataObject mtData in buffer)
            {
                RateSet currRateSet;
                currRateSet = new RateSet(mtData.Date, (mtData.HighPrice + mtData.LowPrice) / 2);
                Add(currRateSet);

            }
        }
        private void AddByTime(MTDataBuffer buffer, int min)
        {

        }

    }
}
