using Encog.ML;
using Encog.Neural.NEAT;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MyProject01.Controller;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.DAO
{
    public class ControllerDAO
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
            dao.UpdateTime = DateTime.Now;
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

            MongoGridFS fs = new MongoGridFS(db);
            fs.Delete(name);

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

        public NEATPopulation GetPopulation()
        {
            NEATPopulation pop = MongoDBUtility.GetFromFS<NEATPopulation>(new TestCaseDatabaseConnector(), Name);
            return pop;

        }

        public void UpdatePopulation(NEATPopulation pop)
        {
            MongoDBUtility.SaveToFS<NEATPopulation>(new TestCaseDatabaseConnector(), pop, Name);
        }

        public NEATNetwork GetBestNetwork()
        {
            if (BestNetwork == null)
                return null;

            MemoryStream stream = new MemoryStream(BestNetwork);
            BinaryFormatter formatter = new BinaryFormatter();
            NEATNetwork obj = (NEATNetwork)formatter.Deserialize(stream);

            return obj;
        }

        public void SetBestNetwork(IMLRegression net)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, net);
            BestNetwork = stream.ToArray();
            stream.Close();

        }

        public void SetTradeDecisionController(ITradeDesisoin ctrl)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, ctrl);
            TradeDecisionController = stream.ToArray();
            stream.Close();
        }
        public ITradeDesisoin GetTradeDecisionController()
        {
            if (BestNetwork == null)
                return null;

            MemoryStream stream = new MemoryStream(TradeDecisionController);
            BinaryFormatter formatter = new BinaryFormatter();
            ITradeDesisoin obj = (ITradeDesisoin)formatter.Deserialize(stream);

            return obj;
        }
        #endregion
        // =============================
        // Data for saving into database.
        #region Data section

        public ObjectId _id;
        public string Name { set; get; }
        public DateTime UpdateTime { set; get; }
        public int InputDataLength { set; get; }
        public InputDataFormaterType InputType { set; get; }
        public OutputDataConvertorType OutType { set; get; }
        public double DataOffset { set; get; }
        public double DataScale { set; get; }
        public byte[] BestNetwork { set; get; }

        public byte[] TradeDecisionController { set; get; }
        

        #endregion

        

    }
}
