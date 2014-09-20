using Encog.ML.Data;
using Encog.ML.Train;
using Encog.ML.Train.Strategy;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Anneal;
using Encog.Neural.Networks.Training.Lma;
using Encog.Neural.Networks.Training.Propagation.Back;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.TrainingMethods
{
    class BackpropagationTraining : BasicTrainingMethod
    {
        public override double TrainNetwork(BasicNetwork network, IMLDataSet trainingSet)
        {
            // train the neural network
            ICalculateScore score = new TrainingSetScore(trainingSet);
            IMLTrain trainAlt = new NeuralSimulatedAnnealing(network, score, 10, 2, 100);

            var trainMain = new Backpropagation(network, trainingSet);
            trainMain.ThreadCount = 16;
            var stop = new StopTrainingStrategy(0.00001, 10);
            trainMain.AddStrategy(new Greedy());
            trainMain.AddStrategy(new HybridStrategy(trainAlt));
            trainMain.AddStrategy(stop);

            int epoch = 0;
            while (!stop.ShouldStop())
            {
                trainMain.Iteration();
                LogFile.WriteLine("Epoch #" + epoch + " Error:" + trainMain.Error);
                //SaveNetworkToFile(network, testName);
                epoch++;
            }
            return trainMain.Error;
        }
    }
}
