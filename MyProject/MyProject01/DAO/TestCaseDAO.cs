using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MyProject01.Agent;
using MyProject01.Test;
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
    class TestEpisodeDAO
    {
        static public string CollectionName = "TestEpisode";
        static public TestEpisodeDAO GetNewDAO()
        {
            /*
            if (string.IsNullOrWhiteSpace(name) == true)
            {
                return null;
            }
            TestCaseDAO retDao = null;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoCollection<TestCaseDAO> collection = db.GetCollection<TestCaseDAO>(CollectionName);
            var query = new QueryDocument { { "TestCaseName", name } };
            var curst = collection.Find(query);
            if (curst.Count() == 0)
            {
                // not existed.
                retDao = new TestEpisodeDAO();
                retDao.TestCaseName = name;
                collection.Save(retDao);
            }
            else
            {
                retDao = curst.FirstOrDefault();
            }
            connector.Close();
            return retDao;
            */
            return new TestEpisodeDAO();
        }

        static public void SaveDao(TestCaseDAO dao)
        {
            if (dao == null)
                return;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection<TestCaseDAO> collection = db.GetCollection<TestCaseDAO>(CollectionName);
            collection.Save(dao);
            connector.Close();

        }


        private TestEpisodeDAO()
        {
            DealLogObjectList = new List<DealLogObject>();
        }
        
        public ObjectId _id;

        public ObjectId TestCaseID { set; get; }
        public int DealCount { set; get; }

        public double TotalMark { set; get; }

        public List<DealLogObject> DealLogObjectList { set; get; }


    }
    class TestCaseDAO
    {
        static private string CollectionName = "TestCases";
        static public TestCaseDAO GetDAO(string testCaseName)
        {
            if (string.IsNullOrWhiteSpace(testCaseName) == true)
            {
                return null;
            }
            TestCaseDAO retDao = null;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoCollection<TestCaseDAO> collection = db.GetCollection<TestCaseDAO>(CollectionName);
            var query = new QueryDocument { { "TestCaseName", testCaseName } };
            var curst = collection.Find(query);
            if (curst.Count() == 0)
            {
                // not existed.
                retDao = new TestCaseDAO();
                retDao.TestCaseName = testCaseName;
                collection.Save(retDao);
            }
            else
            {
                retDao = curst.FirstOrDefault();
            }
            connector.Close();
            return retDao;
        }

        static public void SaveDao(TestCaseDAO dao)
        {
            if (dao == null)
                return;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection<TestCaseDAO> collection = db.GetCollection<TestCaseDAO>(CollectionName);
            collection.Save(dao);
            connector.Close();
        }



        public ObjectId _id;
        public string TestCaseName { set; get; }
        public NetworkTestParameter NetworkParamter { set; get; }

        public byte[] NetworkData { set; get; }

        public void Save()
        {
            SaveDao(this);
        }
        public void AddEpisode(TestEpisodeDAO dao)
        {
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection<TestEpisodeDAO> collection = db.GetCollection<TestEpisodeDAO>(TestEpisodeDAO.CollectionName);
            dao.TestCaseID = this._id;
            collection.Insert(dao);
            connector.Close();
        }

        private TestCaseDAO()
        {
            // To do nothing
        }

    }
}
