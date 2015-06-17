using MyProject01.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter
{
    /*
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
    */
    class MTDataImporter : BasicImporter
    {
        private string _ticker;
        private int _skipLine;
        private int _skipLineCount;

        public MTDataImporter(string ticker, int skipLine)
        {
            _ticker = ticker;
            _skipLine = skipLine;
            _skipLineCount = 0;
        }

        protected override string CollectiongName
        {
            get { return _ticker; }
        }

        protected override BasicDataObject GetNextObject(string lineString)
        {
            if( _skipLineCount < _skipLine )
            {
                _skipLineCount++;
                return null;
            }


            char spiltMark = ',';
            string[] strArr = lineString.Split(spiltMark);

            RateData obj = new RateData();
            
            int year = int.Parse( strArr[1].Substring(0, 4) );
            int mounth = int.Parse( strArr[1].Substring(4, 2) );
            int day = int.Parse( strArr[1].Substring(6,2) );
            int hour = int.Parse( strArr[2].Substring(0,2) );
            int min = int.Parse( strArr[2].Substring(2,2) );
            int second = int.Parse( strArr[2].Substring(4, 2) );
            DateTime date = new DateTime(year, mounth, day, hour, min, second);
            obj.time = date;

            obj.open = double.Parse(strArr[3]);
            obj.high = double.Parse(strArr[4]);
            obj.low = double.Parse(strArr[5]);
            obj.close = double.Parse(strArr[6]);
            obj.tick_volume = int.Parse(strArr[7]);

            return obj;
        }
    }
}
