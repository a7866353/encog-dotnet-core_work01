using Encog.Engine.Network.Activation;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Layers;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Util.Simple;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MyProject01.Test
{
    class FeedForwardNetworkTest
    {
        private LogWriter logger;
        private LogWriter resultLog;
        private string networkFileName = "network.bin";

        public FeedForwardNetworkTest()
        {
            // init log
            logger = new LogWriter("log.txt");
            resultLog = new LogWriter("Results.txt");
            string testName = "FeedFowardNetwork";
            int num = 1;
            NetworkTestParameter[] parmArr = new NetworkTestParameter[]
            {
                new NetworkTestParameter(testName+num++.ToString("D2"),0.03,  0.01,   500),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.03,  0.1,    500),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.03,  1,      500),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.03,  10,     500),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.03,  100,    500),
                // new NetworkParm(testName+num++.ToString("D2"),0.03,  1000,   500),
                // new NetworkParm("Test04",0.03,  10000,  100),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.01,  0.01,   500),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.01,  0.1,    500),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.01,  1,      500),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.01,  10,     500),
                // new NetworkParm(testName+num++.ToString("D2"),0.01,  100,    500),
                // new NetworkParm(testName+num++.ToString("D2"),0.01,  1000,   500),
                // new NetworkParm("Test08",0.01,  10000,  100),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.005, 0.01,   1000),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.005, 0.1,    1000),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.005, 1,      1000),
                new NetworkTestParameter(testName+num++.ToString("D2"),0.005, 10,     1000),
                 new NetworkTestParameter(testName+num++.ToString("D2"),0.0001, 100,     1000),
               // new NetworkParm(testName+num++.ToString("D2"),0.005, 100,    500),
                // new NetworkParm(testName+num++.ToString("D2"),0.005, 1000,   500),
                // new NetworkParm("Test12",0.005, 10000,  100),
            };
            foreach (NetworkTestParameter parm in parmArr)
            {
                Execute(parm);
            }
        }


        /// <summary>
        /// Program entry point.
        /// </summary>
        /// <param name="app">Holds arguments and other info.</param>
        public void Execute(NetworkTestParameter parm)
        {
            logger.SetFileName(parm.name + "_log.txt");
            logger.Reset();
            resultLog.SetFileName(parm.name + "_result.txt");
            resultLog.Reset();

            ResultPrintf(@"------------------------");
            LogPrintf("Test Start!" + parm.name);

            RateDataCreator dataCreator = new RateDataCreator();
            TestData data = dataCreator.GetTestData();

            if (false)
            {
                LogPrintf("[TestData]");
                string dataStr = "";
                for (int i = 0; i < data.TrainInputs.Length; i++)
                {
                    dataStr = i.ToString("D4") + ":";
                    for (int j = 0; j < data.InputSize; j++)
                    {
                        dataStr += data.TrainInputs[i][j].ToString("0.000");
                        dataStr += ", ";
                    }
                    dataStr += "| ";

                    for (int j = 0; j < data.OutputSize; j++)
                    {
                        dataStr += data.TrainIdeaOutputs[i][j].ToString("0.000");
                        dataStr += ", ";
                    }
                    LogPrintf(dataStr);
                }
            }

            BasicNetwork network = null;
            if( true )
            {
                // Create a new trained netwrok
                network = CreateNetwork(data,parm);
                // network = CreateNetwork02(data, parm);
            }
            else
            {
                // Create network from file.
                network = LoadNetwrokFromFile(parm.name);
            }

            if (null == network)
                throw (new Exception("Empty Network."));

            // test the neural network
            //   test train data
            foreach(MyTestData dataObj in data.TrainList)
            {
                IMLData res = network.Compute(new BasicMLData(dataObj.Input));
                double[] realArr = new double[data.OutputSize];
                res.CopyTo(realArr, 0, data.OutputSize);
                dataObj.Real = realArr;
            }

            //   test test data
            foreach (MyTestData dataObj in data.TestList)
            {
                IMLData res = network.Compute(new BasicMLData(dataObj.Input));
                double[] realArr = new double[data.OutputSize];
                res.CopyTo(realArr, 0, data.OutputSize);
                dataObj.Real = realArr;
            }


            // Output results.
            int loopCnt = 1;
            ResultPrintf("------------------------");
            ResultPrintf("Neural Network Results:");
            ResultPrintf("ErrorLimit:\t"+parm.errorlimit.ToString() +"\tNerul:\t" + parm.hidenLayerRaio.ToString() );
            ResultPrintf("TotalError:\t" + data.TestList.ResultError.ToString("000.0000"));
            ResultPrintf(data.ToStringResults());
            LogPrintf("Test end!");
            LogPrintf("");
        }

        public BasicNetwork CreateNetwork(TestData data, NetworkTestParameter parm)
        {
            LogPrintf("[NetworkCreate]"+parm.ToString());
            // create a neural network, without using a factory
            var network = new BasicNetwork();
            network.AddLayer(new BasicLayer(null, true, data.InputSize));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, (int)(data.InputSize * parm.hidenLayerRaio)));
            network.AddLayer(new BasicLayer(new ActivationSigmoid(), true, data.OutputSize));
            network.Structure.FinalizeStructure();
            network.Reset();

            // create training data
            IMLDataSet trainingSet = new BasicMLDataSet(data.TrainInputs, data.TrainIdeaOutputs);

            // train the neural network
            var train = new ResilientPropagation(network, trainingSet);
            train.ThreadCount = 8;

            int epoch = 1;
            SlidingFilter filter = new SlidingFilter(5);
            do
            {
                train.Iteration();
                // if(epoch%1 == 1 )
                LogPrintf(@"Epoch #" + epoch + @" Error:" + train.Error);
                // Save network each train.
                SaveNetworkToFile(network,parm.name);
                epoch++;
                if (epoch > parm.retryCnt)
                    break;
                if(filter.Add(train.Error) < parm.errorlimit)
                    break;
            } while (true);
            train.FinishTraining();
            SaveNetworkToFile(network, parm.name);

            return network;
        }
        // For SimpleFeedForward
        public BasicNetwork CreateNetwork02(TestData data, NetworkTestParameter parm)
        {
            LogPrintf("[NetworkCreate]" + parm.ToString());
            ResultPrintf("[NetworkCreate]" + parm.ToString());
            // create a network
            BasicNetwork network = EncogUtility.SimpleFeedForward(
                data.InputSize,
                (int)(data.InputSize * parm.hidenLayerRaio),
                (int)(data.OutputSize * 10),
                data.OutputSize,
                true);
            // create training data
            IMLDataSet trainingSet = new BasicMLDataSet(data.TrainInputs, data.TrainIdeaOutputs);

            // train the neural network
            // var train = new Backpropagation(network, trainingSet);
            var train = new ResilientPropagation(network, trainingSet);
            train.ThreadCount = 8;

            int epoch = 1;
            SlidingFilter filter = new SlidingFilter(5);
            do
            {
                train.Iteration();
                // if(epoch%1 == 1 )
                LogPrintf(@"Epoch #" + epoch + @" Error:" + train.Error);
                // Save network each train.
                SaveNetworkToFile(network, parm.name);
                epoch++;
                if (epoch > parm.retryCnt)
                    break;
                if (filter.Add(train.Error) < parm.errorlimit)
                    break;
            } while (true);
            train.FinishTraining();
            SaveNetworkToFile(network, parm.name);

            return network;
        }

        // Save network class to file.
        private void SaveNetworkToFile(BasicNetwork network, string filename)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filename+".net", FileMode.Create);
            formatter.Serialize(stream, network);
            stream.Flush();
            stream.Close();
        }
        private BasicNetwork LoadNetwrokFromFile(string filename)
        {
            if (File.Exists(networkFileName) == false)
                return null;

            BasicNetwork network;
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filename+".net", FileMode.Open);
            network = (BasicNetwork)formatter.Deserialize(stream);
            stream.Close();

            return network;
        }
        private void LogPrintf(string str)
        {
            logger.WriteLine(str);
            LogFile.WriteLine(str);
        }
        private void ResultPrintf(string str)
        {
            resultLog.WriteLine(str);
        }

    }
}
