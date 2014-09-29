using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.EA.Train;
using Encog.ML.Train.Strategy;
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
            IMLRegression reg = (IMLRegression)network;
            RateMarketAgentData stateData = agent.Reset();
            int maxActionIndex = -1;
            MarketActions currentAction;
            while(true)
            {
                if (agent.CurrentRateValue > 0)
                {
                    // Get Action Value
                    IMLData output = reg.Compute(new BasicMLData(stateData.RateDataArray));

                    // Choose an action
                    maxActionIndex = 0;
                    for (int i = 1; i < output.Count; i++)
                    {
                        if (output[maxActionIndex] < output[i])
                            maxActionIndex = i;
                    }

                    // Do action
                    switch (maxActionIndex)
                    {
                        case 0:
                            currentAction = MarketActions.Nothing;
                            break;
                        case 1:
                            currentAction = MarketActions.Buy;
                            break;
                        case 2:
                            currentAction = MarketActions.Sell;
                            break;
                        default:
                            currentAction = MarketActions.Nothing;
                            break;
                    }
                    stateData = agent.TakeAction(currentAction);
                }
                if (agent.Next() == false)
                    break;
            }

            return agent.CurrentValue();
        }

    }
    class LogFormater
    {
        private double[] _valueArray;
        public enum ValueName
        {
            Step = 0,
            Score,
        }

        public LogFormater()
        {
            _valueArray = new double[Enum.GetValues(typeof(ValueName)).Length];
        }

        public string GetTitle()
        {
            string title = "";
            string[] arr = Enum.GetNames(typeof(ValueName));
            for(int i=0;i<arr.Length;i++)
                title += arr[i].ToString() + "\t";
            return title;
        }

        public string GetLog()
        {
            string resStr = "";
            for(int i=0; i<_valueArray.Length;i++)
                resStr += _valueArray[i].ToString("G6") + "\t";
            return resStr;
        }

        public void Set(ValueName name, double v)
        {
            this._valueArray[(int)name] = v;
        }
        
    }
    class RateMarketNEATTest : BasicTestCase
    {
        public override void RunTest()
        {
            LogFormater log = new LogFormater();
            double errorLimit = 0.001;
            int toleratedCycles = 10;
            double targetErrorLimit = 0;
            StopTrainingStrategy stopStrategy = null;

            NEATPopulation pop = new NEATPopulation(30, 3, 500);
            pop.Reset();
            pop.InitialConnectionDensity = 1.0; // not required, but speeds processing.
            ICalculateScore score = new RateMarketScore();
            // train the neural network
            TrainEA train = NEATUtil.ConstructNEATTrainer(pop, score);

            int epoch = 1;

            LogFile.WriteLine(@"Beginning training...");
            LogFile.WriteLine(log.GetTitle());
            do
            {
                train.Iteration();
                if (stopStrategy == null)
                {
                    targetErrorLimit = train.Error * errorLimit;
                    stopStrategy = new StopTrainingStrategy(targetErrorLimit, toleratedCycles);
                    // stopStrategy.Init(train);
//                    train.AddStrategy(stopStrategy);
                }
                
                log.Set(LogFormater.ValueName.Step, epoch);
                log.Set(LogFormater.ValueName.Score, train.BestGenome.Score);  

                LogFile.WriteLine(log.GetLog());
                epoch++;
            } while ((stopStrategy.ShouldStop() == false) && !train.TrainingDone);
            train.FinishTraining();


            NEATNetwork network = (NEATNetwork)train.CODEC.Decode(train.BestGenome);
            
            // test the neural network
            LogFile.WriteLine(@"Training end");
        }
    }
}
