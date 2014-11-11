using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MyProject01.Agent;
using MyProject01.Test;
using MyProject01.TestCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.DAO
{
    class BasicTestEpisodeDAO
    {
        static public BasicTestEpisodeDAO GetNewDAO()
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
            return new BasicTestEpisodeDAO();
        }

        static public void SaveDao(BasicTestCaseDAO dao)
        {
            if (dao == null)
                return;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection<BasicTestCaseDAO> collection = db.GetCollection<BasicTestCaseDAO>(BasicTestCaseDAO.EpisodeCollectionName);
            collection.Save(dao);
            connector.Close();

        }

      
        public ObjectId _id;

        public ObjectId TestCaseID { set; get; }

    }
    abstract class BasicTestCaseDAO
    {
        #region Static Region

        static public string CollectionName = "TestCases";
        static public string EpisodeCollectionName = "TestEpisode";
        static public T GetDAO<T>(string testCaseName, bool isNew = false) 
            where T : BasicTestCaseDAO, new()
        {
            if (string.IsNullOrWhiteSpace(testCaseName) == true)
            {
                return null;
            }
            if (isNew == true)
            {
                if (RateMarketTestDAO.IsExist(testCaseName) == true)
                    RateMarketTestDAO.Remove(testCaseName);
            }
            T retDao;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoCollection<T> collection = db.GetCollection<T>(CollectionName);
            var query = new QueryDocument { { "TestCaseName", testCaseName } };
            var curst = collection.Find(query);
            if (curst.Count() == 0)
            {
                // not existed.
                retDao = new T();
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
        static public T[] GetAllDAOs<T>()
            where T : BasicTestCaseDAO, new()
        {
           
            T[] retDaoArr = null;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoCollection<T> collection = db.GetCollection<T>(CollectionName);
            var curst = collection.FindAll();
            long resultCount = curst.Count();
            if (resultCount != 0)
            {
                retDaoArr = new T[resultCount];
                int index = 0;
                foreach( T obj in curst)
                {
                    retDaoArr[index++] = obj;
                }
            }
            connector.Close();
            return retDaoArr;
        }
        static public void SaveDao(BasicTestCaseDAO dao)
        {
            if (dao == null)
                return;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection<BasicTestCaseDAO> collection = db.GetCollection<BasicTestCaseDAO>(CollectionName);
            collection.Save(dao);
            connector.Close();
        }

        static public bool IsExist(string testCaseName)
        {
            if (string.IsNullOrWhiteSpace(testCaseName) == true)
            {
                return false;
            }
            bool ret = false;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoCollection<BasicTestCaseDAO> collection = db.GetCollection<BasicTestCaseDAO>(CollectionName);
            var query = new QueryDocument { { "TestCaseName", testCaseName } };
            var curst = collection.Find(query);
            if (curst.Count() == 0)
            {
                ret = false;
            }
            else
            {
                ret = true;
            }
            connector.Close();

            return ret;
        }

        static public void Remove(string testCaseName)
        {
            if (string.IsNullOrWhiteSpace(testCaseName) == true)
            {
                return;
            }
            BasicTestCaseDAO retDao = null;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();



            MongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>(CollectionName);
            var query = new QueryDocument { { "TestCaseName", testCaseName } };
            var curst = collection.Find(query);
            foreach (BsonDocument testCaseDAO in curst)
            {
                // Remove referd episodes
                MongoCollection<BasicTestCaseDAO> epCollect = db.GetCollection<BasicTestCaseDAO>(EpisodeCollectionName);
                var epQuery = new QueryDocument { { "TestCaseID", testCaseDAO["_id"] } };
                epCollect.Remove(epQuery);
            }
            collection.Remove(query);
            connector.Close();

        }

        #endregion


        // ===================================
        // Protect region
        #region protect region
        #endregion

        // ===========================
        // public region
        #region public region

        public void Save()
        {
            SaveDao(this);
        }

        public void AddEpisode(BasicTestEpisodeDAO dao)
        {
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection<BasicTestEpisodeDAO> collection = db.GetCollection<BasicTestEpisodeDAO>(EpisodeCollectionName);
            dao.TestCaseID = this._id;
            collection.Insert(dao);
            connector.Close();
        }
        public T[] GetAllEpisodes<T>()
        {
            T[] daoArr = null;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection<T> collection = db.GetCollection<T>(EpisodeCollectionName);
            var query = new QueryDocument { { "TestCaseID", _id } };
            var curst = collection.Find(query);
            long count = curst.Count();
            if ( count != 0 )
            {
                daoArr = new T[count];
                int i = 0;
                foreach (T dao in curst)
                {
                    daoArr[i++] = dao;
                }
            }
            connector.Close();

            return daoArr;
        }
        public void Remove()
        {
            RateMarketTestDAO.Remove(this.TestCaseName);
        }

        #endregion
        // =============================
        // Data for saving into database.
        #region Data section

        public ObjectId _id;
        public string TestCaseName { set; get; }

        #endregion

    }
}
