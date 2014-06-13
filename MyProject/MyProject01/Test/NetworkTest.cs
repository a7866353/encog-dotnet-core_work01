using Encog.Neural.Networks;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MyProject01.Test
{
    class NetworkTest
    {
        protected LogWriter logger;
        protected LogWriter resultLog;
        public NetworkTest()
        {
            // init log
            logger = new LogWriter("log.txt");
            resultLog = new LogWriter("Results.txt");
        }
        // Save network class to file.
        protected void SaveNetworkToFile(BasicNetwork network, string filename)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filename + ".net", FileMode.Create);
            formatter.Serialize(stream, network);
            stream.Flush();
            stream.Close();
        }
        protected BasicNetwork LoadNetwrokFromFile(string filename)
        {
            string filePath = filename + ".net";
            if (File.Exists(filePath) == false)
                return null;

            BasicNetwork network;
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filePath, FileMode.Open);
            network = (BasicNetwork)formatter.Deserialize(stream);
            stream.Close();

            return network;
        }
        protected void LogPrintf(string str)
        {
            logger.WriteLine(str);
            LogFile.WriteLine(str);
        }
        protected void ResultPrintf(string str)
        {
            resultLog.WriteLine(str);
        }
    }
}
