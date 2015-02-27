using MongoDB.Bson;
using MongoDB.Driver;
using MyProject01.Agent;
using MyProject01.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.DAO
{
     [Serializable]
    class DealLog
    {
        public MarketActions Action { set; get; }
        public double CurrentMoney { set; get; }
    }
    [Serializable]

    class DealLogList : List<DealLog>
    {

    }
    class RateMarketTestEpisodeDAO : BasicTestEpisodeDAO
    {
        public double ResultMoney { set; get; }
        public int TrainedDealCount { set; get; }
        public int UntrainedDealCount { set; get; }
        public double TrainedDataEarnRate { set; get; }
        public double UnTrainedDataEarnRate { set; get; }
        public DealLogList DealLogs { set; get; }

        public long Step { set; get; }

        // ================
        // Network
        public int HidenNodeCount { set; get; }

        // ====================
        // Functions
        public RateMarketTestEpisodeDAO()
        {
            DealLogs = new DealLogList();
        }
    }

    class RateMarketTestDAO : BasicTestCaseDAO
    {
        public int DataBlockCount { set; get; }
        public int TestDataStartIndex { set; get; }
        public int TotalDataCount { set; get; }
        public long Step { set; get; }
        public double LastTrainedDataEarnRate { set; get; }
        public double LastTestDataEarnRate { set; get; }
        // public double[] TestData { set; get; }
        public byte[] NetworkData { set; get; }
    }
}
