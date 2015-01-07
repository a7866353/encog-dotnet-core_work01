using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketTestClient.RateDataController
{
    class RateDataDAO
    {

        public String Ticker { set; get; }
        public DateTime Date { set; get; }
        public double OpenPrice { set; get; }
        public double HighPrice { set; get; }
        public double LowPrice { set; get; }
        public double ClosePrice { set; get; }
        public double Volume { set; get; }

    }
}
