using Encog.Neural.NEAT;
using MyProject01.Agent;
using MyProject01.DAO;
using MyProject01.ExchangeRateTrade;
using MyProject01.Util.DataObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyProject01.Controller.Jobs;
using MyProject01.Util;
using Encog.Neural.Networks.Training;
using Encog.ML;

namespace MyProject01.Controller
{
    class NormalTrainer : Trainer
    {
        private TrainerContex _context;

        private BasicDataBlock _testDataBlock
        { get { return _context._testDataBlock; } }
        private BasicDataBlock _trainDataBlock
        { get { return _context._trainDataBlock; } }
        private int _trainDataLength
        { get { return _context._trainDataLength; } }


        protected override void PrepareRunnTestCase()
        {
            if (Controller == null)
            {
                throw (new Exception("Parm wrong!"));
            }

            // Init context
            _context = new TrainerContex();

            // Set test data ( Only one )
            TrainingData trainData = DataList.GetNext();
            _context.SetDataLength(trainData.DataBlock, trainData.TestLength);

            RateMarketScore score = new RateMarketScore();
            score.TradeDecisionCtrl = _decisionCtrl;
            score.SetData(_trainDataBlock);
            _context.TestScore = score;


            // train the neural network
            train = NEATUtil.ConstructNEATTrainer(Controller.GetPopulation(), score);
            _context.train = train;

        }

        protected override void PostItration()
        {
            CheckCtrl.Do(_context);

            // Update Test Data
                // Nothing
        }
    }

    public class RateMarketScore : ICalculateScore
    {
        private BasicDataBlock _dataBlock;
        public int StartIndex;
        public int Length;
        public ITradeDesisoin TradeDecisionCtrl;
        public void SetData(BasicDataBlock dataBlock)
        {
            _dataBlock = dataBlock;
            StartIndex = 0;
            Length = _dataBlock.Length;
        }

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
            RateMarketAgent agent = new RateMarketAgent(_dataBlock.GetNewBlock(StartIndex, Length));
            ITradeDesisoin decisionCtrl = TradeDecisionCtrl.Clone();
            decisionCtrl.UpdateNetwork((IMLRegression)network);
            TradeController tradeCtrl = new TradeController(agent, decisionCtrl);
            while (true)
            {
                if (agent.CurrentRateValue > 0)
                {
                    // Get Action Value
                    tradeCtrl.DoAction();
                }
                if (agent.IsEnd == true)
                    break;
            }
            //            System.Console.WriteLine("S: " + agent.CurrentValue);
            double score = agent.CurrentValue - agent.InitMoney;
            // System.Console.WriteLine("S: " + score);
            // return score;
            return agent.CurrentValue;
        }

    }
}
