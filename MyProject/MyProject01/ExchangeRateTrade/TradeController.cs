using Encog.ML;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.Neural.NEAT;
using MyProject01.Agent;
using MyProject01.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyProject01.ExchangeRateTrade
{
    class TradeController
    {
        private RateMarketAgent _agent;
        private IMLRegression _network;
        private RateMarketAgentData stateData;
        private MarketActions _currentAction;

        public RateMarketAgent Agent
        {
            get { return _agent; }
        }
        public MarketActions LastAction
        {
            get { return _currentAction; }
        }

        public TradeController(RateMarketAgent agent, IMLRegression network)
        {
            _agent = agent;
            _network = network;
            stateData = _agent.Reset();
        }
        public void DoAction()
        {
            _currentAction = MarketActions.Init;
            if (_agent.IsEnd == true)
                return;

            if (_agent.CurrentRateValue > 0)
            {
                // Get Action Value
                _currentAction = ChoseAction(stateData.RateDataArray);
                stateData = _agent.TakeAction(_currentAction);
                _agent.Next();
            }
        }

        private MarketActions ChoseAction(double[] rateDataArray)
        {
            MarketActions currentAction;
            IMLData output = _network.Compute(new BasicMLData(rateDataArray, false));

            // Choose an action
            int maxActionIndex = 0;
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
            return currentAction;
        }
    }
}
