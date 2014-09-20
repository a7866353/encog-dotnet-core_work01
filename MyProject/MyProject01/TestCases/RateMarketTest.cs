using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyProject01.Agent;
using System.Threading.Tasks;

namespace MyProject01.TestCases
{
    class RateMarketTest : BasicTestCase
    {

        public override void RunTest()
        {
            RateMarketAgent agent = new RateMarketAgent();
            agent.Run();
           
        }
    }
}
