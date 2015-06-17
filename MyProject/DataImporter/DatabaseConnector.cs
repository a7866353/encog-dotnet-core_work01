using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataImporter
{
    class DatabaseConnector
    {
        // private string connectionString = @"mongodb://127.0.0.1";
        private string connectionString = @"mongodb://192.168.1.15";

        private MongoServer _server;
        private MongoDatabase _database;

        public DatabaseConnector(string dbName)
        {
            _server = MongoServer.Create(connectionString);
            _database = _server.GetDatabase(dbName); // Create a new Database or get a current Database

        }

        public MongoCollection OpenCollectiong(string collectionName)
        {
            if (_database.CollectionExists(collectionName) == true)
                _database.DropCollection(collectionName);
            return _database.GetCollection(collectionName);

        }

        public void Close()
        {
           _server.Disconnect();
        } 
    }
}
