using MongoDB.Driver;
using MyProject01.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.DAO
{
    class RateMarketTestEpisodeDAO : BasicTestEpisodeDAO
    {
        public int EpisodeNumber { set; get; }
    }
    class RateMarketTestDAO : BasicTestCaseDAO
    {
        public NetworkTestParameter NetworkParamter { set; get; }
        public byte[] NetworkData { set; get; }

        public long Step { set; get; }
    }
}
