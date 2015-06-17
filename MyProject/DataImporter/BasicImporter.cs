using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using MyProject01.DAO;

namespace DataImporter
{
    abstract class BasicImporter
    {
        protected string DatabaseName
        {
            get { return "MarketRateDB"; }
        }

        protected abstract string CollectiongName
        {
            get;
        }

        protected abstract BasicDataObject GetNextObject(string lineString);

        private int GetTotalLine(StreamReader sr)
        {
            long streamLen = sr.BaseStream.Length;
            int lineCount = 0;
            int oneBlockSize = 10*1024*1024;
            char[] oneBlock = new char[oneBlockSize];

            while (!sr.EndOfStream)
            {

                long leftLength = streamLen - sr.BaseStream.Position - 1;

                if (leftLength >= oneBlockSize)
                {

                    sr.ReadBlock(oneBlock, 0, oneBlock.Length);

                    lineCount += oneBlock.Count(c => c == '\r');

                }
                else
                {

                    lineCount += sr.ReadToEnd().Count(c => c == '\r');

                }

            }

            return lineCount;
        }

        public void Load(byte[] buffer)
        {
            MemoryStream ms = new MemoryStream(buffer);
            Load(ms);
            ms.Close();
        }

        public void Load(string filePath)
        {
            FileStream file = File.OpenRead(filePath);
            Load(file);
            file.Close();
        }
        private void Load(Stream stream)
        {
            // ----------------
            // Connect to db
            DatabaseConnector conn = new DatabaseConnector(DatabaseName);
            MongoCollection collection = conn.OpenCollectiong(CollectiongName);
            BasicDataObject insertObject = null;

            int batchBufferMax = 10000;
            List<BasicDataObject> objectList = new List<BasicDataObject>();

            _streamReader = new StreamReader(stream);
            while (true)
            {
                string lineStr = _streamReader.ReadLine();
                if (lineStr == null)
                    break;

                insertObject = GetNextObject(lineStr);
                if( insertObject == null)
                {
                    continue;
                }

                objectList.Add(insertObject);

                if(objectList.Count >= batchBufferMax)
                {
                    collection.InsertBatch<BasicDataObject>(objectList);
                    objectList.Clear();
                }
            }
            if(objectList.Count > 0)
                collection.InsertBatch<BasicDataObject>(objectList);

            conn.Close();
            stream.Close();
        }

        public double FinishRate
        {
            get
            {
                double rate = 0;
                Thread.BeginCriticalRegion();
                if (_streamReader == null)
                {
                    rate = 0;
                }
                else
                {
                    rate = (double)_streamReader.BaseStream.Position / _streamReader.BaseStream.Length;
                }
                Thread.EndCriticalRegion();

                return rate;

            }
        }


        private StreamReader _streamReader;
    }
}
