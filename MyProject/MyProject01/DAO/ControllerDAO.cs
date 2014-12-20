﻿using Encog.Neural.NEAT;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoGridFS fs = new MongoGridFS(db);
            MongoGridFSStream file = fs.OpenRead(Name);
            if (file.Length == 0)
                return null;

            byte[] buffer = new byte[file.Length];
            file.Read(buffer, 0, buffer.Length);
            file.Close();

            connector.Close();

            MemoryStream stream = new MemoryStream(buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            NEATPopulation pop = (NEATPopulation)formatter.Deserialize(stream);
            stream.Close();

            return pop;

        }

        public void UpdatePopulation(NEATPopulation pop)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, pop);
            byte[] res = stream.ToArray();
            stream.Close();

            TestCaseDatabaseConnector connector = new TestCaseDatabaseConnector();
            MongoDatabase db = connector.Connect();

            MongoGridFS fs = new MongoGridFS(db);
            MongoGridFSStream file = fs.Create(Name);
            file.Write(res, 0, res.Length);
            file.Close();

            connector.Close();
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

        public void SetBestNetwork(NEATNetwork net)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, net);
            BestNetwork = stream.ToArray();
            stream.Close();

        }
        #endregion
        // =============================
        // Data for saving into database.
        #region Data section

        public ObjectId _id;
        public string Name { set; get; }
        public DateTime UpdateTime { set; get; }
        public int InputVectorLength { set; get; }
        public int OutputVectorLength { set; get; }
        public int PopulationNumeber { set; get; }
        public byte[] BestNetwork { set; get; }

        

        #endregion

        

    }
}