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
        private ITradeDesisoin _decisionCtl;
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

        public TradeController(RateMarketAgent agent, ITradeDesisoin decision)
        {
            _agent = agent;
            _decisionCtl = decision;
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
            MarketActions currentAction = _decisionCtl.GetAction(rateDataArray);
            return currentAction;
        }
    }
}
