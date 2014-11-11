using MongoDB.Bson;
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
        public int DataBlockCount { set; get; }
        public int TestDataStartIndex { set; get; }
        public int TotalDataCount { set; get; }
        public long Step { set; get; }
        public double LastTrainedDataEarnRate { set; get; }
        public double LastTestDataEarnRate { set; get; }
        public double[] TestData { set; get; }
        public byte[] NetworkData { set; get; }
    }
}
