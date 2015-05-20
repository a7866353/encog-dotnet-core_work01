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
using MyProject01.Factorys.PopulationFactorys;

namespace MyProject01.Controller
{
    class NormalTrainer : Trainer
    {
        private TrainerContex _context;
        private RateMarketTestDAO _testDAO;

        public ITradeDesisoin DecisionCtrl;
        public BasicDataBlock TrainDataBlock;
        public BasicPopulationFactory PopulationFacotry;

        protected override void PrepareRunnTestCase()
        {
            // Init context
            _context = new TrainerContex();

            RateMarketScore score = new RateMarketScore();
            score.TradeDecisionCtrl = DecisionCtrl;
            score.SetData(TrainDataBlock);
            _context.TestScore = score;
            _context._trainDataBlock = TrainDataBlock;

            // train the neural network
            train = NEATUtil.ConstructNEATTrainer(
                PopulationFacotry.Get(DecisionCtrl.NetworkInputVectorLength, DecisionCtrl.NetworkOutputVectorLenth), 
                score );
            _context.train = train;

        }

        protected override void PostItration()
        {
            _context.Epoch = _epoch;
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
            Length = _dataBlock.BlockCount;
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
