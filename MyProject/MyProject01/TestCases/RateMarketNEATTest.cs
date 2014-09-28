using Encog.ML;
using Encog.ML.Data;
using Encog.ML.EA.Train;
using Encog.Neural.NEAT;
using Encog.Neural.Networks.Training;
using Encog.Util;
using Encog.Util.Banchmark;
using Encog.Util.Simple;
using MyProject01.Agent;
using MyProject01.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyProject01.TestCases
{
    class NeatRateMarketUser : IRateMarketUser
    {

        public MarketActions Determine(RateMarketAgentData inputData)
        {
            throw new NotImplementedException();
        }

        public double TotalErrorRate
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void EpsodeEnd()
        {
            throw new NotImplementedException();
        }
    }
    public class RateMarketScore : ICalculateScore
    {

        public bool ShouldMinimize
        {
            get { return false; }
        }
        public bool RequireSingleThreaded
        {
            get { return false; }
        }

        public double CalculateScore(IMLMethod network)
        {
            RateMarketAgent agent = new RateMarketAgent();
        }

    }
    class RateMarketNEATTest : BasicTestCase
    {


        public override void RunTest()
        {

            double errorLimit = 0.01;

            IMLDataSet trainingSet = RandomTrainingFactory.Generate(1000, 1500,
                                         30, 3, -1, 1);

            double targetErrorLimit = 0;
            for (int i = 0; i < trainingSet[0].Ideal.Count; i++)
                targetErrorLimit += Math.Abs(trainingSet[0].Ideal[i]);
            targetErrorLimit /= trainingSet[0].Ideal.Count;
            targetErrorLimit *= errorLimit;

            NEATPopulation pop = new NEATPopulation(30, 3, 100);
            pop.Reset();
            pop.InitialConnectionDensity = 1.0; // not required, but speeds processing.
            ICalculateScore score = new TrainingSetScore(trainingSet);
            // train the neural network
            TrainEA train = NEATUtil.ConstructNEATTrainer(pop, score);

            int epoch = 1;
            LogFile.WriteLine(@"Beginning training...");
            do
            {
                train.Iteration();

                LogFile.WriteLine(@"Iteration #" + Format.FormatInteger(epoch)
                         + @" Error:" + Format.FormatPercent(train.Error)
                         + @" Target Error: " + Format.FormatPercent(targetErrorLimit));
                epoch++;
            } while ((train.Error > targetErrorLimit) && !train.TrainingDone);
            train.FinishTraining();


            NEATNetwork network = (NEATNetwork)train.CODEC.Decode(train.BestGenome);

            // test the neural network
            LogFile.WriteLine(@"Neural Network Results:");
            EncogUtility.Evaluate(network, trainingSet);
        }
    }
}
