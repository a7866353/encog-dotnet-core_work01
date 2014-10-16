﻿using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.DAO
{
    class MarketRateDatabaseConnector
    {
        public static string DatabaseName = "MarketRateDB";
        public static string ConnectionString = @"mongodb://127.0.0.1";

        public MongoDatabase Database
        {
            get { return db; }
        }

        private MongoServer server;
        private MongoDatabase db;


        public MongoDatabase Connect()
        {
            server = MongoServer.Create(ConnectionString);
            if (null == server)
            {
                throw (new Exception("Cannot connect to server!"));
            }

            db = server.GetDatabase(DatabaseName); // Create a new Database or get a current Database
            if (null == db)
            {
                throw (new Exception("Cannot connect to server!"));
            }

            return db;
        }

        public void Close()
        {
            server.Disconnect();
            db = null;
        }
    }
    class TestCaseDatabaseConnector
    {
        public static string DatabaseName = "NetWorkTestDB";
        // public static string ConnectionString = @"mongodb://127.0.0.1";
        public static string ConnectionString = @"mongodb://192.168.1.11";

        public MongoDatabase Database
        {
            get { return db; }
        }

        private MongoServer server;
        private MongoDatabase db;


        public MongoDatabase Connect()
        {
            server = MongoServer.Create(ConnectionString);
            if (null == server)
            {
                throw (new Exception("Cannot connect to server!"));
            }

            db = server.GetDatabase(DatabaseName); // Create a new Database or get a current Database
            if (null == db)
            {
                throw (new Exception("Cannot connect to server!"));
            }

            return db;
        }

        public void Close()
        {
            server.Disconnect();
            db = null;
        }
    }
}
