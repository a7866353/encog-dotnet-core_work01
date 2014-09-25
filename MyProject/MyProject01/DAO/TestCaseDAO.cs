using MongoDB.Bson;
using MongoDB.Driver;
using MyProject01.Agent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.DAO
{
    class NetworkParamterObject
    {
        public string NetworkName { set; get; }
    }

    class DealLogObject
    {
        public int Index { set; get; }
        public MarketActions Action { set; get; }
    }
    class TestEpisodeOjbect
    {
        public int DealCount { set; get; }

        public double TotalMark { set; get; }

        public List<DealLogObject> DealLogObjectList { set; get; }


    }
    class TestCaseDAO
    {
        public ObjectId _id;

        public NetworkParamterObject NetworkParamter { set; get; }

        public byte[] NetworkData { set; get; }

        public List<TestEpisodeOjbect> TestEpisodeObjectList { set; get; }

    }
}
