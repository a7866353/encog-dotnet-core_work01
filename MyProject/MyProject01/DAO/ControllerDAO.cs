using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.DAO
{
    class ControllerDAO
    {
        #region Static Region

        static public string CollectionName = "Controllers";
        static public ControllerDAO GetDAO(string name, bool isNew = false)
        {
            if (string.IsNullOrWhiteSpace(name) == true)
            {
                return null;
            }
            if (isNew == true)
            {
                if (RateMarketTestDAO.IsExist(name) == true)
                    RateMarketTestDAO.Remove(name);
            }
            ControllerDAO retDao;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoCollection<ControllerDAO> collection = db.GetCollection<ControllerDAO>(CollectionName);
            var query = new QueryDocument { { "Name", name } };
            var curst = collection.Find(query);
            if (curst.Count() == 0)
            {
                // not existed.
                retDao = new ControllerDAO();
                retDao.Name = name;
                collection.Save(retDao);
            }
            else
            {
                retDao = curst.FirstOrDefault();
            }
            connector.Close();
            return retDao;
        }
        static public ControllerDAO[] GetAllDAOs<ControllerDAO>()
        {

            ControllerDAO[] retDaoArr = null;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoCollection<ControllerDAO> collection = db.GetCollection<ControllerDAO>(CollectionName);
            var curst = collection.FindAll();
            long resultCount = curst.Count();
            if (resultCount != 0)
            {
                retDaoArr = new ControllerDAO[resultCount];
                int index = 0;
                foreach (ControllerDAO obj in curst)
                {
                    retDaoArr[index++] = obj;
                }
            }
            connector.Close();
            return retDaoArr;
        }
        static public void SaveDao(ControllerDAO dao)
        {
            if (dao == null)
                return;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection<ControllerDAO> collection = db.GetCollection<ControllerDAO>(CollectionName);
            collection.Save(dao);
            connector.Close();
        }

        static public bool IsExist(string controllerName)
        {
            if (string.IsNullOrWhiteSpace(controllerName) == true)
            {
                return false;
            }
            bool ret = false;
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoCollection<BasicTestCaseDAO> collection = db.GetCollection<BasicTestCaseDAO>(CollectionName);
            var query = new QueryDocument { { "Name", controllerName } };
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

        static public void Remove(string name)
        {
            if (string.IsNullOrWhiteSpace(name) == true)
            {
                return;
            }
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();
            MongoCollection<BsonDocument> collection = db.GetCollection<BsonDocument>(CollectionName);
            var query = new QueryDocument { { "Name", name } };
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


        public void Remove()
        {
            ControllerDAO.Remove(this.Name);
        }

        #endregion
        // =============================
        // Data for saving into database.
        #region Data section

        public ObjectId _id;
        public string Name { set; get; }
        public DateTime UpdateTime { set; get; }
        public byte[] Data { set; get; }

        #endregion

        

    }
}
