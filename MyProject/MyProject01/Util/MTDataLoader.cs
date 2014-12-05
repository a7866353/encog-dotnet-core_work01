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
    abstract class BasicDataObject
    {
        public ObjectId _id;
    }

    class MtDataObject : BasicDataObject
    {
        public String Ticker { set; get; }
        public DateTime Date { set; get; }
        public double OpenPrice { set; get; }
        public double HighPrice { set; get; }
        public double LowPrice { set; get; }
        public double ClosePrice { set; get; }
        public double Volume { set; get; }
    }
    class MTDataLoader : DataLoader
    {
        static List<MTDataLoader> _loaderList;
        static MTDataLoader()
        {
            _loaderList = new List<MTDataLoader>();
        }
        static public MTDataLoader GetLoader(string tickerName)
        {
            // find saved loader
            MTDataLoader loader = null;
            foreach( MTDataLoader l in _loaderList)
            {
                if(l.TickerName == tickerName)
                {
                    loader = l;
                    break;
                }

            }
            if (loader != null)
                return loader;

            // find in db
            loader = new MTDataLoader(tickerName);
            if (loader.Count == 0)
                return null;

            _loaderList.Add(loader);
            return loader;
        }

        private string _tickerName;
        public string TickerName
        {
            get { return _tickerName; }
        }
        private MTDataLoader(string tickerName)
        {
            _tickerName = tickerName;

            MarketRateDatabaseConnector connector = new MarketRateDatabaseConnector();
            MongoDatabase db = connector.Connect();

            string collectionName = _tickerName;
            MongoCollection collection = db.GetCollection(collectionName);

            if (false)
            {
                MongoCursor cursor = collection.FindAllAs<MtDataObject>();
                cursor.BatchSize = 1000;
                RateSet currRateSet;

                foreach (MtDataObject dataObj in cursor)
                {

                    currRateSet = new RateSet(dataObj.Date, (dataObj.HighPrice + dataObj.LowPrice) / 2);
                    Add(currRateSet);
                }
            }
            else // for parallel
            {
                var collc = collection.ParallelScanAs<MtDataObject>(new ParallelScanArgs() { NumberOfCursors = 32, BatchSize = 1000 });

                Parallel.ForEach(collc, curs =>
                {
                    while (curs.MoveNext())
                    {
                        RateSet currRateSet;
                        currRateSet = new RateSet(curs.Current.Date, (curs.Current.HighPrice + curs.Current.LowPrice) / 2);
                        lock (this)
                        {
                            Add(currRateSet);
                        }
                    }
                });

            }


            connector.Close();

            SortByDate();
        }

    }
}
