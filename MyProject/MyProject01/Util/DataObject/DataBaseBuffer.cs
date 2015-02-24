using MongoDB.Bson;
using MongoDB.Driver;
using MyProject01.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

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

    class MtDateComparer : IComparer<MtDataObject>  
    {
        private bool _isIncr = false;

        public MtDateComparer()
        {
        }
        public int Compare(MtDataObject x, MtDataObject y)
        {
            if (x.Date > y.Date)
                return 1;
            else if (x.Date == y.Date)
                return 0;
            else
                return -1;
        }
    }
    class MTDataBuffer : List<MtDataObject>
    {
        static List<MTDataBuffer> _loaderList;
        static Semaphore _getLock;
        static MTDataBuffer()
        {
            _loaderList = new List<MTDataBuffer>();
        }
        static public MTDataBuffer GetLoader(string tickerName)
        {
            // find saved loader
            MTDataBuffer loader = null;
            foreach (MTDataBuffer l in _loaderList)
            {
                if (l.TickerName == tickerName)
                {
                    loader = l;
                    break;
                }

            }
            if (loader != null)
                return loader;

            // find in db
            loader = new MTDataBuffer(tickerName);
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
        private MTDataBuffer(string tickerName)
        {
            _tickerName = tickerName;

            MarketRateDatabaseConnector connector = new MarketRateDatabaseConnector();
            MongoDatabase db = connector.Connect();

            string collectionName = _tickerName;
            MongoCollection collection = db.GetCollection(collectionName);

            if (true)
            {
                /*
                MongoCursor cursor = collection.FindAllAs<MtDataObject>();
                cursor.BatchSize = 1000;
                int loadMaxCount = 200000;
                foreach (MtDataObject dataObj in cursor)
                {
                    Add(dataObj);
                    loadMaxCount--;
                    if (loadMaxCount == 0)
                        break;
                }
                */
                QueryDocument query = new QueryDocument();
                BsonDocument b = new BsonDocument();
                b.Add("$gt", DateTime.Now.AddMonths(-6));
                b.Add("$lt", DateTime.Now);
                query.Add("Date", b);

                var curst = collection.FindAs<MtDataObject>(query);
                curst.BatchSize = 1000;
                foreach (MtDataObject dataObj in curst)
                {
                    Add(dataObj);
                }

            }
            else // for parallel
            {
                var collc = collection.ParallelScanAs<MtDataObject>(new ParallelScanArgs() { NumberOfCursors = 128, BatchSize = 1000 });

                Parallel.ForEach(collc, curs =>
                {
                    while (curs.MoveNext())
                    {
                        lock (this)
                        {
                            Add(curs.Current);
                        }
                    }
                });

            }
            connector.Close();
            SortByDate();
        }
               
        private void SortByDate()
        {
            Sort(new MtDateComparer());
        }
    }
}
