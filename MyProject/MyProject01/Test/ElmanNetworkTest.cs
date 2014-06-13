using Encog.Engine.Network.Activation;
using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Anneal;
using Encog.Neural.Networks.Training.Lma;
using Encog.Neural.Networks.Training.Propagation.Back;
using Encog.Neural.Pattern;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.Test
{
    class ElmanNetworkTest : NetworkTest
    {
        private string testName = "ElmanNetwork";
        NetworkTestParameter parm;
        public ElmanNetworkTest()
        {
            int num = 1;
            NetworkTestParameter[] parmArr = new NetworkTestParameter[]
            {

                new NetworkTestParameter(testName+num++.ToString("D2"), 50),
                /*
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
                */
            };
            this.parm = new NetworkTestParameter(null, 1);
            for (int i = 1; i < 50; i++)
            {
                parm.name = testName + num++.ToString("D2");
                parm.hidenLayerNum = i;
                Execute();
            }
            /*
            foreach (NetworkTestParameter parm in parmArr)
            {
                this.parm = parm;
                Execute();
            }
            */
        }
        public void Execute()
        {
            RateDataCreator dataCreator = new RateDataCreator();
            TestData data = dataCreator.GetTestData();
            IMLDataSet trainingSet = new BasicMLDataSet(data.TrainInputs, data.TrainIdeaOutputs);

         
            var elmanNetwork = (BasicNetwork)CreateElmanNetwork(trainingSet.InputSize);
            // var feedforwardNetwork = (BasicNetwork)CreateFeedforwardNetwork(trainingSet.InputSize);

            double elmanError = TrainNetwork("Elman", elmanNetwork, trainingSet, "Leven");
            // double feedforwardError = TrainNetwork("Feedforward", feedforwardNetwork, trainingSet, "Leven");

            LogPrintf("Best error rate with Elman Network: " + elmanError);
            // LogPrintf("Best error rate with Feedforward Network: " + feedforwardError);
            LogPrintf("(Elman should outperform feed forward)");
            LogPrintf("If your results are not as good, try rerunning, or perhaps training longer.");
            // test the neural network
            //   test train data
            foreach (MyTestData dataObj in data.TrainList)
            {
                IMLData res = elmanNetwork.Compute(new BasicMLData(dataObj.Input));
                double[] realArr = new double[data.OutputSize];
                res.CopyTo(realArr, 0, data.OutputSize);
                dataObj.Real = realArr;
            }

            //   test test data
            foreach (MyTestData dataObj in data.TestList)
            {
                IMLData res = elmanNetwork.Compute(new BasicMLData(dataObj.Input));
                double[] realArr = new double[data.OutputSize];
                res.CopyTo(realArr, 0, data.OutputSize);
                dataObj.Real = realArr;
            }


            // Output results.
            int loopCnt = 1;
            ResultPrintf("------------------------");
            ResultPrintf("Neural Network Results:");
            ResultPrintf("hidenLayerNum:\t" + parm.hidenLayerNum.ToString());
            ResultPrintf("TotalError:\t" + data.TestList.ResultError.ToString("000.0000"));
            ResultPrintf(data.ToStringResults());
            LogPrintf("Test end!");
            LogPrintf("");
        }

    
        private IMLMethod CreateElmanNetwork(int input)
        {
            // construct an Elman type network
            var pattern = new ElmanPattern
            {
                ActivationFunction = new ActivationSigmoid(),
                InputNeurons = input
            };
            pattern.AddHiddenLayer(parm.hidenLayerNum);
            pattern.OutputNeurons = 1;
            return pattern.Generate();
        }

        private IMLMethod CreateFeedforwardNetwork(int input)
        {
            // construct a feedforward type network
            var pattern = new FeedForwardPattern();
            pattern.ActivationFunction = new ActivationSigmoid();
            pattern.InputNeurons = input;
            pattern.AddHiddenLayer(parm.hidenLayerNum);
            pattern.OutputNeurons = 1;
            return pattern.Generate();
        }

        private double TrainNetwork(String what, BasicNetwork network, IMLDataSet trainingSet, string Method)
        {
            // train the neural network
            ICalculateScore score = new TrainingSetScore(trainingSet);
            IMLTrain trainAlt = new NeuralSimulatedAnnealing(network, score, 10, 2, 100);
            IMLTrain trainMain;
            if (Method.Equals("Leven"))
            {
                Console.WriteLine("Using LevenbergMarquardtTraining");
                trainMain = new LevenbergMarquardtTraining(network, trainingSet);
            }
            else
                trainMain = new Backpropagation(network, trainingSet);

            var stop = new StopTrainingStrategy(0.00001, 10);
            trainMain.AddStrategy(new Greedy());
            trainMain.AddStrategy(new HybridStrategy(trainAlt));
            trainMain.AddStrategy(stop);

            int epoch = 0;
            while (!stop.ShouldStop())
            {
                trainMain.Iteration();
                LogPrintf("Training " + what + ", Epoch #" + epoch + " Error:" + trainMain.Error);
                SaveNetworkToFile(network, testName);
                epoch++;
            }
            return trainMain.Error;
        }

    }
}
