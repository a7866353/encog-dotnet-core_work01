using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyProject01.Agent;

namespace MyProject01.Reinforcement
{
    class QLearn : IRateMarketUser
    {
        public MarketActions Determine(RateMarketAgentData inputData)
        {
            return MarketActions.Nothing;
        }
    }
}
