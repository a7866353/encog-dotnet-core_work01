using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyProject01.Agent;

namespace MyProject01.Reinforcement
{
    class QLearn : IRateMarketUser
    {
        private RateMarketAgentData PreviousState;

        public QLearn()
        {
            PreviousState = null;
        }

        public MarketActions Determine(RateMarketAgentData state)
        {
            return MarketActions.Nothing;
        }
    }
}
